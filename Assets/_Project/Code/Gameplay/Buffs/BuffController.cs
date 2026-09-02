using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    public class BuffController
    {
        public readonly List<BuffInstance> ActiveBuffs = new();
        private readonly StatsRuntimeBase stats;

        public BuffController(StatsRuntimeBase stats)
        {
            this.stats = stats;
        }

        public void AddBuff(Buff buff)
        {
            // Заменяем старый, если есть
            for (int i = 0; i < ActiveBuffs.Count; i++)
            {
                if (ActiveBuffs[i].source.Id == buff.Id)
                {
                    ActiveBuffs[i] = new BuffInstance(buff);
                    stats.Recalculate();
                    return;
                }
            }

            ActiveBuffs.Add(new BuffInstance(buff));
            stats.Recalculate();
        }
        
        public void RemoveBuff(BuffId buffId)
        {
            for (int i = 0; i < ActiveBuffs.Count; i++)
            {
                if (ActiveBuffs[i].source.Id == buffId)
                {
                    ActiveBuffs.RemoveAt(i);
                    stats.Recalculate();
                    return;
                }
            }
        }

        public bool HasBuff(BuffId buffId)
        {
            for (int i = 0; i < ActiveBuffs.Count; i++)
                if (ActiveBuffs[i].source.Id == buffId)
                    return true;
            return false;
        }

        public void Update(float delta)
        {
            for (int i = ActiveBuffs.Count - 1; i >= 0; i--)
            {
                ActiveBuffs[i].timeRemaining -= delta;

                if (ActiveBuffs[i].timeRemaining <= 0)
                {
                    ActiveBuffs.RemoveAt(i);
                    stats.Recalculate();
                }
            }
        }
    }
}