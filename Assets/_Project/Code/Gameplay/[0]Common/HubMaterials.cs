using UnityEngine;

namespace Galactic1
{
    public class HubMaterials : MonoBehaviour, IGameService
    {

        [SerializeField] private CMaterial2D _material2D;

        public CMaterial2D Material2D => _material2D;

        [System.Serializable]
        public struct CMaterial2D
        {
            public PhysicsMaterial2D normal, frictionZero;
        }
    }
}