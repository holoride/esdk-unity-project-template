// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System.Collections;
    using System.Collections.Generic;
    using ElasticSDK;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SubsceneSwitcher : MonoBehaviour
    {
        [Tooltip("The generation origin.")] [SerializeField]
        private Transform generationOrigin;

        [Tooltip("The sceneName switcher.")] [SerializeField]
        private SceneSwitcher sceneSwitcher;

        [Tooltip("The scenes to switch automatically. Automatic switching is disabled if the list is empty.")]
        [SerializeField]
        private List<string> autoSwitchingSubscenes;

        [Tooltip("The time interval in seconds before the next switch.")] [SerializeField]
        private float autoSwitchAfterSeconds = 10;

        private string currentSceneName = null;

        private void Start()
        {
            if (this.autoSwitchingSubscenes.Count > 0)
            {
                StartCoroutine(this.KeepSwitchingEnvironments());
            }
        }

        private IEnumerator KeepSwitchingEnvironments()
        {
            var waitForInterval = new WaitForSeconds(this.autoSwitchAfterSeconds);

            for (var i = 0;; i = (i + 1) % this.autoSwitchingSubscenes.Count)
            {
                this.SwitchSubscene(this.autoSwitchingSubscenes[i]);
                yield return waitForInterval;
            }
        }

        public void SwitchSubscene(string sceneName)
        {
            if (this.currentSceneName == null)
            {
                this.LoadAndConnectScene(sceneName);
                return;
            }

            if (this.sceneSwitcher.SceneTransitionController == null)
            {
                SceneManager.UnloadSceneAsync(this.currentSceneName);
                this.LoadAndConnectScene(sceneName);
                return;
            }

            this.sceneSwitcher.SceneTransitionController.PlayFinalDisappearAnimation(() =>
            {
                SceneManager.UnloadSceneAsync(this.currentSceneName);
                this.LoadAndConnectScene(sceneName);
            });
        }

        private void OnDestroy()
        {
            if (this.currentSceneName != null)
            {
                SceneManager.UnloadScene(this.currentSceneName);
            }
        }

        private void LoadAndConnectScene(string sceneName)
        {
            this.currentSceneName = sceneName;
            var a = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            a.completed += _ =>
            {
                FindObjectOfType<ElasticSceneGenerator>().GenerationOrigin = this.generationOrigin;
                this.sceneSwitcher.SceneTransitionController = FindObjectOfType<SceneTransitionController>();
            };
        }
    }
}
