using System;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class HeatComponent : WeaponComponentBase
    {
        public float Heat    { get; private set; }
        public bool  IsHot   => Heat >= _entity?.Definition.OverheatThreshold;

        private WeaponEntity _entity;
        private bool         _inCooldown;
        private float        _cooldownTimer;

        public override void OnEquip(WeaponEntity entity) => _entity = entity;

        public override void Tick(float dt)
        {
            if (_entity == null) return;
            var def = _entity.Definition;

            if (_inCooldown)
            {
                _cooldownTimer -= dt;
                Heat -= def.HeatCoolRate * dt;
                Heat  = Math.Max(0f, Heat);

                if (_cooldownTimer <= 0f)
                {
                    _inCooldown = false;
                    _entity.SetState(WeaponState.Ready);
                }
            }
            else if (Heat > 0f)
            {
                Heat -= def.HeatCoolRate * dt;
                Heat  = Math.Max(0f, Heat);
            }
        }

        public override void OnFireExecuted(WeaponEntity entity)
        {
            var def = entity.Definition;
            Heat += def.HeatPerShot;

            if (Heat >= def.OverheatThreshold)
            {
                Heat           = def.OverheatThreshold;
                _inCooldown    = true;
                _cooldownTimer = def.CooldownSec;
                entity.SetState(WeaponState.Overheated);
                entity.RaiseOverheated();
            }
        }
    }
}