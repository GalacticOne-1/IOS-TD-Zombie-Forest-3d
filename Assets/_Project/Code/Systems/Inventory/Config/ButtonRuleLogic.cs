using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Configs.UI
{
    public abstract class ButtonRuleLogic : ScriptableObject
    {
        public abstract bool Evaluate(InventoryManagementWindow window);
    }
}