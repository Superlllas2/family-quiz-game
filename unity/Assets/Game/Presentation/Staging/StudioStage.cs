using System;
using System.Collections.Generic;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.State;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FamilyQuiz.Presentation.Staging
{
    /// <summary>
    /// The 3D set behind the TV overlay (docs/21_staging.md, "Game night 1 version"; 0015, 0016). Loads the Studio
    /// scene additively when switched on, then every frame puts the camera on the shot ShotPlan picks, opens and
    /// closes the curtain, brings the studio lights up and down, and plays the guests and hosts from the events.
    /// A listener only: it reads the game and the events it is handed and never changes state or holds a phase.
    /// Switched off, it hides the studio and the flat views show as before.
    /// </summary>
    public sealed class StudioStage : MonoBehaviour
    {
        public const string SceneName = "Studio";
        private const float CurtainOpenScale = 0.16f;
        private const float SeatsDriftMetres = 0.18f;
        private const float SeatsDriftSeconds = 24f;

        private static readonly Dictionary<string, Color> HostColors = new Dictionary<string, Color>
        {
            ["konstantin"] = new Color(0.45f, 0.12f, 0.14f),
            ["ilja"] = new Color(0.13f, 0.2f, 0.38f)
        };

        private readonly SeatMap _seats = new SeatMap(8);
        private readonly Dictionary<string, GuestView> _guests = new Dictionary<string, GuestView>();
        private readonly Dictionary<string, int> _guestCharacter = new Dictionary<string, int>();
        private readonly Dictionary<string, HostView> _hosts = new Dictionary<string, HostView>();
        private readonly List<(long DueMs, string PlayerId, GuestView.Move Move)> _pending = new List<(long, string, GuestView.Move)>();

        private TvContext _context;
        private Action<bool> _visibleChanged;
        private ShotPlan _plan;
        private StudioRig _rig;
        private Scene _studioScene;
        private Scene _mainScene;
        private Camera _mainCamera;
        private Transform _cast;
        private bool _on;
        private bool _loading;
        private bool _visible;
        private long _phaseStartMs;
        private Shot _lastShot = Shot.Wide;
        private float _baseExposure;
        private float _baseVignette;
        private float _guestKey;
        private float _hostKey;
        private float[] _stageLamps = Array.Empty<float>();
        private ColorAdjustments _color;
        private Vignette _vignette;
        private DepthOfField _dof;

        public static StudioStage Create(TvContext context, Action<bool> visibleChanged)
        {
            var go = new GameObject("StudioStage");
            var stage = go.AddComponent<StudioStage>();
            stage._context = context;
            stage._visibleChanged = visibleChanged;
            stage._mainScene = SceneManager.GetActiveScene();
            return stage;
        }

        /// <summary>True while the studio is loaded and shown behind the overlay.</summary>
        public bool Visible => _visible;

        public bool IsOn => _on;

        public void SetOn(bool on)
        {
            _on = on;
            if (on && _rig == null && !_loading) Load();
            ApplyVisibility();
        }

        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            var game = _context.Game;
            if (game == null) return;
            EnsurePlan();
            bool roster = false;
            foreach (var e in events)
            {
                switch (e)
                {
                    case PhaseStarted started:
                        _plan.OnPhase(started.Phase, game.NowMs);
                        _phaseStartMs = game.NowMs;
                        if (_rig != null) EnterPhase(started.Phase);
                        break;
                    case AnswerLocked locked:
                        Guest(locked.PlayerId)?.Play(GuestView.Move.Nod);
                        break;
                    case PowerUpApplied applied:
                        Guest(applied.TargetId)?.Play(GuestView.Move.Flinch);
                        break;
                    case RaceRevealed race:
                        if (_rig != null) RaceReactions(race.Answer);
                        break;
                    case RosterChanged _:
                    case PlayerJoined _:
                    case PlayerLeft _:
                    case PlayerUpdated _:
                    case PlayerConnectionChanged _:
                    case ReadyChanged _:
                    case SettingsChanged _:
                        roster = true;
                        break;
                }
            }

            if (roster && _rig != null) SyncGuests(true);
        }

        /// <summary>A host line that starts now and shows until endsAtMs on the core clock.</summary>
        public void OnHostLine(string host, long endsAtMs)
        {
            var game = _context.Game;
            if (game == null) return;
            EnsurePlan();
            _plan.OnHostLine(host, endsAtMs);
            if (_hosts.TryGetValue(host ?? string.Empty, out var view)) view.Speak((endsAtMs - game.NowMs) / 1000f);
        }

        // ------------------------------------------------------------------------ loading and showing

        private void EnsurePlan()
        {
            if (_plan != null || _context.Game == null) return;
            var intro = _context.Game.Content.Config.Staging.Intro;
            _plan = new ShotPlan((long)(intro.EstablishSeconds * 1000), (long)(intro.LightsUpSeconds * 1000));
            _plan.OnPhase(_context.Game.State.Phase, _context.Game.NowMs);
            _phaseStartMs = _context.Game.NowMs;
        }

        private void Load()
        {
            if (!Application.CanStreamedLevelBeLoaded(SceneName))
            {
                Debug.LogWarning("StudioStage: the Studio scene is not in the build; the 3D set stays off");
                return;
            }

            _loading = true;
            var op = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            op.completed += _ => OnLoaded();
        }

        private void OnLoaded()
        {
            _loading = false;
            _studioScene = SceneManager.GetSceneByName(SceneName);
            foreach (var root in _studioScene.GetRootGameObjects())
            {
                _rig = root.GetComponentInChildren<StudioRig>(true);
                if (_rig != null) break;
            }

            if (_rig == null)
            {
                Debug.LogWarning("StudioStage: the Studio scene has no StudioRig");
                return;
            }

            LightProbes.TetrahedralizeAsync();
            var profile = _rig.Volume.profile; // an instance, so the asset never changes at runtime
            profile.TryGet(out _color);
            profile.TryGet(out _vignette);
            profile.TryGet(out _dof);
            _baseExposure = _color != null ? _color.postExposure.value : 0f;
            _baseVignette = _vignette != null ? _vignette.intensity.value : 0f;
            _guestKey = _rig.GuestKey != null ? _rig.GuestKey.intensity : 0f;
            _hostKey = _rig.HostKey != null ? _rig.HostKey.intensity : 0f;
            _stageLamps = new float[_rig.StageLamps != null ? _rig.StageLamps.Length : 0];
            for (int i = 0; i < _stageLamps.Length; i++) _stageLamps[i] = _rig.StageLamps[i].intensity;

            _cast = new GameObject("Cast").transform;
            SceneManager.MoveGameObjectToScene(_cast.gameObject, _studioScene);
            foreach (var pair in HostColors)
            {
                var place = pair.Key == "konstantin" ? _rig.HostKonstantin : _rig.HostIlja;
                if (place != null) _hosts[pair.Key] = HostView.Create(_cast, place, pair.Key, pair.Value);
            }

            if (_context.Game != null)
            {
                EnsurePlan();
                SyncGuests(false);
                EnterPhase(_context.Game.State.Phase);
            }

            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            bool visible = _on && _rig != null;
            if (_rig != null)
            {
                foreach (var root in _studioScene.GetRootGameObjects()) root.SetActive(visible);
                if (visible) SceneManager.SetActiveScene(_studioScene);
                else if (_mainScene.IsValid()) SceneManager.SetActiveScene(_mainScene);
            }

            if (_mainCamera == null)
                foreach (var root in _mainScene.GetRootGameObjects())
                {
                    var cam = root.GetComponentInChildren<Camera>();
                    if (cam != null && (_rig == null || cam != _rig.Camera))
                    {
                        _mainCamera = cam;
                        break;
                    }
                }

            if (_mainCamera != null) _mainCamera.enabled = !visible;
            if (visible == _visible) return;
            _visible = visible;
            _visibleChanged?.Invoke(visible);
        }

        // ------------------------------------------------------------------------ the cast

        private GuestView Guest(string playerId)
        {
            return playerId != null && _guests.TryGetValue(playerId, out var g) && !g.Gone ? g : null;
        }

        private void SyncGuests(bool animate)
        {
            var game = _context.Game;
            var state = game.State;
            var ids = new List<string>();
            foreach (var p in state.Players) ids.Add(p.PlayerId);
            var changes = _seats.Sync(ids);
            var popSeconds = (float)game.Content.Config.Staging.Lobby.SeatPopSeconds;
            var cardSeconds = (float)game.Content.Config.Staging.Reveal.CardsUpSeconds;

            foreach (var id in changes.Left)
            {
                if (!_guests.TryGetValue(id, out var gone)) continue;
                gone.Play(GuestView.Move.Leave);
                Destroy(gone.gameObject, 1f);
                _guests.Remove(id);
                _guestCharacter.Remove(id);
            }

            foreach (var p in state.Players)
            {
                int seat = _seats.SeatOf(p.PlayerId);
                if (seat < 0 || seat >= _rig.Seats.Length || _rig.Seats[seat] == null) continue;
                if (!_guests.TryGetValue(p.PlayerId, out var view))
                {
                    view = GuestView.Create(_cast, _rig.Seats[seat], p.Name);
                    _guests[p.PlayerId] = view;
                    _guestCharacter[p.PlayerId] = p.CharacterId;
                    view.Configure(_context.ChipColor(p.CharacterId), popSeconds, cardSeconds);
                    if (animate) view.Play(GuestView.Move.Enter);
                }
                else if (_guestCharacter[p.PlayerId] != p.CharacterId)
                {
                    // A character swap: the new character makes its entrance in the same seat.
                    _guestCharacter[p.PlayerId] = p.CharacterId;
                    view.Configure(_context.ChipColor(p.CharacterId), popSeconds, cardSeconds);
                    view.Play(GuestView.Move.Enter);
                }

                view.SetLabels(CharacterName(p.CharacterId), p.Name, state.Phase == GamePhase.Lobby && p.Ready);
                view.SetAway(!p.Connected);
            }
        }

        private string CharacterName(int id)
        {
            foreach (var c in _context.Game.Content.Characters)
                if (c.Id == id) return c.Name != null ? c.Name.Get(_context.Lang) : string.Empty;
            return string.Empty;
        }

        private void EnterPhase(GamePhase phase)
        {
            var state = _context.Game.State;
            foreach (var g in _guests.Values)
            {
                g.SetApplauding(false);
                g.SetCelebrating(false);
                g.SetLookingDown(false);
                if (phase != GamePhase.Reveal) g.LowerCard();
            }

            _pending.Clear();
            switch (phase)
            {
                case GamePhase.Lobby:
                    SyncGuests(false);
                    break;
                case GamePhase.Reveal:
                    RevealCards(state);
                    break;
                case GamePhase.RoundEnd:
                {
                    var winners = new HashSet<string>();
                    foreach (var p in Standings.RoundWinners(state)) winners.Add(p.PlayerId);
                    foreach (var pair in _guests)
                    {
                        if (winners.Contains(pair.Key)) pair.Value.Play(GuestView.Move.Wave);
                        else pair.Value.SetApplauding(true);
                    }

                    break;
                }
                case GamePhase.Results:
                    foreach (var pair in _guests)
                    {
                        if (pair.Key == state.WinnerId) pair.Value.SetCelebrating(true);
                        else pair.Value.SetApplauding(true);
                    }

                    break;
                case GamePhase.Poll:
                    foreach (var g in _guests.Values) g.SetLookingDown(true);
                    break;
            }
        }

        /// <summary>Reveal: every guest holds up their answer, then reacts once the cards are up.</summary>
        private void RevealCards(GameState state)
        {
            var q = state.Question;
            var cfg = _context.Game.Content.Config.Staging.Reveal;
            long due = _context.Game.NowMs + (long)(cfg.CardsUpSeconds * 1000);
            foreach (var p in state.Players)
            {
                var view = Guest(p.PlayerId);
                if (view == null) continue;
                var turn = p.Turn;
                string text = string.Empty;
                var color = TvColors.Text;
                bool answered = false;
                if (turn != null && q != null)
                {
                    if (q.IsClosestValue && turn.Value.HasValue)
                    {
                        text = NumberText.Format(turn.Value.Value);
                        color = new Color(0.1f, 0.1f, 0.12f);
                        answered = true;
                    }
                    else if (!q.IsClosestValue && turn.Choice.HasValue)
                    {
                        text = ((char)('A' + turn.Choice.Value)).ToString();
                        color = TvColors.Answer(turn.Choice.Value);
                        answered = true;
                    }
                }

                view.RaiseCard(text, color);
                _pending.Add((due, p.PlayerId, !answered ? GuestView.Move.Shrug : turn.Correct ? GuestView.Move.Happy : GuestView.Move.Sad));
            }
        }

        private void RaceReactions(bool answer)
        {
            foreach (var p in _context.Game.State.Players)
            {
                var view = Guest(p.PlayerId);
                if (view == null) continue;
                var tf = p.Turn?.TrueFalse;
                view.Play(!tf.HasValue ? GuestView.Move.Shrug : tf.Value == answer ? GuestView.Move.Happy : GuestView.Move.Sad);
            }
        }

        // ------------------------------------------------------------------------ per frame

        private void LateUpdate()
        {
            if (!_visible || _rig == null || _context.Game == null || _plan == null) return;
            long now = _context.Game.NowMs;
            var state = _context.Game.State;

            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (now < _pending[i].DueMs) continue;
                Guest(_pending[i].PlayerId)?.Play(_pending[i].Move);
                _pending.RemoveAt(i);
            }

            var choice = _plan.Current(now);
            if (choice.Shot == Shot.Establish && _lastShot != Shot.Establish)
                foreach (var g in _guests.Values) g.Play(GuestView.Move.Wave);
            _lastShot = choice.Shot;
            PlaceCamera(choice, now, state);
            Curtain(state, now);
            Lights(_plan.LightLevel(now), state, now);
            foreach (var g in _guests.Values) g.FaceCamera(_rig.Camera);
        }

        private void PlaceCamera(ShotChoice choice, long now, GameState state)
        {
            var cam = _rig.Camera;
            Transform pose = _rig.WideShot;
            float fov = _rig.WideFieldOfView;
            Vector3 position;
            Quaternion rotation;
            switch (choice.Shot)
            {
                case Shot.Seats:
                    pose = _rig.SeatsShot;
                    fov = _rig.SeatsFieldOfView;
                    break;
                case Shot.Establish:
                {
                    long start = _plan.EstablishStartMs ?? now;
                    double span = _context.Game.Content.Config.Staging.Intro.EstablishSeconds * 1000;
                    float t = Mathf.SmoothStep(0f, 1f, span > 0 ? (float)((now - start) / span) : 1f);
                    cam.transform.SetPositionAndRotation(Vector3.Lerp(_rig.EstablishStart.position, _rig.EstablishEnd.position, t),
                        Quaternion.Slerp(_rig.EstablishStart.rotation, _rig.EstablishEnd.rotation, t));
                    cam.fieldOfView = _rig.EstablishFieldOfView;
                    SetDof(false);
                    return;
                }
                case Shot.HostSolo:
                    pose = choice.Host == "ilja" ? _rig.HostSoloIlja : _rig.HostSoloKonstantin;
                    fov = _rig.HostSoloFieldOfView;
                    break;
                case Shot.Curtain:
                    pose = _rig.CurtainShot;
                    fov = _rig.CurtainFieldOfView;
                    break;
                case Shot.Stage:
                    if (state.Phase == GamePhase.Answer && state.PhaseEndsAtMs.HasValue)
                    {
                        // A slow push in over the answer window (docs/21_staging.md, Answer).
                        double span = Math.Max(1, state.PhaseEndsAtMs.Value - _phaseStartMs);
                        float t = Mathf.Clamp01((float)((now - _phaseStartMs) / span));
                        cam.transform.SetPositionAndRotation(Vector3.Lerp(_rig.StageShot.position, _rig.StagePushIn.position, t), _rig.StageShot.rotation);
                        cam.fieldOfView = _rig.StageFieldOfView;
                        SetDof(false);
                        return;
                    }

                    pose = _rig.StageShot;
                    fov = _rig.StageFieldOfView;
                    break;
            }

            position = pose.position;
            rotation = pose.rotation;
            if (choice.Shot == Shot.Seats)
                position += pose.right * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / SeatsDriftSeconds) * SeatsDriftMetres;
            cam.transform.SetPositionAndRotation(position, rotation);
            cam.fieldOfView = fov;
            SetDof(choice.Shot == Shot.HostSolo);
        }

        private void SetDof(bool on)
        {
            if (_dof == null) return;
            _dof.mode.Override(on ? DepthOfFieldMode.Gaussian : DepthOfFieldMode.Off);
        }

        private void Curtain(GameState state, long now)
        {
            if (_rig.CurtainLeft == null || _rig.CurtainRight == null) return;
            float open;
            switch (state.Phase)
            {
                case GamePhase.QuestionShow:
                {
                    double span = _context.Game.Content.Config.Staging.QuestionShow.CurtainOpenSeconds * 1000;
                    open = span > 0 ? Mathf.SmoothStep(0f, 1f, (float)((now - _phaseStartMs) / span)) : 1f;
                    break;
                }
                case GamePhase.Answer:
                    open = 1f;
                    break;
                default:
                    open = 0f; // closes off camera at the start of Reveal (docs/21_staging.md, "Between questions")
                    break;
            }

            float sx = Mathf.Lerp(1f, CurtainOpenScale, open);
            _rig.CurtainLeft.localScale = new Vector3(sx, 1f, 1f);
            _rig.CurtainRight.localScale = new Vector3(sx, 1f, 1f);
        }

        private void Lights(double level, GameState state, long now)
        {
            float dark = (float)(1.0 - level);
            if (_color != null) _color.postExposure.Override(_baseExposure - _rig.DarkStops * dark);
            if (_vignette != null) _vignette.intensity.Override(Mathf.Lerp(_baseVignette, Mathf.Max(_baseVignette, 0.5f), dark));
            float boost = Mathf.Pow(2f, _rig.DarkStops * dark);
            if (_rig.GuestKey != null) _rig.GuestKey.intensity = _guestKey * boost;
            if (_rig.HostKey != null) _rig.HostKey.intensity = _hostKey * boost;

            float lamp;
            switch (state.Phase)
            {
                case GamePhase.TopicVote:
                    lamp = 0.65f + Mathf.Sin(Time.unscaledTime * 0.8f) * 0.15f;
                    break;
                case GamePhase.QuestionShow:
                    lamp = Mathf.Lerp(0.6f, 1f, Mathf.Clamp01((now - _phaseStartMs) / 1200f));
                    break;
                case GamePhase.Answer:
                {
                    long left = state.PhaseEndsAtMs.HasValue ? state.PhaseEndsAtMs.Value - now : long.MaxValue;
                    lamp = left < 3000 ? 1.3f : 1f; // the lamps brighten in the last seconds
                    break;
                }
                default:
                    lamp = 0.55f;
                    break;
            }

            for (int i = 0; i < _stageLamps.Length; i++)
                if (_rig.StageLamps[i] != null) _rig.StageLamps[i].intensity = _stageLamps[i] * lamp;
        }
    }
}
