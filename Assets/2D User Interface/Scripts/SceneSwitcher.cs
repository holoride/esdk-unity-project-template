// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using ElasticSDK;
    using ElasticSDK.FadeToBackground.BuildInRenderPipeline;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneSwitcher : MonoBehaviour
    {
        [SerializeField] 
        private Menu menu;
        
        [SerializeField] 
        private GameObject canvas;
        
        [SerializeField] 
        private LocalizationEvents localizationEvents;
        
        [SerializeField] 
        private FadeToBackgroundManager fadeToBackgroundManager;
        
        private bool isLoading = false;

        private void Awake()
        {
            this.canvas.SetActive(false);

            int sceneCount = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sceneCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);

                string buttonText = sceneName == SceneManager.GetActiveScene().name
                    ? $"{sceneName}<line-height=0>\n<align=right>(reload)<line-height=1em>"
                    : sceneName;

                var button = this.menu.AddButton(buttonText, () => this.LoadScene(sceneName));
            }
        }

        private void Update()
        {
            if (Input.anyKeyDown)
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

            if (this.localizationEvents != null)
            {
                this.localizationEvents.enabled = false;
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
