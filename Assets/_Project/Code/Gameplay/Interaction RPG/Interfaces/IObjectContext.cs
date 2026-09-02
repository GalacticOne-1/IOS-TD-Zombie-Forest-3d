using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    public interface IObjectContext
    {
        Vector3 PivotCenter();

        Vector3 PivotCenterBottom();
    }
}