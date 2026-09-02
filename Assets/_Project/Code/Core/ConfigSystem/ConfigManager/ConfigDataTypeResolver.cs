using System;
using System.Collections.Generic;
using Gameplay;

namespace Galactic1.Configs
{
    public static class ConfigDataTypeResolver
    {
        private static readonly Dictionary<Type, Type> map = new()
        {
            // { typeof(BuildConfig), typeof(BuildConfig.Wrapper) },
            // { typeof(CombatEquipmentConfig), typeof(CombatEquipmentConfig.Wrapper) },
            // { typeof(WeaponConfig), typeof(WeaponConfig.Wrapper) },
        };

        public static Type Resolve(Type configType)
        {
            return map.TryGetValue(configType, out var dataType) ? dataType : null;
        }
    }

}