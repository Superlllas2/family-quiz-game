using FamilyQuiz.Core.State;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// Which shot the 3D set shows and how far the studio lights are up (docs/21_staging.md, "Game night 1 version";
    /// D91, D95 to D97). Plain C#: fed the phase changes and the host lines that start, asked once per frame on the core
    /// clock. A listener only: shots follow phases, so a phase change always cuts, and nothing here holds a phase.
    /// <list type="bullet">
    /// <item>Lobby and StartCountdown: Seats, the studio dark (light level 0).</item>
    /// <item>Intro: HostSolo on each host while their gameStart line shows (the studio still dark), then Establish for
    /// establishMs with the lights coming up over its first lightsUpMs, then Wide. Lines after Establish has begun
    /// never cut back.</item>
    /// <item>FinalIntro: HostSolo while a line shows (the finalStart line), Wide otherwise.</item>
    /// <item>Every other phase: its own shot, whatever the hosts say.</item>
    /// </list>
    /// </summary>
    public sealed class ShotPlan
    {
        /// <summary>
        /// How long HostSolo holds after a bubble ends, so the second Intro line, started by the dialogue a frame later,
        /// never shows a flash of the studio between the two hosts. A camera detail, not a game rule, so not in
        /// content/config.json.
        /// </summary>
        public const long HostLineSlackMs = 250;

        private readonly long _establishMs;
        private readonly long _lightsUpMs;
        private GamePhase _phase = GamePhase.Lobby;
        private string _lineHost;
        private long _lineEndsAtMs;
        private long? _establishStartMs;

        /// <param name="establishMs">staging.intro.establishSeconds in ms.</param>
        /// <param name="lightsUpMs">staging.intro.lightsUpSeconds in ms.</param>
        public ShotPlan(long establishMs, long lightsUpMs)
        {
            _establishMs = establishMs;
            _lightsUpMs = lightsUpMs;
        }

        public GamePhase Phase => _phase;

        public void OnPhase(GamePhase phase, long nowMs)
        {
            _phase = phase;
            _lineHost = null;
            _establishStartMs = null;
        }

        /// <summary>A host line that starts now and shows until endsAtMs; it replaces the line before it.</summary>
        public void OnHostLine(string host, long endsAtMs)
        {
            _lineHost = host;
            _lineEndsAtMs = endsAtMs;
        }

        public ShotChoice Current(long nowMs)
        {
            Advance(nowMs);
            switch (_phase)
            {
                case GamePhase.Lobby:
                case GamePhase.StartCountdown:
                    return ShotChoice.Of(Shot.Seats);
                case GamePhase.Intro:
                    if (_establishStartMs == null) return ShotChoice.Solo(_lineHost);
                    return ShotChoice.Of(nowMs - _establishStartMs.Value < _establishMs ? Shot.Establish : Shot.Wide);
                case GamePhase.FinalIntro:
                    return LineShowing(nowMs) ? ShotChoice.Solo(_lineHost) : ShotChoice.Of(Shot.Wide);
                case GamePhase.TopicVote:
                    return ShotChoice.Of(Shot.Curtain);
                case GamePhase.QuestionShow:
                case GamePhase.Answer:
                    return ShotChoice.Of(Shot.Stage);
                default:
                    return ShotChoice.Of(Shot.Wide);
            }
        }

        /// <summary>0 for a dark studio (only the seat lamps), 1 for the studio fully lit.</summary>
        public double LightLevel(long nowMs)
        {
            Advance(nowMs);
            switch (_phase)
            {
                case GamePhase.Lobby:
                case GamePhase.StartCountdown:
                    return 0;
                case GamePhase.Intro:
                    if (_establishStartMs == null) return 0;
                    if (_lightsUpMs <= 0) return 1;
                    double t = (double)(nowMs - _establishStartMs.Value) / _lightsUpMs;
                    return t <= 0 ? 0 : t >= 1 ? 1 : t;
                default:
                    return 1;
            }
        }

        /// <summary>In the Intro, Establish begins the first time no host line shows.</summary>
        private void Advance(long nowMs)
        {
            if (_phase == GamePhase.Intro && _establishStartMs == null && !LineShowing(nowMs)) _establishStartMs = nowMs;
        }

        private bool LineShowing(long nowMs)
        {
            return _lineHost != null && nowMs < _lineEndsAtMs + HostLineSlackMs;
        }
    }
}
