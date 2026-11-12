using System.Collections.Generic;
using FamilyQuiz.Core;
using FamilyQuiz.Core.Events;
using FamilyQuiz.Core.Content;
using FamilyQuiz.Presentation.Staging;
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

        /// <summary>The power ups drawn at this question's Reveal (PowerUpDrawn), for the TV's "why" tags (D145). The presenter fills and clears it.</summary>
        public List<PowerUpDrawn> Draws { get; } = new List<PowerUpDrawn>();

        /// <summary>
        /// The power ups used in this pick window (PowerUpUsed), in order, for the attack arrows (D149). The presenter
        /// fills it and clears it when the next Answer starts.
        /// </summary>
        public List<PowerUpUsed> Attacks { get; } = new List<PowerUpUsed>();

        /// <summary>
        /// Which seat each player sits in, the same fill order as the 3D studio's (docs/21_staging.md, "Seats"), so the
        /// Scores row stands in sofa order (D159) with the studio on or off. The presenter syncs it with the roster.
        /// </summary>
        public SeatMap Seats { get; } = new SeatMap(SofaOrder.SeatsLeftToRight.Length);

        /// <summary>The attack arrows of this pick window.</summary>
        public List<AttackArrow> Arrows()
        {
            return AttackArrows.Build(Attacks, Game?.State);
        }

        /// <summary>Seats new players and frees the seats of players who left.</summary>
        public void SyncSeats()
        {
            if (Game == null) return;
            var ids = new List<string>();
            foreach (var p in Game.State.Players) ids.Add(p.PlayerId);
            Seats.Sync(ids);
        }

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

        public string Format(string key, string name1, string value1, string name2, string value2, string name3, string value3)
        {
            if (_strings == null) return string.Empty;
            return _strings.Format(key, Lang, new Dictionary<string, string> { { name1, value1 }, { name2, value2 }, { name3, value3 } });
        }

        /// <summary>A power up hit's text: "Аня → Слизь → Соня" (docs/03_rules.md, "Using"; D145).</summary>
        public string HitText(PowerUpHit hit)
        {
            if (hit == null) return string.Empty;
            return Format("tv.powerUps.hit", "name", hit.AttackerName, "powerUp", T("tv.powerUps." + hit.PowerUp), "target", hit.TargetName);
        }

        /// <summary>A power up draw's two lines: "Аня получает: Слизь" and why (D145).</summary>
        public (string Title, string Reason) DrawText(string name, string powerUp, int rankBefore)
        {
            return (Format("tv.powerUps.drawn", "name", name, "powerUp", T("tv.powerUps." + powerUp)), T(PowerUpDrawReason.Key(rankBefore)));
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
