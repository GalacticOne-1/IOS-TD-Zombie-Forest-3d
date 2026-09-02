using UnityEngine;

namespace Galactic1
{
    public class StructureBinder : MonoBehaviour
    {
        public void Bind(StructureViewModel viewModel)
        {
            Vector2 v = viewModel.Position.CurrentValue;
            transform.position = v;
        }
    }
}