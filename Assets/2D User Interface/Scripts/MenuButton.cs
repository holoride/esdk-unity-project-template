// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    /// <summary>
    /// A button that exposes the OnSelect event.
    /// </summary>
    public class MenuButton : UnityEngine.UI.Button
    {
        [SerializeField] 
        private UnityEvent onSelected;

        /// <summary>
        /// The exposed OnSelect event.
        /// </summary>
        public UnityEvent OnSelected => onSelected;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            this.onSelected.Invoke();
        }
    }
}