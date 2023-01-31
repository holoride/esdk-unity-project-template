using Holoride.ElasticSDK;
using TMPro;
using UnityEngine;

public class SpeedIndicator : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textField;
    
    void Update()
    {
        int speed = (int)StateReceiver.VehicleSensorState.VehicleSpeed_Kmh;
        this.textField.text = $"{speed} km/h";
    }
}
