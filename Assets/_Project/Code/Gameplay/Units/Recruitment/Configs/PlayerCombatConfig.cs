using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    [CreateAssetMenu(
        fileName = "PlayerCombatConfig",
        menuName = "Game Configs/Player/Player Combat Config")]
    public sealed class PlayerCombatConfig : ScriptableObject
    {
        public float AttackRange;
        public float HitRange;
        public float Damage;
        public float Cooldown;
        public float ReadyToAttackAngle;
    }
}