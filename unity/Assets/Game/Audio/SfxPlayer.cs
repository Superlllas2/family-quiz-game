using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using UnityEngine;

namespace FamilyQuiz.Audio
{
    /// <summary>
    /// The TV's sound effects (decisions/0020, D120, D121): the cue of each game event (<see cref="SfxCues"/>), a tick on
    /// each of the answer clock's last seconds, and a gibberish voice while a host bubble speaks. A listener only: it
    /// reads the game and the events and never changes state. Volume 0 mutes it.
    /// </summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        /// <summary>No host voice.</summary>
        public const string NoVoice = "none";

        private const float VoiceSecondsPerLetter = 0.05f;
        private const float VoiceMaxSeconds = 3f;
        private const float VoiceFadeSeconds = 0.15f;

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AudioClip> _voices = new Dictionary<string, AudioClip>();
        private Game _game;
        private AudioSource _effects;
        private AudioSource _ticks;
        private AudioSource _voice;
        private float _volume;
        private string _voiceStyle;
        private int _lastTick;
        private long _tickPhaseId = -1;
        private float _voiceEndsAt;

        /// <param name="voiceStyle">sfx.hostVoice: none, or a voice clip style such as d_wahwah.</param>
        public static SfxPlayer Create(Game game, float volume, string voiceStyle)
        {
            var go = new GameObject("SfxPlayer");
            DontDestroyOnLoad(go);
            var player = go.AddComponent<SfxPlayer>();
            player._game = game;
            player._volume = MusicVolumeFile.Clamp(volume);
            player._voiceStyle = voiceStyle ?? NoVoice;
            player._effects = player.Source();
            player._ticks = player.Source();
            player._voice = player.Source();
            player._voice.loop = true;
            foreach (var cue in SfxCues.All)
            {
                var clip = Resources.Load<AudioClip>("Sfx/sfx_" + cue);
                if (clip != null) player._clips[cue] = clip;
                else Debug.LogWarning("Sfx: missing Resources/Sfx/sfx_" + cue);
            }

            if (player._voiceStyle != NoVoice)
            {
                foreach (var host in new[] { "konstantin", "ilja" })
                {
                    var clip = Resources.Load<AudioClip>("Voices/voice_" + player._voiceStyle + "_" + host);
                    if (clip != null) player._voices[host] = clip;
                    else Debug.LogWarning("Sfx: missing Resources/Voices/voice_" + player._voiceStyle + "_" + host);
                }
            }

            player.ApplyVolume();
            return player;
        }

        /// <summary>The settings panel slider, 0 to 1.</summary>
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = MusicVolumeFile.Clamp(value);
                ApplyVolume();
            }
        }

        private AudioSource Source()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            return source;
        }

        private void ApplyVolume()
        {
            _effects.volume = _volume;
            _ticks.volume = _volume * 0.8f;
            if (!_voice.isPlaying) _voice.volume = _volume * 0.7f;
        }

        /// <summary>Hand every batch of events, like every other listener.</summary>
        public void OnEvents(IReadOnlyList<IGameEvent> events)
        {
            if (_game == null || events == null || events.Count == 0) return;
            foreach (var cue in SfxCues.For(events, _game.State)) Play(cue);
        }

        /// <summary>A host bubble starts: the host's voice for about as long as its text takes to read, at most 3 s.</summary>
        public void HostSpeaks(string host, string text)
        {
            if (!_voices.TryGetValue(host ?? "", out var clip)) return;
            float seconds = Mathf.Clamp((text?.Length ?? 0) * VoiceSecondsPerLetter, 0.6f, VoiceMaxSeconds);
            _voice.clip = clip;
            _voice.time = Random.Range(0f, Mathf.Max(0f, clip.length - seconds - 0.1f));
            _voice.volume = _volume * 0.7f;
            _voice.Play();
            _voiceEndsAt = Time.unscaledTime + seconds;
        }

        private void Play(string cue)
        {
            if (_volume <= 0f || !_clips.TryGetValue(cue, out var clip)) return;
            _effects.PlayOneShot(clip);
        }

        private void Update()
        {
            if (_game == null) return;
            // The answer clock's last seconds: one tick per second, a little higher for the last three.
            var state = _game.State;
            if (state.PhaseId != _tickPhaseId)
            {
                _tickPhaseId = state.PhaseId;
                _lastTick = int.MaxValue;
            }

            int left = SfxCues.SecondsLeft(state, _game.NowMs);
            if (left >= 1 && left <= SfxCues.TickFromSeconds && left < _lastTick)
            {
                _lastTick = left;
                if (_volume > 0f && _clips.TryGetValue(SfxCues.Tick, out var tick))
                {
                    _ticks.pitch = left <= 3 ? 1.25f : 1f;
                    _ticks.PlayOneShot(tick);
                }
            }

            // The host voice fades out at the end of its line.
            if (_voice.isPlaying)
            {
                float remaining = _voiceEndsAt - Time.unscaledTime;
                if (remaining <= 0f) _voice.Stop();
                else if (remaining < VoiceFadeSeconds) _voice.volume = _volume * 0.7f * remaining / VoiceFadeSeconds;
            }
        }
    }
}
