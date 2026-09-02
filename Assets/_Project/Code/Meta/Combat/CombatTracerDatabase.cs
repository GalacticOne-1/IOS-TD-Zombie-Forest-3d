using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Visual
{
    /// <summary>
    /// Runtime lookup: WeaponType → AmmoDefinition (pool key для TracerProjectile).
    ///
    /// Строится из всех WeaponDefinition SO в ConfigProvider.
    /// Если несколько WeaponDefinition имеют одинаковый WeaponType —
    /// используется первый найденный (все пистолеты одного типа
    /// разделяют одну AmmoDefinition).
    ///
    /// Используется только FakeBulletSystem.
    /// </summary>
    public sealed class CombatTracerDatabase
    {
        private readonly Dictionary<WeaponType, AmmoDefinition> _map = new();
        private AmmoDefinition _fallback;

        public CombatTracerDatabase(IReadOnlyList<ItemConfig> rawConfigs)
        {
            foreach (var raw in rawConfigs)
            {
                if (raw is ItemConfig config && config.HasModule<WeaponModule>())
                {
                    var weapon = config.Weapon;
                    if (weapon.Definition?.supportedAmmo == null)
                    {
                        Debug.LogError($"[CombatTracerDatabase] Weapon [{weapon.Item.name}] not have AmmoDefinition");
                        continue;
                    }

                    var weaponType = weapon.Info.weaponType;

                    if (!_map.ContainsKey(weaponType))
                        _map[weaponType] = weapon.Definition.supportedAmmo;

                    _fallback ??= weapon.Definition.supportedAmmo;
                }
            }

            if (_fallback == null)
                Debug.LogError("[CombatTracerDatabase] No WeaponDefinition with AmmoDefinition found.");
        }

        public AmmoDefinition Get(WeaponType weaponType)
        {
            if (_map.TryGetValue(weaponType, out var ammo))
                return ammo;

            return _fallback;
        }
    }
}