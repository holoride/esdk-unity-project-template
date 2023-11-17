// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using UnityEngine.Events;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Controls the UI to select and switch to another scene, or reload the current scene.
    /// </summary>
    public class SceneSwitcherController : MonoBehaviour
    {
        [Tooltip("The menu to display the scene selection.")]
        [SerializeField]
        private Menu menu;
        
        [Tooltip("The canvas to toggle the UI.")]
        [SerializeField] 
        private GameObject canvas;

        [Tooltip("The LocalizationEvents to disable before the scene unloads.")]
        [SerializeField] 
        private LevelSwitcher levelSwitcher;
        
        [Tooltip("Gets invoked when the menu gets opened.")]
        [SerializeField] 
        private UnityEvent onMenuOpened;
        
        [Tooltip("Gets invoked when the menu gets closed.")]
        [SerializeField] 
        private UnityEvent onMenuClosed;
        
        private bool isChangingLevel = false;

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

                this.menu.AddButton(buttonText, () =>
                {
                    this.isChangingLevel = true;
                    this.canvas.SetActive(false);
                    this.levelSwitcher.SwitchLevel(sceneName);
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
