namespace FamilyQuiz.Net
{
    /// <summary>Sends one text frame to the relay. Dropped while the socket is not open.</summary>
    public interface IRelaySender
    {
        void Send(string json);
    }
}
