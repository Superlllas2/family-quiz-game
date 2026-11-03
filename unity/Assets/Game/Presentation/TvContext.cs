using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Content;
using UnityEngine;

namespace FamilyQuiz.Presentation
{
    /// <summary>
    /// What every TV view reads: the game (once it exists), the ui strings and the characters. Texts are looked up
    /// in the game language at the moment they are drawn, so a language change shows on the next refresh.
    /// </summary>
    public sealed class TvContext
    {
        private UiStrings _strings;
        private IReadOnlyList<CharacterDef> _characters;
        private string _lang = Languages.Default;

        /// <summary>The game, or null before Attach (content error path).</summary>
        public Game Game { get; private set; }

        public string Lang => Game != null ? Game.State.GameLanguage : _lang;

        public bool HasStrings => _strings != null;

        public void Configure(UiStrings strings, IReadOnlyList<CharacterDef> characters, string lang)
        {
            _strings = strings;
            _characters = characters;
            _lang = string.IsNullOrEmpty(lang) ? Languages.Default : lang;
        }

        public void Attach(Game game)
        {
            Game = game;
            if (game != null) Configure(game.Content.Strings, game.Content.Characters, game.State.GameLanguage);
        }

        public bool Has(string key)
        {
            return _strings != null && _strings.Has(key);
        }

        /// <summary>The text of a ui_strings key in the game language; the key itself when it is missing.</summary>
        public string T(string key)
        {
            return _strings != null ? _strings.Get(key, Lang) : string.Empty;
        }

        public string Format(string key, string name, string value)
        {
            if (_strings == null) return string.Empty;
            return _strings.Format(key, Lang, new Dictionary<string, string> { { name, value } });
        }

        public string Format(string key, string name1, string value1, string name2, string value2)
        {
            if (_strings == null) return string.Empty;
            return _strings.Format(key, Lang, new Dictionary<string, string> { { name1, value1 }, { name2, value2 } });
        }

        /// <summary>A power up marker's text: "Dad → Freeze" (docs/03_rules.md, "Using").</summary>
        public string HitText(PowerUpHit hit)
        {
            if (hit == null) return string.Empty;
            return Format("tv.powerUps.hit", "name", hit.AttackerName, "powerUp", T("tv.powerUps." + hit.PowerUp));
        }

        /// <summary>The character's signature color from content/characters.json.</summary>
        public Color ChipColor(int characterId)
        {
            if (_characters != null)
            {
                foreach (var c in _characters)
                {
                    if (c.Id == characterId) return TvColors.FromHex(c.Color, TvColors.Text);
                }
            }

            return TvColors.Text;
        }
    }
}
