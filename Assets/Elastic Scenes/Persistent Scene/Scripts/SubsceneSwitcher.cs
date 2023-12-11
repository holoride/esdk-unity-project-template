// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using ElasticSDK;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SubsceneSwitcher : MonoBehaviour
    {
        [Tooltip("The generation origin.")] 
        [SerializeField]
        private Transform generationOrigin;

        [Tooltip("The scene switcher.")]
        [SerializeField]
        private SceneSwitcher sceneSwitcher;

        [Tooltip("The scenes to switch automatically. Automatic switching is disabled if the list is empty.")]
        [SerializeField]
        private List<string> autoSwitchingSubscenes;

        [Tooltip("The time interval in seconds before the next switch.")] 
        [SerializeField]
        private float autoSwitchAfterSeconds = 1;

        private Scene currentSubscene;
        
        private CancellationTokenSource destroyCancellationTokenSource = new CancellationTokenSource();
        
        private void Awake()
        {
            if (this.autoSwitchingSubscenes.Count > 0)
            {
                this.KeepSwitchingSubscenes().Forget();
            }
        }
        
        private void OnDestroy()
        {
            this.destroyCancellationTokenSource.Cancel();
            this.destroyCancellationTokenSource.Dispose();
        }

        private async UniTask KeepSwitchingSubscenes()
        {
            int i = 0;
            
            while (!this.destroyCancellationTokenSource.IsCancellationRequested)
            {
                await this.SwitchSubscene(this.autoSwitchingSubscenes[i]);
                await UniTask.Delay(TimeSpan.FromSeconds(this.autoSwitchAfterSeconds), cancellationToken: this.destroyCancellationTokenSource.Token);
                i = (i + 1) % this.autoSwitchingSubscenes.Count;
            }
        }

        public async UniTask SwitchSubscene(string sceneName)
        {
            if (this.sceneSwitcher.FadeTransitionController != null)
            {
                bool hasFadedOut = false;
                this.sceneSwitcher.FadeTransitionController.PlayFinalDisappearAnimation(() => hasFadedOut = true);
                await UniTask.WaitUntil(() => hasFadedOut, cancellationToken: this.destroyCancellationTokenSource.Token);
            }
                
            if (this.currentSubscene != default)
            {
                var asyncUnloadOperation = SceneManager.UnloadSceneAsync(this.currentSubscene);
                await UniTask.WaitUntil(() => asyncUnloadOperation.isDone, cancellationToken: this.destroyCancellationTokenSource.Token);
            }

            var asyncLoadOperation = this.LoadAndConnectSubscene(sceneName);
            await UniTask.WaitUntil(() => asyncLoadOperation.isDone, cancellationToken: this.destroyCancellationTokenSource.Token);
        }

        private AsyncOperation LoadAndConnectSubscene(string sceneName)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.completed += _ =>
            {
                var loadedSubscene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                
                if (this == null)
                {
                    SceneManager.UnloadSceneAsync(loadedSubscene);
                    return;
                }

                this.currentSubscene = loadedSubscene;
                FindObjectOfType<ElasticSceneGenerator>().GenerationOrigin = this.generationOrigin;
                this.sceneSwitcher.FadeTransitionController = FindObjectOfType<FadeTransitionController>();
            };

            return asyncOperation;
        }
    }
}
