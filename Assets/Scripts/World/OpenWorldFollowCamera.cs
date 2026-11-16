using UnityEngine;

namespace SandsOfLegends.World
{
    /// <summary>Smoothed third-person camera for the open-world preview.</summary>
    public sealed class OpenWorldFollowCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 7.5f, -10.5f);
        [SerializeField] private float followSharpness = 7f;
        [SerializeField] private float lookHeight = 1.35f;
        private Transform target;

        public void SetTarget(Transform value) { target = value; }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSharpness * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}