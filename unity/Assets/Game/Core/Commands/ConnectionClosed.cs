namespace FamilyQuiz.Core.Commands
{
    /// <summary>The relay reports a closed phone socket.</summary>
    public sealed class ConnectionClosed : IGameCommand
    {
        public ConnectionClosed(string conn)
        {
            Conn = conn;
        }

        public string Conn { get; }
    }
}
