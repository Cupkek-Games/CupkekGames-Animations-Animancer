using System.Threading;
using Animancer;
using CupkekGames.Animations;
using CupkekGames.Character;
using UnityEngine;

namespace CupkekGames.Animations.Animancer
{
    [RequireComponent(typeof(AnimancerAnimationEngine))]
    [RequireComponent(typeof(LocomotionAnimancer))]
    public class CombatAnimationsAnimancer : MonoBehaviour, ICombatAnimations
    {
        public ClipTransition GetHit;
        public ClipTransition Death;
        public ClipTransition Win;

        private AnimancerAnimationEngine _engine;
        private LocomotionAnimancer _locomotion;

        private void Awake()
        {
            _engine = GetComponent<AnimancerAnimationEngine>();
            _locomotion = GetComponent<LocomotionAnimancer>();
        }

        public void PlayIdle()
        {
            _locomotion.PlayIdle();
        }

        public void PlayGetHit()
        {
            _engine.ChangeState(GetHit, _locomotion.Idle, _locomotion.Idle,
                GetHit.FadeDuration, FadeMode.FromStart);
        }

        public void PlayDeath()
        {
            _engine.ChangeState(Death, null, null, Death.FadeDuration, FadeMode.FromStart);
        }

        public void PlayWin()
        {
            _engine.ChangeState(Win, null, null, Win.FadeDuration, FadeMode.FromStart);
        }

        public void PlayAnimationWithReturnToIdle(AnimationClip clip, float fadeDuration = 0.25f,
            CancellationToken? cancellationToken = null)
        {
            AnimancerState state = _engine.Play(clip, fadeDuration, FadeMode.FromStart);
            CancellationTokenRegistration? registration = null;
            if (cancellationToken.HasValue)
            {
                registration = cancellationToken.Value.Register(() => { _engine.Play(_locomotion.Idle); });
            }

            _engine.RegisterOnEndReturnTo(state, AnimatorControllerLayer.Base, _locomotion.Idle, registration);
        }

        public Transform Transform => transform;
    }
}
