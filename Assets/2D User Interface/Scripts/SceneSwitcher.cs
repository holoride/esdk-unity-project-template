// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using ElasticSDK;
    using ElasticSDK.FadeToBackground.BuildInRenderPipeline;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Controls the UI to select and switch to another scene, or reload the current scene. Triggers the fadeout
    /// animation on scene changes if the FadeToBackgroundManager is linked. Disables a LocalizationEvent component to
    /// not interrupt an ongoing fadeout animation before a scene change. 
    /// </summary>
    public class SceneSwitcher : MonoBehaviour
    {
        [Tooltip("The menu to display the scene selection.")]
        [SerializeField]
        private Menu menu;
        
        [Tooltip("The canvas to toggle the UI.")]
        [SerializeField] 
        private GameObject canvas;
        
        [Tooltip("The LocalizationEvents to disable before the scene unloads.")]
        [SerializeField] 
        private LocalizationEvents fadeLocalizationEvents;
        
        [Tooltip("The FadeToBackgroundManager to fade the scene in or out.")]
        [SerializeField] 
        private FadeToBackgroundManager fadeToBackgroundManager;
        
        private bool isLoading = false;

        private void Awake()
        {
            this.canvas.SetActive(false);

            int sceneCount = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sceneCount; i++)
            {
                // this is a workaround due to this issue: https://forum.unity.com/threads/getscenebybuildindex-problem.452560
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);

                string buttonText = sceneName == SceneManager.GetActiveScene().name
                    ? $"{sceneName}<line-height=0>\n<align=right>(reload)<line-height=1em>"
                    : sceneName;

                this.menu.AddButton(buttonText, () => this.LoadScene(sceneName));
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
            {
                if (!this.canvas.activeInHierarchy && !isLoading)
                {
                    this.canvas.SetActive(true);
                }
                else
                {
                    if (Input.GetButtonDown("Cancel"))
                    {
                        this.canvas.SetActive(false);
                    }

                    this.menu.RestorePreviousSelection();
                }
            }
        }

        private void LoadScene(string scene)
        {
            this.isLoading = true;

            if (this.fadeToBackgroundManager == null)
            {
                SceneManager.LoadScene(scene);
                return;
            }

            this.canvas.SetActive(false);

            if (this.fadeLocalizationEvents != null)
            {
                this.fadeLocalizationEvents.enabled = false;
            }

            if (this.fadeToBackgroundManager.CurrentAnimationSecond ==
                this.fadeToBackgroundManager.AnimationBeginSecond)
            {
                SceneManager.LoadScene(scene);
            }
            else
            {
                this.fadeToBackgroundManager.OnDisappearAnimationFinished.AddListener(() =>
                    SceneManager.LoadScene(scene));
                this.fadeToBackgroundManager.PlayDisappearAnimation();
            }
        }
    }
}
