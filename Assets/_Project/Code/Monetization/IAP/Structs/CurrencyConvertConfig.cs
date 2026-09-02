using UnityEngine;

namespace Galactic1.UI.Shop
{
    /// <summary>
    /// Описание конвертации валют.
    /// </summary>
    [System.Serializable]
    public struct CurrencyConvertConfig
    {
        public CurrencyType from;
        public int fromAmount;

        public CurrencyType to;
        public int toAmount;
    }
}