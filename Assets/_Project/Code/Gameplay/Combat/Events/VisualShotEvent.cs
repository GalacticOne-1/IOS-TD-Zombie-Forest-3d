using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Visual notification that a shot occurred.
    /// No gameplay authority.
    /// </summary>
    public readonly struct VisualShotEvent : IEvent
    {
        public readonly IUnitSceneContext Attacker;

        public readonly Vector3 Origin;
        public readonly Vector3 Forward;
        public readonly Quaternion MuzzleRotation;

        public readonly FireRequest Request;
        public readonly WeaponDefinitionData Weapon;

        public VisualShotEvent(
            IUnitSceneContext attacker,
            Vector3 origin,
            Vector3 forward,
            Quaternion muzzleRotation,
            FireRequest request,
            WeaponDefinitionData weapon)
        {
            Attacker = attacker;
            Origin = origin;
            Forward = forward;
            MuzzleRotation = muzzleRotation;
            Request = request;
            Weapon = weapon;
        }
    }
}