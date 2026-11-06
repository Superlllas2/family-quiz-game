using System.Collections.Generic;
using FamilyQuiz.Core;
using UnityEngine;

namespace FamilyQuiz.Audio
{
    /// <summary>
    /// The background music (decisions/0019, D113, D114): the tracks of <see cref="MusicTrack.All"/> in an endless
    /// shuffled loop with a crossfade between tracks, from app start, in every phase and through a pause. A listener
    /// only: it reads nothing of the game and never changes state. Runs on real time, not the core clock. Volume 0
    /// mutes it.
    /// </summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        private readonly List<MusicTrack> _tracks = new List<MusicTrack>();
        private readonly List<AudioClip> _clips = new List<AudioClip>();
        private AudioSource[] _sources;
        private MusicPlaylist _playlist;
        private float _fadeSeconds;
        private float _volume;
        private int _active = -1;
        private float _fade = 1f;
        private MusicTrack _nowPlaying;

        /// <param name="volume">0 to 1, the settings panel slider.</param>
        /// <param name="fadeSeconds">The crossfade between tracks (music.fadeSeconds).</param>
        /// <param name="random">Shuffles the playlist.</param>
        public static MusicPlayer Create(float volume, float fadeSeconds, IRandom random)
        {
            var go = new GameObject("MusicPlayer");
            DontDestroyOnLoad(go);
            var player = go.AddComponent<MusicPlayer>();
            player._fadeSeconds = Mathf.Max(0.01f, fadeSeconds);
            player._volume = MusicVolumeFile.Clamp(volume);
            player._sources = new[] { player.Source(), player.Source() };
            foreach (var track in MusicTrack.All)
            {
                var clip = Resources.Load<AudioClip>("Music/" + track.Resource);
                if (clip == null)
                {
                    Debug.LogWarning("Music: missing Resources/Music/" + track.Resource);
                    continue;
                }

                player._tracks.Add(track);
                player._clips.Add(clip);
            }

            if (player._clips.Count > 0) player._playlist = new MusicPlaylist(player._clips.Count, random);
            return player;
        }

        /// <summary>The slider value, 0 to 1.</summary>
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = MusicVolumeFile.Clamp(value);
                ApplyVolumes();
            }
        }

        /// <summary>The track that plays now (the newer one during a crossfade), or null.</summary>
        public MusicTrack NowPlaying => _nowPlaying;

        private AudioSource Source()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.volume = 0f;
            return source;
        }

        private void Start()
        {
            if (_playlist != null) PlayNext();
        }

        private void Update()
        {
            if (_playlist == null || _active < 0) return;
            var current = _sources[_active];
            if (_fade < 1f)
            {
                _fade = Mathf.Min(1f, _fade + Time.unscaledDeltaTime / _fadeSeconds);
                if (_fade >= 1f) _sources[1 - _active].Stop();
                ApplyVolumes();
                return;
            }

            // Start the next track when this one is about to end (or ended early, as a streamed clip can).
            var clip = current.clip;
            bool ending = clip != null && current.time >= clip.length - _fadeSeconds;
            if (!current.isPlaying || ending) PlayNext();
        }

        private void PlayNext()
        {
            int index = _playlist.Next();
            _active = _active < 0 ? 0 : 1 - _active;
            var source = _sources[_active];
            source.clip = _clips[index];
            source.time = 0f;
            source.Play();
            _nowPlaying = _tracks[index];
            _fade = 0f;
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (_sources == null || _active < 0) return;
            _sources[_active].volume = _volume * _fade;
            _sources[1 - _active].volume = _volume * (1f - _fade);
        }
    }
}
