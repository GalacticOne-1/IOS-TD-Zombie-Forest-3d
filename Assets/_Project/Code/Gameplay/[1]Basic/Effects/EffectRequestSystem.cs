using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.PoolObject
{
    public class EffectRequestSystem : MonoBehaviour, IGameService, IUpdate
    {
        //[SerializeField] private VfxPlayer vfxPlayer;
        //[SerializeField] private SfxPlayer sfxPlayer;
        //[SerializeField] private CameraShakeSystem shakeSystem;

        [Header("Queue BasicSettings")] [SerializeField]
        private int maxEffectsPerFrame = 10;

        [SerializeField] private float effectTimeout = 3f;

        private readonly List<QueuedEffect> queue = new();

        
        
        private Dictionary<RuntimeId, EffectConfig> _lookup;
        private VfxPlayer vfxPlayer;
        

        public void Initialize(List<EffectConfig> effectConfigs)
        {
            _lookup ??= effectConfigs.ToDictionary(c => c.Id, c => c);
            
            vfxPlayer = new VfxPlayer();
            
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Add(this);
        }

        public IObjectPoolConfig Get(VfxId id)
            => _lookup.TryGetValue(id, out var entry) ? entry : null;





        public void IUpdateClear()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().update.Remove(this);
        }

        public void UpdateM()
        {
            int count = 0;

            // Удаляем просроченные
            queue.RemoveAll(q => Time.time - q.EnqueueTime > effectTimeout);

            // Высокий приоритет сначала
            queue.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            while (queue.Count > 0 && count < maxEffectsPerFrame)
            {
                var item = queue[0];
                queue.RemoveAt(0);

                ProcessRequest(item);
                count++;
            }
        }

        public void Request(
            EffectRequest request, 
            EffectPriority priority = EffectPriority.Normal,
            Action<GameObject> onComplete = null)
        {
            queue.Add(new QueuedEffect(request, priority, onComplete));
        }

        private void ProcessRequest(QueuedEffect data)
        {
            var config = Get(data.Request.Id);
            if (config == null)
            {
                DLog.Alert($"[EffectRequestSystem] Unknown effect key: {data.Request.Id}", EDlogColor.RED);
                return;
            }
            
            /*
             *      Здесь идет общий вызов
             *       например:
             *              вместе с эффектом взрыва, должен идти звук или камера затрястись ...
             *      т.е настройка в одном месте как AbstractFactory
             */

            //if (config.playVfx)
            var effect = vfxPlayer.Play(config, data.Request);
            if (effect == null)
            {
                DLog.Alert("Effect is null!", EDlogColor.RED);
                return;
            }
            
            // if (config.playSfx && config.sfxClip != null)
            //     sfxPlayer.Play(config.sfxClip, request.position);
            //
            // if (config.shakeCamera)
            //     shakeSystem.Shake();

            data.OnComplete?.Invoke(effect);
            //Debug.Log($"[EffectRequestSystem] Played: {data.Request.key} at {data.Request.position}");
        }

        
    }


}