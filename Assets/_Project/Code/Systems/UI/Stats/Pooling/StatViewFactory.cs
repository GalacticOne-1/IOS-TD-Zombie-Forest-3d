
using System.Collections.Generic;
using Galactic1.Game.UI.Stats.DTO;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public class StatViewFactory : IGameService
    {
        private readonly StatLayoutConfig _statConfig;
        
        private readonly Dictionary<StatLayoutType, Queue<IPooledStatView<StatDtoBase>>> _pool = new();


        
        public StatViewFactory(StatLayoutConfig statConfig)
        {
            _statConfig = statConfig;
        }


        public IPooledStatView<StatDtoBase> Get(StatLayoutType type, Transform parent)
        {
            if (!_pool.TryGetValue(type, out var queue))
            {
                queue = new Queue<IPooledStatView<StatDtoBase>>();
                _pool[type] = queue;
            }

            IPooledStatView<StatDtoBase> view;

            if (queue.Count > 0)
            {
                view = queue.Dequeue();
                ((MonoBehaviour)view).gameObject.SetActive(true);
            }
            else
            {
                view = Create(type, parent);
            }

            ((MonoBehaviour)view).transform.SetParent(parent, false);
            return view;
        }

        public void Release(StatLayoutType type, IPooledStatView<StatDtoBase> view)
        {
            view.ResetView();
            ((MonoBehaviour)view).gameObject.SetActive(false);
            _pool[type].Enqueue(view);
        }

        private IPooledStatView<StatDtoBase> Create(StatLayoutType type, Transform parent)
        {

            _statConfig.TryGet(type, out var prefab);
            return prefab.gameObject
                .CreateGO(parent)
                .GetComponent<IPooledStatView<StatDtoBase>>();

        }
    }
}