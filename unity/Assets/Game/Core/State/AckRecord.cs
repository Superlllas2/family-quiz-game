namespace FamilyQuiz.Core.State
{
    /// <summary>The first answer given to an inputId, kept so a resend gets the same ack.</summary>
    public sealed class AckRecord
    {
        public AckRecord(bool ok, string reason)
        {
            Ok = ok;
            Reason = reason;
        }

        public bool Ok { get; }
        public string Reason { get; }
    }
}
