// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using System;
    using ElasticSDK;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Writes the current vehicle speed into a text field.
    /// </summary>
    public class VehicleValueText : MonoBehaviour
    {
        [Tooltip("The text to be replaced by the current vehicle speed.")]
        [SerializeField] 
        private TMP_Text textField;

        [FormerlySerializedAs("indication")] [SerializeField] 
        private VehicleValue vehicleValue;

        private string formattableText;

        enum VehicleValue
        {
            Speed,
            Yaw,
            Pitch,
            Roll, 
            Longitude,
            Latitude,
        }

        private void Start()
        {
            this.formattableText = String.IsNullOrEmpty(textField.text) ? "{0:N2}" : textField.text;
        }

        void Update()
        {
            double value = 0;
            
            switch (this.vehicleValue)
            {
                case VehicleValue.Speed:
                    value = StateReceiver.VehicleSensorState.VehicleSpeed_Kmh;
                    break;
                case VehicleValue.Yaw:
                    value = StateReceiver.VehicleSensorState.VehicleHeading_Deg;
                    break;
                case VehicleValue.Pitch:
                    value = StateReceiver.VehicleSensorState.VehiclePitch_Deg;
                    break;
                case VehicleValue.Roll:
                    value = StateReceiver.VehicleSensorState.VehicleRoll_Deg;
                    break;
                case VehicleValue.Longitude:
                    value = StateReceiver.VehicleSensorState.GeoCoordinate.Longitude;
                    break;
                case VehicleValue.Latitude:
                    value = StateReceiver.VehicleSensorState.GeoCoordinate.Latitude;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.textField.text = String.Format(this.formattableText, value);
        }
    }
}
