using System;
using System.Collections;
using System.Collections.Generic;
using Holoride.ElasticSDK;
using Holoride.ElasticSDKTemplate;
using UnityEditor;
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
    private List<SceneAsset> autoSwitchingSubscenes;
    
    [Tooltip("The time interval in seconds before the next switch.")]
    [SerializeField] 
    private float autoSwitchAfterSeconds = 10;
    
    private SceneAsset currentScene = null;

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

    public void SwitchSubscene(SceneAsset scene)
    {
        if (this.currentScene == null)
        {
            this.LoadAndConnectScene(scene);
            return;
        }
        
        if (this.sceneSwitcher.SceneTransitionController == null)
        {
            SceneManager.UnloadSceneAsync(this.currentScene.name);
            this.LoadAndConnectScene(scene);
            return;
        }

        this.sceneSwitcher.SceneTransitionController.PlayFinalDisappearAnimation(() =>
        {
            SceneManager.UnloadSceneAsync(this.currentScene.name);
            this.LoadAndConnectScene(scene);
        });
    }

    private void OnDestroy()
    {
        if (this.currentScene != null)
        {
            SceneManager.UnloadScene(this.currentScene.name);
        }
    }

    private void LoadAndConnectScene(SceneAsset scene)
    {
        this.currentScene = scene;
        var a = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
        a.completed += _ =>
        {
            FindObjectOfType<ElasticSceneGenerator>().GenerationOrigin = this.generationOrigin;
            this.sceneSwitcher.SceneTransitionController = FindObjectOfType<SceneTransitionController>();
        };
    }
}
