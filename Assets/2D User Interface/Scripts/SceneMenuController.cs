// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Controls the UI to select and switch to another scene, or reload the current scene.
    /// </summary>
    public class SceneMenuController : MonoBehaviour
    {
        [Tooltip("The scenes to switch in the background.")]
        [SerializeField] private List<string> switchableScenes;
        
        [Tooltip("The menu to display the scene selection.")]
        [SerializeField]
        private Menu menu;
        
        [Tooltip("The canvas to toggle the UI.")]
        [SerializeField] 
        private GameObject canvas;
        
        [Tooltip("Gets invoked when the menu gets opened.")]
        [SerializeField] 
        private UnityEvent onMenuOpened;
        
        [Tooltip("Gets invoked when the menu gets closed.")]
        [SerializeField] 
        private UnityEvent onMenuClosed;
        
        [Tooltip("Gets invoked when a menu entry gets clicked.")]
        [SerializeField] 
        private UnityEvent<string> onSceneEntryClicked;
        
        private bool isChangingLevel = false;

        private void Awake()
        {
            this.canvas.SetActive(false);

            foreach (var sceneName in this.switchableScenes)
            {
                string buttonText = sceneName == SceneManager.GetActiveScene().name
                    ? $"{sceneName}<line-height=0>\n<align=right>(reload)<line-height=1em>"
                    : sceneName;

                this.menu.AddButton(buttonText, () =>
                {
                    this.isChangingLevel = true;
                    this.canvas.SetActive(false);
                    this.onSceneEntryClicked.Invoke(sceneName);
                });
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
            {
                Debug.Log("SUBMIT isChangingLevel? " + isChangingLevel);
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
