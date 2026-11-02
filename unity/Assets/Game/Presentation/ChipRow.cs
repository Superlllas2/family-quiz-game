using System.Collections.Generic;
using FamilyQuiz.Core.State;
using UnityEngine;
using UnityEngine.UI;

namespace FamilyQuiz.Presentation
{
    /// <summary>One character chip per seated player in a centered row, in join order. Rebuilt only when the roster changes.</summary>
    public sealed class ChipRow : MonoBehaviour
    {
        private const float MaxChipWidth = 340f;
        private const float Spacing = 16f;
        private const float NameSize = 44f;

        private readonly List<CharacterChip> _chips = new List<CharacterChip>();
        private TvContext _context;
        private HorizontalLayoutGroup _layout;

        public static ChipRow Create(Transform parent, TvContext context)
        {
            var rect = UiBuilder.Rect("Chips", parent);
            var row = rect.gameObject.AddComponent<ChipRow>();
            row._context = context;
            row._layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            row._layout.childAlignment = TextAnchor.LowerCenter;
            row._layout.spacing = Spacing;
            row._layout.childControlWidth = true;
            row._layout.childControlHeight = true;
            row._layout.childForceExpandWidth = false;
            row._layout.childForceExpandHeight = false;
            return row;
        }

        public IReadOnlyList<CharacterChip> Chips => _chips;

        public CharacterChip Find(string playerId)
        {
            foreach (var chip in _chips)
            {
                if (chip.PlayerId == playerId) return chip;
            }

            return null;
        }

        /// <summary>Matches the chips to the seated players and updates names, colors and connection.</summary>
        public void Sync(GameState state, float chipHeight)
        {
            if (!SameRoster(state.Players))
            {
                foreach (var chip in _chips) Destroy(chip.gameObject);
                _chips.Clear();
                foreach (var player in state.Players) _chips.Add(CharacterChip.Create(transform, player.PlayerId, NameSize));
            }

            float available = ((RectTransform)transform).rect.width;
            if (available <= 0f) available = 1840f;
            int count = Mathf.Max(1, _chips.Count);
            float width = Mathf.Min(MaxChipWidth, (available - Spacing * (count - 1)) / count);
            for (int i = 0; i < _chips.Count; i++)
            {
                var player = state.Players[i];
                UiBuilder.Size(_chips[i], width, chipHeight);
                _chips[i].Set(player.Name, _context.ChipColor(player.CharacterId), player.Connected);
            }
        }

        private bool SameRoster(IReadOnlyList<PlayerState> players)
        {
            if (players.Count != _chips.Count) return false;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].PlayerId != _chips[i].PlayerId) return false;
            }

            return true;
        }
    }
}
