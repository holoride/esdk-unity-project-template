// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using Holoride.ElasticSDK;
    using Holoride.ElasticSDK.FadeToBackground.BuildInRenderPipeline;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    ///<summary>
    /// Component to control level transitions.
    /// Triggers the fadeout animation on scene changes if the FadeToBackgroundManager is linked.
    /// Disables a LocalizationEvent component to not interrupt an ongoing fadeout animation before a scene change.
    /// </summary>
    public class LevelSwitcher : MonoBehaviour
    {
        [Tooltip("The LocalizationEvents to disable before the scene unloads.")] 
        [SerializeField]
        private LocalizationEvents fadeLocalizationEvents;

        [Tooltip("The FadeToBackgroundManager to fade the scene in or out.")] 
        [SerializeField]
        private FadeToBackgroundManager fadeToBackgroundManager;

        public void SwitchLevel(string levelName)
        {
            if (this.fadeToBackgroundManager == null)
            {
                SceneManager.LoadScene(levelName);
                return;
            }

            if (this.fadeLocalizationEvents != null)
            {
                this.fadeLocalizationEvents.enabled = false;
            }

            if (IsLevelCompletelyFadedOut())
            {
                SceneManager.LoadScene(levelName);
            }
            else
            {
                this.fadeToBackgroundManager.OnDisappearAnimationFinished.AddListener(() =>
                    SceneManager.LoadScene(levelName));
                this.fadeToBackgroundManager.PlayDisappearAnimation();
            }
        }

        private bool IsLevelCompletelyFadedOut()
        {
            return !this.fadeToBackgroundManager.IsPlayingForward && this.fadeToBackgroundManager.CurrentAnimationSecond <= this.fadeToBackgroundManager.AnimationEndSecond;
        }
    }
}
