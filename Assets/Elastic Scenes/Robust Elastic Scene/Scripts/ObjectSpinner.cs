// Copyright (c) holoride GmbH. All Rights Reserved.

namespace Holoride.ElasticSDKTemplate
{
    using UnityEngine;

    /// <summary>
    /// Spins an object around every euler axis.
    /// </summary>
    public class ObjectSpinner : MonoBehaviour
    {
        [Tooltip("The speed of the spin animation.")]
        [SerializeField] 
        private float speed = 20;

        private float randomStartAngle;

        private void Start()
        {
            this.randomStartAngle = Random.Range(0, 360);
        }

        private void Update()
        {
            float angle = this.randomStartAngle + Time.realtimeSinceStartup * this.speed;
            this.transform.rotation = Quaternion.Euler(Vector3.one * angle);
        }
    }
}
