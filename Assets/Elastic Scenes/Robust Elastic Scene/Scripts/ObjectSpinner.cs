// Copyright (c) holoride GmbH. All Rights Reserved.

using System;

namespace Holoride.ElasticSDKTemplate
{
    using UnityEngine;

    public class ObjectSpinner : MonoBehaviour
    {
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
