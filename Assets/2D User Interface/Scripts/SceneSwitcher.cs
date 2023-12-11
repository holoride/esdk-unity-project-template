// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Fades out and switches the scene.
    /// </summary>
    public class SceneSwitcher : MonoBehaviour
    {
        [Tooltip("An optional FadeTransitionController. If empty, the current scene won't fade out but swap instantly.")]
        [SerializeField] 
        private FadeTransitionController fadeTransitionController;
        
        public FadeTransitionController FadeTransitionController
        {
            get => this.fadeTransitionController;
            set => this.fadeTransitionController = value;
        }
        
        public void SwitchToScene(string sceneName)
        {
            if (this.fadeTransitionController == null)
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
            else
            {
                this.fadeTransitionController.PlayFinalDisappearAnimation(
                    () => SceneManager.LoadSceneAsync(sceneName));
            }
        }
    }
}
