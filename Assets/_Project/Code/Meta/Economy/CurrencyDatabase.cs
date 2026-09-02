using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Economy
{
    [CreateAssetMenu(
        fileName = "CurrencyDatabase",
        menuName = "Game Configs/Economy/Currency Database")]
    public sealed class CurrencyDatabase : ScriptableObject
    {
        [SerializeField]
        private List<CurrencyConfig> currencies = new();

        public IReadOnlyList<CurrencyConfig> Currencies => currencies;
    }
}