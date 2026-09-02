using UnityEngine;

namespace Galactic1.Code.Cameras
{
    public class CameraCorridorBounds : MonoBehaviour
    {
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        public Vector2 Min => minBounds;
        public Vector2 Max => maxBounds;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Camera")) return;

            var cam = other.GetComponentInParent<CameraController>();
            cam.SetBounds(Min, Max);
        }

        

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = (minBounds + maxBounds) * 0.5f;
            Vector3 size = maxBounds - minBounds;
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}