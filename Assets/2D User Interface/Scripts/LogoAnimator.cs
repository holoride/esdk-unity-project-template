// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;

    public class LogoAnimator : MonoBehaviour
    {
        [SerializeField] private List<CanvasGroup> SplashScreenDisplayGroup;

        [SerializeField] private float SecondsPerLogo = 1f;

        [SerializeField] private AnimationCurve FadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 0.5f, 1f);
        [SerializeField] private AnimationCurve FadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 0.5f, 0f);

        [SerializeField] public UnityEvent OnSplashScreensShown;

        private void Start()
        {
            foreach (var group in SplashScreenDisplayGroup)
            {
                group.alpha = 0f;
            }

            PlaySplashScreenAnimations().Forget();
        }

        private async UniTaskVoid PlaySplashScreenAnimations()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f));

            foreach (var group in SplashScreenDisplayGroup)
            {
                await AnimateLogo(group);
            }

            OnSplashScreensShown?.Invoke();
        }

        private async UniTask AnimateLogo(CanvasGroup group)
        {
            var time = 0f;
            while (group.alpha < 1f)
            {
                time += Time.deltaTime;
                group.alpha = FadeInCurve.Evaluate(time);
                await UniTask.NextFrame();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(SecondsPerLogo));

            time = 0f;
            while (group.alpha > 0f)
            {
                time += Time.deltaTime;
                group.alpha = FadeOutCurve.Evaluate(time);
                await UniTask.NextFrame();
            }
        }
    }
}