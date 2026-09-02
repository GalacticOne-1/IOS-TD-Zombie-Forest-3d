namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class SuppressComponent : WeaponComponentBase
    {
        public bool IsSuppressionMode { get; set; }

        public bool IsInArc(float[] unitPos, float[] weaponPos, float[] forward)
        {
            // Упрощённая проверка — реальная через Vector3 в View
            return true;
        }
    }
}