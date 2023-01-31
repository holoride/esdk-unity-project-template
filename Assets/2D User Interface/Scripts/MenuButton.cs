// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    public class MenuButton : UnityEngine.UI.Button
    {
        [SerializeField] 
        private UnityEvent onSelected;

        public UnityEvent OnSelected => onSelected;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            this.onSelected.Invoke();
        }
    }
}