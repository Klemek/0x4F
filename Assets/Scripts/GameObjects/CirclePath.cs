using System;
using UnityEngine;

namespace GameObjects
{
    [ExecuteInEditMode]
    public class CirclePath : MonoBehaviour
    {
        public Vector3 root;
        public Vector3 range;
        public Vector3 speed;
        public Vector3 offset;

        private void Start()
        {
            UpdatePosition();
        }

        private void FixedUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            transform.position = new Vector3(
                root.x + range.x * Mathf.Cos(Time.time * speed.x + Mathf.Deg2Rad * offset.x),
                root.y + range.y * Mathf.Cos(Time.time * speed.y + Mathf.Deg2Rad * offset.y),
                root.z + range.z * Mathf.Cos(Time.time * speed.z + Mathf.Deg2Rad * offset.z)
            );
        }
        
        private void OnValidate()
        {
            UpdatePosition();
        }
    }
}