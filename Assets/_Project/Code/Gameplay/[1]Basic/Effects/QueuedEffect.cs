using System;
using UnityEngine;

namespace Galactic1.PoolObject
{
    public enum EffectPriority { Low, Normal, High }

    public class QueuedEffect
    {
        public EffectRequest Request;
        public EffectPriority Priority;
        public Action<GameObject> OnComplete;
        public float EnqueueTime;

        public QueuedEffect(EffectRequest request, EffectPriority priority, Action<GameObject> onComplete)
        {
            Request = request;
            Priority = priority;
            OnComplete = onComplete;
            EnqueueTime = Time.time;
        }
    }

}