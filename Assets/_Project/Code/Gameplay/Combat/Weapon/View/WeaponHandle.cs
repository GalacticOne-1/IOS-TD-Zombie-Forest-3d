using System;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    // ─────────────────────────────────────────────
    //  WeaponHandle — токен владения оружием
    //  Dispose снимает и чистит всё
    // ─────────────────────────────────────────────

    public sealed class WeaponHandle : IDisposable
    {
        public WeaponEntity Entity { get; private set; }
        public WeaponView View { get; }

        private bool _disposed;

        public WeaponHandle(WeaponEntity entity, WeaponView view)
        {
            Entity = entity;
            View = view;
        }

       
        public void SetVisible(bool visible)
        {
            if (View == null)
                return;

            View.gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Entity.Unequip();
            Entity = null;
            View.Unbind();
            Destroy(View.gameObject);
        }

        private static void Destroy(GameObject go)
        {
            if (go != null) UnityEngine.Object.Destroy(go);
        }
    }
}