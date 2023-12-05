// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.Serialization;

    /// <summary>
    /// Controls the UI to select and switch to another scene, or reload the current scene.
    /// </summary>
    public class SceneSwitcher : MonoBehaviour
    {
        [Tooltip("The scenes to switch in the background.")]
        [SerializeField] private List<string> switchableScenes;
        
        [Tooltip("The menu to display the scene selection.")]
        [SerializeField]
        private Menu menu;
        
        [Tooltip("The canvas to toggle the UI.")]
        [SerializeField] 
        private GameObject canvas;

        [Tooltip("The LocalizationEvents to disable before the scene unloads.")]
        [SerializeField] 
        private SceneTransitionController sceneTransitionController;
        
        [Tooltip("Gets invoked when the menu gets opened.")]
        [SerializeField] 
        private UnityEvent onMenuOpened;
        
        [Tooltip("Gets invoked when the menu gets closed.")]
        [SerializeField] 
        private UnityEvent onMenuClosed;
        
        private bool isChangingLevel = false;

        public SceneTransitionController SceneTransitionController
        {
            get => this.sceneTransitionController;
            set => this.sceneTransitionController = value;
        }

        private void Awake()
        {
            this.canvas.SetActive(false);

            int sceneCount = SceneManager.sceneCountInBuildSettings;

            foreach (var sceneName in this.switchableScenes)
            {
                string buttonText = sceneName == SceneManager.GetActiveScene().name
                    ? $"{sceneName}<line-height=0>\n<align=right>(reload)<line-height=1em>"
                    : sceneName;

                this.menu.AddButton(buttonText, () =>
                {
                    this.isChangingLevel = true;
                    this.canvas.SetActive(false);

                    if (this.sceneTransitionController == null)
                    {
                        SceneManager.LoadScene(sceneName);
                    }
                    else
                    {
                        this.sceneTransitionController.PlayFinalDisappearAnimation(() => SceneManager.LoadScene(sceneName));
                    }
                });
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
            {
                if (!this.canvas.activeInHierarchy && !isChangingLevel)
                {
                    this.canvas.SetActive(true);
                    this.onMenuOpened.Invoke();
                }
                else
                {
                    if (Input.GetButtonDown("Cancel"))
                    {
                        this.canvas.SetActive(false);
                        this.onMenuClosed.Invoke();
                    }

                    this.menu.RestorePreviousSelection();
                }
            }
        }
    }
}
