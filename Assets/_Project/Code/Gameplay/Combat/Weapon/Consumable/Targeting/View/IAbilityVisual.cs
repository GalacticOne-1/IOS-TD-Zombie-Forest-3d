using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public interface IAbilityVisual
    {
        void Show();
        void Hide();

        void SetRadius(float smallRadius, float bigRadius);
        void Update(Vector3 position, bool valid);
    }
}