namespace FamilyQuiz.Presentation
{
    /// <summary>One attack the TV draws as an arrow (D149): attacker to target in the power up's colour.</summary>
    public sealed class AttackArrow
    {
        public AttackArrow(string attackerId, string targetId, string powerUp, int lane)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            PowerUp = powerUp;
            Lane = lane;
        }

        public string AttackerId { get; }
        public string TargetId { get; }
        public string PowerUp { get; }

        /// <summary>How many earlier arrows join the same two players: each lane arcs further, so none lie on each other.</summary>
        public int Lane { get; }

        /// <summary>Identifies the arrow between redraws, so only a new one flies in.</summary>
        public string Key => AttackerId + ">" + TargetId + ":" + PowerUp + ":" + Lane;
    }
}
