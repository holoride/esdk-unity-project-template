// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System.Collections;
    using System.Collections.Generic;
    using ElasticSDK;
    using Unity.VisualScripting;
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

        private Scene currentSubscene = default(Scene);

        private void Awake()
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
                bool hasFadedOut = false;
                if (this.sceneSwitcher.SceneTransitionController != null)
                {
                    this.sceneSwitcher.SceneTransitionController.PlayFinalDisappearAnimation(() => hasFadedOut = true);
                }
                else
                {
                    hasFadedOut = true;
                }
                
                yield return new WaitUntil(() => hasFadedOut);

                if (this.currentSubscene != default(Scene))
                {
                    var asyncUnloadOperation = SceneManager.UnloadSceneAsync(this.currentSubscene);
                    yield return new WaitUntil(() => asyncUnloadOperation.isDone);
                }

                var asyncLoadOperation = LoadAndConnectScene(this.autoSwitchingSubscenes[i]);
                yield return new WaitUntil(() => asyncLoadOperation.isDone);
                
                yield return waitForInterval;
            }
        }

        private AsyncOperation LoadAndConnectScene(string sceneName)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.completed += test =>
            {
                var loadedSubscene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                
                if (this == null)
                {
                    SceneManager.UnloadSceneAsync(loadedSubscene);
                    return;
                }

                this.currentSubscene = loadedSubscene;
                FindObjectOfType<ElasticSceneGenerator>().GenerationOrigin = this.generationOrigin;
                this.sceneSwitcher.SceneTransitionController = FindObjectOfType<SceneTransitionController>();
            };

            return asyncOperation;
        }
    }
}
