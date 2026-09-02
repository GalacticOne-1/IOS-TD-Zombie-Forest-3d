using UnityEngine;

namespace Galactic1.Code.Gameplay.Construction.Repair
{
    /// <summary>
    /// Округление вверх — гарантирует, что списанных ресурсов хватит
    /// на заявленный процент восстановления HP.
    /// </summary>
    public class CeilRepairRoundingStrategy : IRepairRoundingStrategy
    {
        public int Round(float amount) => Mathf.CeilToInt(amount);
    }
}