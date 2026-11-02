using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FamilyQuiz.Core;

namespace FamilyQuiz.Net
{
    /// <summary>
    /// The TV app's socket to the relay (PROTOCOL.md section 2), on a background task.
    /// One socket at a time: the loop opens a socket, reads it until it closes, then waits
    /// net.tvReconnectSeconds and opens the next one with the same room code and secret. So only the
    /// current socket closing ever leads to a reconnect; a replaced socket is already gone.
    /// Close code 4001 (wrong secret) makes a new room code and secret first.
    /// Everything it receives goes into the MessageQueue for the main thread.
    /// </summary>
    public sealed class RelayClient : IRelaySender, IDisposable
    {
        private const int ReceiveBufferBytes = 8192;
        private static readonly TimeSpan CloseHandshakeTimeout = TimeSpan.FromSeconds(2);

        private readonly RelayEndpoint _endpoint;
        private readonly MessageQueue _inbox;
        private readonly IRandom _random;
        private readonly TimeSpan _reconnectDelay;
        private readonly Action<string> _log;
        private readonly ConcurrentQueue<string> _outgoing = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim _outgoingSignal = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private volatile RoomCredentials _room;
        private volatile bool _open;
        private Task _loop;

        public RelayClient(RelayEndpoint endpoint, MessageQueue inbox, IRandom random, double reconnectSeconds, Action<string> log)
        {
            _endpoint = endpoint;
            _inbox = inbox;
            _random = random;
            _reconnectDelay = TimeSpan.FromSeconds(reconnectSeconds);
            _log = log ?? (_ => { });
            _room = RoomCredentials.Create(random);
        }

        /// <summary>The room code and secret the next (or current) socket uses.</summary>
        public RoomCredentials Room => _room;

        public bool IsOpen => _open;

        public void Start()
        {
            if (_loop != null) return;
            _loop = Task.Run(() => RunAsync(_stop.Token));
        }

        /// <summary>Queues a text frame. Dropped while the socket is not open (the relay would drop it too).</summary>
        public void Send(string json)
        {
            if (!_open) return;
            _outgoing.Enqueue(json);
            _outgoingSignal.Release();
        }

        public void Dispose()
        {
            if (_stop.IsCancellationRequested) return;
            _stop.Cancel();
            try
            {
                _loop?.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException)
            {
                // The loop ends with a cancellation; nothing to report on shutdown.
            }
        }

        private async Task RunAsync(CancellationToken stop)
        {
            while (!stop.IsCancellationRequested)
            {
                int? closeCode = null;
                string detail = null;
                var room = _room;
                using (var socket = new ClientWebSocket())
                using (var session = CancellationTokenSource.CreateLinkedTokenSource(stop))
                {
                    try
                    {
                        await socket.ConnectAsync(_endpoint.TvSocket(room), stop).ConfigureAwait(false);
                        ClearOutgoing();
                        _open = true;
                        _inbox.Enqueue(RelayInboxItem.Opened(room.Code));
                        var sender = SendLoopAsync(socket, session.Token);
                        closeCode = await ReceiveLoopAsync(socket, stop).ConfigureAwait(false);
                        detail = socket.CloseStatusDescription;
                        session.Cancel();
                        await sender.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (stop.IsCancellationRequested)
                    {
                        socket.Abort();
                        _open = false;
                        return;
                    }
                    catch (Exception e)
                    {
                        detail = e.GetType().Name + ": " + e.Message;
                    }
                    finally
                    {
                        _open = false;
                        session.Cancel();
                    }
                }

                if (stop.IsCancellationRequested) return;
                _inbox.Enqueue(RelayInboxItem.Closed(closeCode, detail));
                if (closeCode == ProtocolConstants.CloseWrongSecret)
                {
                    _room = RoomCredentials.Create(_random);
                    _log("Relay refused the secret for room " + room.Code + " (4001); new room " + _room.Code);
                }

                try
                {
                    await Task.Delay(_reconnectDelay, stop).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private async Task<int?> ReceiveLoopAsync(ClientWebSocket socket, CancellationToken stop)
        {
            var buffer = new byte[ReceiveBufferBytes];
            using (var message = new MemoryStream())
            {
                while (socket.State == WebSocketState.Open)
                {
                    message.SetLength(0);
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), stop).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await FinishCloseAsync(socket).ConfigureAwait(false);
                            return (int?)result.CloseStatus;
                        }

                        message.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
                        _inbox.Enqueue(RelayInboxItem.Message(text));
                    }
                }
            }

            return (int?)socket.CloseStatus;
        }

        private async Task SendLoopAsync(ClientWebSocket socket, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _outgoingSignal.WaitAsync(token).ConfigureAwait(false);
                    while (_outgoing.TryDequeue(out var json))
                    {
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // The session ended.
            }
            catch (Exception e)
            {
                // A failed send means the socket is broken: abort it so the receive loop ends and we reconnect.
                _log("Relay send failed: " + e.Message);
                socket.Abort();
            }
        }

        private static async Task FinishCloseAsync(ClientWebSocket socket)
        {
            try
            {
                using (var timeout = new CancellationTokenSource(CloseHandshakeTimeout))
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, timeout.Token)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // The relay is gone already; the socket is closed either way.
            }
        }

        private void ClearOutgoing()
        {
            while (_outgoing.TryDequeue(out _))
            {
            }

            while (_outgoingSignal.CurrentCount > 0) _outgoingSignal.Wait(0);
        }
    }
}
