
using Galactic1.Game.UI.Stats.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public abstract class StatViewBase : MonoBehaviour , IPooledStatView<StatDtoBase>
    {
        private RectTransform rt;
        public RectTransform RectTransform => rt ?? GetComponent<RectTransform>();

        private StatDtoBase dtoBase;
        public StatDtoBase Dto => dtoBase;
        
        

        private void Awake()
        {
            rt = GetComponent<RectTransform>();
        }

        public virtual void Bind(StatDtoBase data)
        {
            dtoBase = data;
        }

        public abstract void ResetView();
    }
}