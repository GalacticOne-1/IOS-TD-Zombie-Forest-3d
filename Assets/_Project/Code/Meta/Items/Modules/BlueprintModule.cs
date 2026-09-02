
using System.Collections.Generic;
using Galactic1.Code.Items;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Предмет-чертёж. При использовании разблокирует рецепт.
    /// Всегда одноразовый — consumeOnUse = true.
    /// Не имеет эффектов на статы — для этого UseModule.
    /// </summary>
    [System.Serializable]
    public class BlueprintModule : ItemModule
    {
        [Tooltip("Рецепт который разблокируется при использовании")] [SerializeField]
        private CraftRecipeConfig unlocksRecipe;

        public CraftRecipeConfig UnlocksRecipe => unlocksRecipe;


        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {

        }
    }
}