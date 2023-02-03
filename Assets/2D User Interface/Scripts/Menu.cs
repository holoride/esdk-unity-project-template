// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// A simple menu to add MenuButton instances to.
    /// </summary>
    public class Menu : MonoBehaviour
    {
        [Tooltip("A button prefab that requires a MenuButton component.")]
        [SerializeField] 
        private GameObject buttonPrefab;
        
        [Tooltip("The ScrollRect which is parent to the buttons.")]
        [SerializeField] 
        private ScrollRect scrollRect;

        private readonly float adjustSeconds = 0.2f;

        private RectTransform contentRectTransform;
        private MenuButton lastSelected;
        private Vector2 targetScrollPosition;
        private Vector2 targetScrollVelocity;
        private Vector2 previousAnchoredPosition;

        private void Awake()
        {
            this.contentRectTransform = this.scrollRect.content.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Instantiates and adds a MenuButton to the menu.
        /// </summary>
        /// <param name="text">
        /// The text displayed on the button.
        /// </param>
        /// <param name="action">
        /// The action to invoke if the button is pressed.
        /// </param>
        /// <returns>
        /// The MenuButton that has been instantiated and added to the menu.
        /// </returns>
        public Button AddButton(string text, UnityAction action)
        {
            var buttonGameObject = GameObject.Instantiate(this.buttonPrefab, this.scrollRect.content.transform);

            var button = buttonGameObject.GetComponent<MenuButton>();
            var buttonRectTransform = button.GetComponent<RectTransform>();
            button.onClick.AddListener(action);
            button.OnSelected.AddListener(() => this.lastSelected = button);
            button.OnSelected.AddListener(() => this.SnapTo(buttonRectTransform));

            var buttonText = buttonGameObject.GetComponentInChildren<TMP_Text>();
            buttonText.text = text;

            return button;
        }

        /// <summary>
        /// Restores the previous selection of the last selected MenuButton.
        /// </summary>
        public void RestorePreviousSelection()
        {
            if (this.scrollRect.content.transform.childCount > 0)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(this.lastSelected == null
                    ? this.scrollRect.content.transform.GetChild(0).gameObject
                    : this.lastSelected.gameObject);
            }
        }

        private void OnEnable()
        {
            this.RestorePreviousSelection();
        }

        private void SnapTo(RectTransform target)
        {
            Vector2 viewportLocalPosition = this.scrollRect.viewport.localPosition;
            Vector2 childLocalPosition = target.localPosition;
            Vector2 result = new Vector2(0, -viewportLocalPosition.y - childLocalPosition.y);

            this.targetScrollPosition = result;
        }

        private void Update()
        {
            var currentScrollPosition = this.contentRectTransform.anchoredPosition;
            if (Vector2.Distance(this.targetScrollPosition, currentScrollPosition) > 0.1f)
            {
                this.contentRectTransform.anchoredPosition = Vector2.SmoothDamp(
                    currentScrollPosition,
                    this.targetScrollPosition,
                    ref targetScrollVelocity,
                    this.adjustSeconds);

                this.scrollRect.normalizedPosition = new Vector2(
                    Mathf.Clamp01(this.scrollRect.normalizedPosition.x),
                    Mathf.Clamp01(this.scrollRect.normalizedPosition.y));
            }
        }
    }
}
