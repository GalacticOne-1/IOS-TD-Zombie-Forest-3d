using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Fast collider → damage receiver lookup.
    /// Avoids TryGetComponent spam in combat systems.
    /// </summary>
    public static class HitboxRegistry
    {
        private static readonly Dictionary<Collider, DamageReceiverProxy> _map = new();
        
        public static void Clear() => _map.Clear();

        public static void Register(
            Collider collider,
            DamageReceiverProxy receiver)
        {
            if (collider == null || receiver == null)
                return;

            _map[collider] = receiver;
        }

        public static void Unregister(Collider collider)
        {
            if (collider == null)
                return;

            _map.Remove(collider);
        }

        public static bool TryGetReceiver(Collider collider, out DamageReceiverProxy receiver)
        {
            if (!_map.TryGetValue(collider, out receiver))
                return false;

            if (receiver == null)
            {
                _map.Remove(collider);
                return false;
            }

            return true;
        }
    }
}