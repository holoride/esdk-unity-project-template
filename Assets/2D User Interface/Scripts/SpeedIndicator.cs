// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using ElasticSDK;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Writes the current vehicle speed into a text field.
    /// </summary>
    public class SpeedIndicator : MonoBehaviour
    {
        [Tooltip("The text to be replaced by the current vehicle speed.")]
        [SerializeField] 
        private TMP_Text textField;

        void Update()
        {
            int speed = (int) StateReceiver.VehicleSensorState.VehicleSpeed_Kmh;
            this.textField.text = $"{speed} km/h";
        }
    }
}
