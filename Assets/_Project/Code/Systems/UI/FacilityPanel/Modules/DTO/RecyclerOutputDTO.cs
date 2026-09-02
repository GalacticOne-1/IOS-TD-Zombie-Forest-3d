using UnityEngine;

namespace Galactic1.Game.UI.Production.DTO
{
    /// <summary>
    /// DTO одного выходного ресурса Recycler.
    /// </summary>
    public sealed class RecyclerOutputDTO
    {
        public Sprite Icon { get; }
        public int Amount { get; }

        public RecyclerOutputDTO(Sprite icon, int amount)
        {
            Icon = icon;
            Amount = amount;
        }
    }
}