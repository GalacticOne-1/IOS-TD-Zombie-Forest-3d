
using Galactic1.Game.UI.Stats.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public interface IPooledStatView<T>
    {
        RectTransform RectTransform { get; }
        StatDtoBase Dto { get; }
        void Bind(T data);
        void ResetView();
    }
}