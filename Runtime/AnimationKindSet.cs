using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.Animations.Animancer
{
    /// <summary>
    /// A shared list of kinds and their clips that characters start from
    /// (<see cref="CharacterAnimationStateAnimancer.Set"/>), for example one
    /// per body type: a character's own entries add to it and win on the same
    /// kind, so she lists only her signature moves. A set can build on a
    /// <see cref="Base"/> set the same way: the base's kinds first, this set's
    /// win. Its transitions are shared by every character that uses it; each
    /// character plays them on her own states, so nothing she plays leaks to another.
    /// </summary>
    [CreateAssetMenu(menuName = "CupkekGames/Animations/Animation Kind Set", fileName = "AnimationKindSet")]
    public class AnimationKindSet : ScriptableObject
    {
        // Deeper than any sane chain of bases: a loop of bases is an authoring error.
        private const int MaxDepth = 16;

        [Tooltip("Optional: the set this one builds on. Its kinds come first; this set's entries win on the same kind.")]
        [SerializeField] private AnimationKindSet _base;

        [Tooltip("This set's kinds (kind → clip transition + optional return-to / only-from rules).")]
        [SerializeField] private List<AnimationKindEntry> _entries = new List<AnimationKindEntry>();

        /// <summary>The set this one builds on; null when none.</summary>
        public AnimationKindSet Base => _base;

        /// <summary>This set's own entries (its base's not included).</summary>
        public IReadOnlyList<AnimationKindEntry> Entries => _entries;

        /// <summary>
        /// Adds this set's kinds to <paramref name="map"/>: its base's first,
        /// then its own, each winning over what came before on the same kind.
        /// </summary>
        public void AddTo(Dictionary<string, AnimationKindEntry> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            AddTo(map, 0);
        }

        private void AddTo(Dictionary<string, AnimationKindEntry> map, int depth)
        {
            if (depth > MaxDepth)
                throw new InvalidOperationException($"[AnimationKindSet] '{name}': its bases loop (more than {MaxDepth} deep).");

            if (_base != null) _base.AddTo(map, depth + 1);
            foreach (AnimationKindEntry entry in _entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Kind) || entry.Transition == null) continue;
                map[entry.Kind] = entry;
            }
        }
    }
}
