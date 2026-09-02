
using System;
using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Gameplay;
using Galactic1.RaidLoot.Enums;
using UnityEngine;

namespace Galactic1.RaidLoot.Authoring
{
    /// <summary>
    /// Кривая истощения контейнера.
    /// Определяет: при каком открытии какой multiplier бюджета и какие тиры доступны.
    /// Один ассет на тип контейнера (Military, Civilian, etc.) или один глобальный.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DepletionCurveConfig",
        menuName = "Game Configs/Loot/Depletion Curve Config")]
    public sealed class DepletionCurveConfig : ScriptableObject
    {
        [SerializeField] private List<DepletionStageRule> _stages = new()
        {
            new()
            {
                Stage = DepletionStage.Full, OpenIndex = 0, BudgetMultiplier = 1.00f, MaxTierAllowed = Tier.T3
            },
            new()
            {
                Stage = DepletionStage.Reduced, OpenIndex = 1, BudgetMultiplier = 0.50f, MaxTierAllowed = Tier.T2
            },
            new()
            {
                Stage = DepletionStage.Scarce, OpenIndex = 2, BudgetMultiplier = 0.20f, MaxTierAllowed = Tier.T1
            },
            new()
            {
                Stage = DepletionStage.Empty, OpenIndex = 3, BudgetMultiplier = 0.00f, MaxTierAllowed = Tier.T1
            },
        };

        [Serializable]
        public struct DepletionStageRule
        {
            public DepletionStage Stage;

            [Tooltip("Порядковый номер открытия (0 = первое).")]
            public int OpenIndex;

            [Tooltip("Множитель бюджета. 1.0 = полный, 0.0 = пусто.")] [Range(0f, 1f)]
            public float BudgetMultiplier;

            [Tooltip("Максимальный тир предмета доступный на этой стадии.")]
            public Tier MaxTierAllowed;
        }

        /// <summary>Возвращает правило для данного количества предыдущих открытий.</summary>
        public DepletionStageRule GetStage(int openCount)
        {
            DepletionStageRule last = _stages.Count > 0 ? _stages[0] : default;

            foreach (var rule in _stages)
            {
                if (openCount >= rule.OpenIndex)
                    last = rule;
                else
                    break;
            }

            return last;
        }
    }
}