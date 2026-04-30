using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Animancer;
using CupkekGames.Animations;
using CupkekGames.Character;
using CupkekGames.AddressableAssets;
using CupkekGames.SceneManagement;
using CupkekGames.Sequencer;
using CupkekGames.Services;
using CupkekGames.Settings;
using CupkekGames.GameSave;
using CupkekGames.TimeSystem;
using UnityEngine.Playables;

namespace CupkekGames.Animations.Animancer
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AnimancerComponent))]
    public class AnimancerAnimationEngine : MonoBehaviour, IAnimationEngine, IAnimationTimeController
    {
        public static readonly float FADE_DURATION = 0.2f;

        private AnimancerComponent _animancerComponent;
        private List<AnimancerLayer> _layers;
        private TimeContext _timeContext;

        public event Action<AnimationClip> OnAnimationPlayed;

        public virtual void Awake()
        {
            _animancerComponent = GetComponent<AnimancerComponent>();

            AnimatorControllerLayerDatabase animatorControllerLayerDatabase =
                ServiceLocator.Get<AnimatorControllerLayerDatabase>();

            _layers = new List<AnimancerLayer>();
            for (int i = 0; i < Enum.GetValues(typeof(AnimatorControllerLayer)).Length; i++)
            {
                AnimancerLayer animancerLayer = _animancerComponent.Layers[i];
                AvatarMask mask = animatorControllerLayerDatabase.GetMask((AnimatorControllerLayer)i);
                if (mask != null)
                {
                    animancerLayer.Mask = mask;
                }

                _layers.Add(animancerLayer);
            }
        }

        public virtual void OnDestroy()
        {
            UnRegisterTimeContext();
        }

        // --- Internal methods used by capability components ---

        public AnimancerState Play(AnimationClip clip, float fadeDuration = 0.2f,
            FadeMode fadeMode = FadeMode.FixedSpeed, AnimatorControllerLayer layer = 0)
        {
            AnimancerState state = _layers[(int)layer].Play(clip, fadeDuration, fadeMode);
            OnAnimationPlayed?.Invoke(clip);
            return state;
        }

        public AnimancerState Play(ClipTransition clipTransition, float? fadeDuration = null,
            FadeMode? fadeMode = null, AnimatorControllerLayer layer = 0)
        {
            if (fadeDuration == null)
                fadeDuration = clipTransition.FadeDuration;

            if (fadeMode == null)
                fadeMode = clipTransition.FadeMode;

            AnimancerState state = _layers[(int)layer].Play(clipTransition, fadeDuration.Value, fadeMode.Value);
            OnAnimationPlayed?.Invoke(clipTransition.Clip);
            return state;
        }

        public bool IsPlaying(ClipTransition clip)
        {
            return _animancerComponent.IsPlaying(clip);
        }

        public AnimancerState ChangeState(ClipTransition clip, ClipTransition onlyFrom, ClipTransition returnTo,
            float fadeDuration, FadeMode fadeMode, AnimatorControllerLayer layer = 0, bool fadeOut = false,
            CancellationToken? cancellationToken = null)
        {
            if (onlyFrom != null && !IsPlaying(onlyFrom))
                return null;

            AnimancerState state = Play(clip, fadeDuration, fadeMode, layer);

            if (returnTo != null)
            {
                CancellationTokenRegistration? registration = null;
                if (cancellationToken.HasValue)
                {
                    registration = cancellationToken.Value.Register(() => { Play(returnTo, FADE_DURATION); });
                }

                RegisterOnEndReturnTo(state, layer, returnTo, registration);
            }
            else if (fadeOut)
            {
                CancellationTokenRegistration? registration = null;
                if (cancellationToken.HasValue)
                {
                    registration =
                        cancellationToken.Value.Register(() => { _layers[(int)layer].StartFade(0, FADE_DURATION); });
                }

                RegisterOnEndFadeOut(state, layer, registration);
            }

            return state;
        }

        public void RegisterOnEndReturnTo(AnimancerState state, AnimatorControllerLayer layer,
            ClipTransition returnTo, CancellationTokenRegistration? registration)
        {
            if (state.Events(this, out AnimancerEvent.Sequence events))
            {
                events.OnEnd = () =>
                {
                    registration?.Dispose();
                    Play(returnTo, FADE_DURATION, returnTo.FadeMode, layer);
                };
            }
        }

        public void RegisterOnEndFadeOut(AnimancerState state, AnimatorControllerLayer layer,
            CancellationTokenRegistration? registration)
        {
            if (state.Events(this, out AnimancerEvent.Sequence events))
            {
                events.OnEnd = () =>
                {
                    registration?.Dispose();
                    _layers[(int)layer].StartFade(0, FADE_DURATION);
                };
            }
        }

        // --- IAnimationEngine implementation ---

        public void PlayClip(AnimationClipData clipData, AnimationClipData returnClip = null,
            int layer = 0, bool fadeOutLayer = false)
        {
            if (clipData == null || clipData.Clip == null) return;

            var animLayer = (AnimatorControllerLayer)layer;
            AnimancerState state = Play(clipData.Clip, clipData.FadeDuration, FadeMode.FromStart, animLayer);

            if (returnClip != null && returnClip.Clip != null)
            {
                if (state.Events(this, out AnimancerEvent.Sequence events))
                {
                    var retLayer = animLayer;
                    events.OnEnd = () =>
                    {
                        Play(returnClip.Clip, returnClip.FadeDuration, FadeMode.FromStart, retLayer);
                    };
                }
            }
            else if (fadeOutLayer)
            {
                RegisterOnEndFadeOut(state, animLayer, null);
            }
        }

        // --- IAnimationTimeController implementation ---

        public void Pause()
        {
            foreach (var layer in _layers)
            {
                layer.Playable.Pause();
            }
        }

        public void Resume()
        {
            foreach (var layer in _layers)
            {
                layer.Playable.Play();
            }
        }

        public void SetPlaySpeed(float speed)
        {
            foreach (var layer in _layers)
            {
                layer.Speed = speed;
            }
        }

        public void RegisterTimeContext(TimeContext timeContext)
        {
            UnRegisterTimeContext();

            _timeContext = timeContext;
            _timeContext.OnTimeScaleChanged += SetPlaySpeed;
        }

        void IAnimationTimeController.RegisterTimeContext(object timeContext)
        {
            RegisterTimeContext((TimeContext)timeContext);
        }

        public void UnRegisterTimeContext()
        {
            if (_timeContext != null)
            {
                _timeContext.OnTimeScaleChanged -= SetPlaySpeed;
                _timeContext = null;
            }
        }
    }
}
