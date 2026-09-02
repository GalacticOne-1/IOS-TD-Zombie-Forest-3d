using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Items
{
    public abstract class ItemActionConfig : ScriptableObject
    {
        [Tooltip("Название действия, чтобы дизайнеру было понятно")] 
        [SerializeField]
        private string ActionName;

        /// <summary>
        /// Логика этого действия
        /// </summary>
        public abstract void Execute(ItemContext ctx);
    }
}