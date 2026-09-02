namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    /// <summary>
    /// Отвечает за прочность оружия.
    /// Единственный источник истины по durability.
    /// </summary>
    public sealed class DurabilityComponent : WeaponComponentBase
    {
        private WeaponEntity _entity;

        public int Current { get; private set; }
        public int Max { get; private set; }

        public float Normalized => Max > 0 ? (float)Current / Max : 0f;

        public override void OnEquip(WeaponEntity entity)
        {
            _entity = entity;

            Max = entity.Module.Item.Physical.maxDurability;
        }

        public void RestoreDurability(int durability)
        {
            Current = durability;
            _entity.RaiseDurabilityChanged(Current, Max);
        }

        public override void OnFireExecuted(WeaponEntity entity)
        {
            // === уменьшаем прочность оружия, пока 1 за выстрел
            Current--;
            
            
            if (Current < 0) 
                Current = 0;
            
            _entity.RaiseDurabilityChanged(Current, Max); // WeaponInventorySync передаст новое значение прочности

            if (Current == 0)
            {
                // можно добавить:
                // _entity.SetState(WeaponState.Broken);
            }
        }

    }
}