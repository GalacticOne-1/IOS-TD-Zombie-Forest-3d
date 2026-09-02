
using System;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    /// <summary>
    /// Простой враг — реализует ITargetable + интеракт (атака).
    /// Содержит несколько коллайдеров на prefab: Hitbox (layer EnemyHitbox), InteractCollider (layer Interaction).
    /// </summary>
    public class EnemyInteractable : InteractableBase, ITargetable
    {
        [SerializeField] private Sprite icon;
        
        [Header("Stats")]
        public float maxHealth = 100f;
        public float health;

        
        public override ActionType ActionType => ActionType.None;
        
        public event Action<float> OnHealthChanged;
        public event Action OnDied;
        
        
        

        private void Start()
        {
            health = maxHealth;
        }

        
        
        public override void Interact(Transform interactor)
        {
            // ❌ Ничего не делаем!
            // Враг НЕ должен реагировать на кнопку "действие".
        }

        public void ReceiveAttack(Transform attacker)
        {
            // Взаимодействие через кнопку — по умолчанию наносим урон (агрессия)
            TakeDamage(10f);
        }

        public void TakeDamage(float dmg)
        {
            health -= dmg;
            OnHealthChanged?.Invoke(health);
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            enabledForInteraction = false;
            OnDied?.Invoke();
            Debug.Log("[Enemy] Died: " + name);
            // Включить Corpse collider / поменять слой → превратить в lootable corpse
        }

        // ITargetable
        public float MaxHealth => maxHealth;
        float ITargetable.Health => health;
        float ITargetable.MaxHealth => MaxHealth;
        bool ITargetable.IsAlive => health > 0;

        public override InteractionInfo GetInfo()
        {
            return new InteractionInfo { Name = "Enemy", Icon = icon, IsAvailable = IsAvailable };
        }

    }
}