namespace FamilyQuiz.Core.Commands
{
    /// <summary>The relay reports an open phone socket. Conn is an opaque id made by the relay.</summary>
    public sealed class ConnectionOpened : IGameCommand
    {
        public ConnectionOpened(string conn)
        {
            Conn = conn;
        }

        public string Conn { get; }
    }
}
