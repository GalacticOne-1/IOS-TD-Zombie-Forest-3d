using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Items
{
    [CreateAssetMenu(
        fileName = "ItemActionDatabase",
        menuName = "Game Configs/Inventory/Item Action Database"
    )]
    public class ItemActionDatabase : ScriptableObject
    {
        public List<ItemActionConfig> actions = new ();
    }
}