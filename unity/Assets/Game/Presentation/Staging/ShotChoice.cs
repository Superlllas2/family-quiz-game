using System;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>A shot, and for HostSolo the host it frames.</summary>
    public readonly struct ShotChoice : IEquatable<ShotChoice>
    {
        public ShotChoice(Shot shot, string host)
        {
            Shot = shot;
            Host = shot == Shot.HostSolo ? host : null;
        }

        public Shot Shot { get; }

        /// <summary>The host HostSolo frames; null for every other shot.</summary>
        public string Host { get; }

        public static ShotChoice Of(Shot shot) => new ShotChoice(shot, null);

        public static ShotChoice Solo(string host) => new ShotChoice(Shot.HostSolo, host);

        public bool Equals(ShotChoice other) => Shot == other.Shot && Host == other.Host;

        public override bool Equals(object obj) => obj is ShotChoice other && Equals(other);

        public override int GetHashCode() => ((int)Shot * 397) ^ (Host != null ? Host.GetHashCode() : 0);

        public override string ToString() => Host == null ? Shot.ToString() : Shot + "(" + Host + ")";
    }
}
