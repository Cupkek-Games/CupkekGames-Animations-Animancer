using System;
using System.Collections.Generic;
using System.Threading;
using Animancer;
using CupkekGames.Animations;
using CupkekGames.Character;
using UnityEngine;

namespace CupkekGames.Animations.Animancer
{
    /// <summary>
    /// Animancer-backed implementation of <see cref="IAnimationStateController"/>.
    /// Kinds and their clips are authored in <see cref="_entries"/>; per-entry
    /// <c>ReturnToKind</c> / <c>OnlyFromKind</c> drive Animancer state transitions.
    /// </summary>
    [RequireComponent(typeof(AnimancerAnimationEngine))]
    public class CharacterAnimationStateAnimancer : MonoBehaviour, IAnimationStateController
    {
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

        [Header("Authored kinds (kind → clip transition + optional return-to / only-from rules)")]
        [SerializeField] private List<AnimationKindEntry> _entries = new List<AnimationKindEntry>();

        private AnimancerAnimationEngine _engine;
        private Dictionary<string, AnimationKindEntry> _map;

        public Transform Transform => transform;

        public event Action<AnimationClip> OnAnimationPlayed
        {
            add { EnsureInitialized(); _engine.OnAnimationPlayed += value; }
            remove { EnsureInitialized(); _engine.OnAnimationPlayed -= value; }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Lazy init: during runtime Instantiate, Unity initializes parent
        /// GameObjects before children, so a parent's OnEnable (e.g.
        /// HumonoidCharacter wiring EyeMovement) can reach this component
        /// before its own Awake has run. Every public entry point funnels
        /// through here so pre-Awake access is safe.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_map != null) return;

            _engine = GetComponent<AnimancerAnimationEngine>();
            _map = new Dictionary<string, AnimationKindEntry>();
            foreach (AnimationKindEntry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Kind) || entry.Transition == null)
                    continue;
                _map[entry.Kind] = entry;
            }
        }

        private void OnEnable()
        {
            // Default: play idle on enable (matches the old LocomotionAnimancer behavior).
            if (_map != null && _map.TryGetValue(AnimationKinds.Idle, out AnimationKindEntry idle))
                _engine.Play(idle.Transition);
        }

        public void Play(string kind)
        {
            if (string.IsNullOrEmpty(kind)) return;
            EnsureInitialized();
            if (!_map.TryGetValue(kind, out AnimationKindEntry entry) || entry.Transition == null)
                return;

            ClipTransition onlyFrom = ResolveTransition(entry.OnlyFromKind);
            ClipTransition returnTo = ResolveTransition(entry.ReturnToKind);

            _engine.ChangeState(
                entry.Transition,
                onlyFrom,
                returnTo,
                entry.Transition.FadeDuration,
                entry.Transition.FadeMode);
        }

        public void PlayClipWithReturnToIdle(AnimationClip clip, float fadeDuration = 0.25f, CancellationToken? cancellationToken = null)
        {
            if (clip == null) return;

            EnsureInitialized();
            AnimancerState state = _engine.Play(clip, fadeDuration, FadeMode.FromStart);

            ClipTransition idle = ResolveTransition(AnimationKinds.Idle);
            if (idle == null) return;

            CancellationTokenRegistration? registration = null;
            if (cancellationToken.HasValue)
            {
                ClipTransition idleCapture = idle;
                registration = cancellationToken.Value.Register(() => _engine.Play(idleCapture));
            }

            _engine.RegisterOnEndReturnTo(state, AnimatorControllerLayer.Base, idle, registration);
        }

        private ClipTransition ResolveTransition(string kind)
        {
            if (string.IsNullOrEmpty(kind)) return null;
            return _map.TryGetValue(kind, out AnimationKindEntry entry) ? entry.Transition : null;
        }
    }
}
