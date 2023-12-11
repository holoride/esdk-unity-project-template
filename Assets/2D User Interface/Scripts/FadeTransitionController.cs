// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System;
    using ElasticSDK;
    using ElasticSDK.FadeToBackground.BuildInRenderPipeline;
    using UnityEngine;

    ///<summary>
    /// Component to control scene transitions.
    /// Triggers the fadeout animation on scene changes if the FadeToBackgroundManager is linked.
    /// Disables a LocalizationEvent component to not interrupt an ongoing fadeout animation before a scene change.
    /// </summary>
    public class FadeTransitionController : MonoBehaviour
    {
        [Tooltip("The LocalizationEvents to disable before the scene unloads.")] 
        [SerializeField]
        private LocalizationEvents fadeLocalizationEvents;

        [Tooltip("The FadeToBackgroundManager to fade the scene in or out.")] 
        [SerializeField]
        private FadeToBackgroundManager fadeToBackgroundManager;

        public void PlayFinalDisappearAnimation(Action onDisappearAnimationFinished)
        {
            if (this.fadeToBackgroundManager == null)
            {
                onDisappearAnimationFinished.Invoke();
                return;
            }

            if (this.fadeLocalizationEvents != null)
            {
                this.fadeLocalizationEvents.enabled = false;
            }

            if (IsLevelCompletelyFadedOut())
            {
                onDisappearAnimationFinished.Invoke();
            }
            else
            {
                this.fadeToBackgroundManager.OnDisappearAnimationFinished.AddListener(() =>
                {
                    onDisappearAnimationFinished.Invoke();
                });
                this.fadeToBackgroundManager.PlayDisappearAnimation();
            }
        }

        private bool IsLevelCompletelyFadedOut()
        {
            return !this.fadeToBackgroundManager.IsPlayingForward && this.fadeToBackgroundManager.CurrentAnimationSecond <= this.fadeToBackgroundManager.AnimationBeginSecond;
        }
    }
}
