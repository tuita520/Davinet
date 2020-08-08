using UnityEngine;

namespace Davinet.Sample
{
    public class SmoothFollowCamera : MonoBehaviour
    {
        public Transform target;

        [SerializeField]
        float smoothTime = 1;

        private Vector3 offset;

        private Vector3 velocity;

        private void Start()
        {
            offset = transform.position - target.position;
        }

        private void LateUpdate()
        {
            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);
        }
    }
}
