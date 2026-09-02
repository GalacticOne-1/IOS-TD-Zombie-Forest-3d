using System.Collections;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction
{
    public class DC : MonoBehaviour
    {
        public Vector3 Center;
        public float Radius = 1;
        public Color Color = Color.black;
        public bool IsDestroy = true;
        public float DestroyingDelay = 1;
        public bool DrawWireSphere = false;

        IEnumerator Start()
        {
            name = "DC";
            yield return new WaitForSeconds(DestroyingDelay);
            if (IsDestroy)
                Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color;
            if (!DrawWireSphere)
                Gizmos.DrawSphere(Center, Radius);
            else
                Gizmos.DrawWireSphere(Center, Radius);
        }
    }
}