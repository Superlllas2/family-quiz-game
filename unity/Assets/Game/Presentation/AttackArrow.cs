namespace FamilyQuiz.Presentation
{
    /// <summary>One attack the TV draws as an arrow (D149): attacker to target in the power up's colour.</summary>
    public sealed class AttackArrow
    {
        public AttackArrow(string attackerId, string targetId, string powerUp, int lane, int attackerEnd = 0,
            int attackerEnds = 1, int targetEnd = 0, int targetEnds = 1)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            PowerUp = powerUp;
            Lane = lane;
            AttackerEnd = attackerEnd;
            AttackerEnds = attackerEnds;
            TargetEnd = targetEnd;
            TargetEnds = targetEnds;
        }

        public string AttackerId { get; }
        public string TargetId { get; }
        public string PowerUp { get; }

        /// <summary>How many earlier arrows join the same two players: each lane arcs further, so none lie on each other.</summary>
        public int Lane { get; }

        /// <summary>
        /// Which of the arrow ends at the attacker's piece this one is, of how many (in the order the arrows were used),
        /// so several arrows leave and land side by side instead of on one point.
        /// </summary>
        public int AttackerEnd { get; }

        public int AttackerEnds { get; }

        /// <summary>The same at the target's piece.</summary>
        public int TargetEnd { get; }

        public int TargetEnds { get; }

        /// <summary>Identifies the arrow between redraws, so only a new one flies in.</summary>
        public string Key => AttackerId + ">" + TargetId + ":" + PowerUp + ":" + Lane;
    }
}
