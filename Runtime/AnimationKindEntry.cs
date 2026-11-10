using System;
using Animancer;
using UnityEngine;

namespace CupkekGames.Animations.Animancer
{
    /// <summary>
    /// One kind and its clip, with optional <c>ReturnToKind</c> /
    /// <c>OnlyFromKind</c> rules: authored on a character
    /// (<see cref="CharacterAnimationStateAnimancer"/>) or in a shared
    /// <see cref="AnimationKindSet"/>.
    /// </summary>
    [Serializable]
    public class AnimationKindEntry
    {
        public string Kind;
        public ClipTransition Transition;

        [Tooltip("Optional: kind name to return to after this clip ends.")]
        public string ReturnToKind;

        [Tooltip("Optional: only play if currently playing this kind (Animancer onlyFrom). Empty = play unconditionally.")]
        public string OnlyFromKind;
    }
}
