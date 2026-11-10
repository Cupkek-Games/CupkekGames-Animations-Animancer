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
    /// A character starts from a shared <see cref="AnimationKindSet"/> (for
    /// example her body type's) and adds her own entries in <see cref="_entries"/>,
    /// which win on the same kind; per-entry <c>ReturnToKind</c> /
    /// <c>OnlyFromKind</c> drive Animancer state transitions.
    /// </summary>
    [RequireComponent(typeof(AnimancerAnimationEngine))]
    public class CharacterAnimationStateAnimancer : MonoBehaviour, IAnimationStateController
    {
        [Tooltip("The shared set this character starts from (for example her body type's). Her own entries below add to it and win on the same kind.")]
        [SerializeField] private AnimationKindSet _set;

        [Header("Her own kinds (add to the set's; win on the same kind)")]
        [SerializeField] private List<AnimationKindEntry> _entries = new List<AnimationKindEntry>();

        private AnimancerAnimationEngine _engine;
        private Dictionary<string, AnimationKindEntry> _map;

        public Transform Transform => transform;

        /// <summary>The shared set this character starts from; null when she lists everything herself.</summary>
        public AnimationKindSet Set => _set;

        /// <inheritdoc />
        public IReadOnlyCollection<string> Kinds
        {
            get
            {
                EnsureInitialized();
                return _map.Keys;
            }
        }

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
            // The set's kinds first; her own win on the same kind.
            if (_set != null) _set.AddTo(_map);
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

        /// <inheritdoc />
        public bool TryGetClip(string kind, out AnimationClip clip)
        {
            clip = null;
            if (string.IsNullOrEmpty(kind)) return false;
            EnsureInitialized();
            if (!_map.TryGetValue(kind, out AnimationKindEntry entry) || entry.Transition == null) return false;
            clip = entry.Transition.Clip;
            return clip != null;
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
