using System;
using Animancer;
using CupkekGames.Animations;
using CupkekGames.Character;
using UnityEngine;

namespace CupkekGames.Animations.Animancer
{
    [RequireComponent(typeof(AnimancerAnimationEngine))]
    public class LocomotionAnimancer : MonoBehaviour, ILocomotion
    {
        public ClipTransition Idle;
        public ClipTransition Walk;

        private AnimancerAnimationEngine _engine;

        private void Awake()
        {
            _engine = GetComponent<AnimancerAnimationEngine>();
        }

        private void OnEnable()
        {
            _engine.Play(Idle);
        }

        public void PlayIdle()
        {
            _engine.ChangeState(Idle, Walk, null, Idle.FadeDuration, Idle.FadeMode);
        }

        public void PlayWalk()
        {
            _engine.ChangeState(Walk, Idle, null, Walk.FadeDuration, Walk.FadeMode);
        }

        public void PlayClipWithReturnToIdle(AnimationClip clip, float fadeDuration = 0.25f)
        {
            AnimancerState state = _engine.Play(clip, fadeDuration, FadeMode.FromStart);
            _engine.RegisterOnEndReturnTo(state, AnimatorControllerLayer.Base, Idle, null);
        }

        public event Action<AnimationClip> OnAnimationPlayed
        {
            add => _engine.OnAnimationPlayed += value;
            remove => _engine.OnAnimationPlayed -= value;
        }
    }
}
