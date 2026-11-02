namespace FamilyQuiz.Core.Commands
{
    /// <summary>
    /// The TV app lost its relay socket. Every phone counts as disconnected until the relay reports its
    /// socket again ("connected" after the TV reconnects, PROTOCOL.md section 2) or the phone rejoins.
    /// </summary>
    public sealed class RelayConnectionLost : IGameCommand
    {
    }
}
