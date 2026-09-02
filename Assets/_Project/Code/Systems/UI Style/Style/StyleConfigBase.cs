
using UnityEngine;

namespace Galactic1.UI.Core
{
    public abstract class StyleConfigBase : ScriptableObject, IUIStyleConfig
    {
        [field: SerializeField] public string ConfigId { get; private set; }

        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
    }
}