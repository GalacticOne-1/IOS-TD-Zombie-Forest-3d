using System;
using UnityEngine;
using System.Collections;
using Galactic1.Core;

namespace Galactic1
{
    public class ServerTimeSync : IServerTimeSync
    {
        private readonly IServerAPI serverAPI;
        private readonly Coroutines _routine;
        
        
        public double serverOffset;
        public float syncInterval = 300f; // 5 минут
        const int RESET_HOUR_UTC = 5;

        public event Action OnServerTimeSynced;


        public ServerTimeSync(DIContainer container, Coroutines routine)
        {
            serverAPI = container.Resolve<IServerAPI>();
            _routine = routine;
            
            _routine.StartCoroutine(SyncRoutine());
        }


        private IEnumerator SyncRoutine()
        {
            while (true)
            {
                yield return _routine.StartCoroutine(SyncServerTime());
                yield return new WaitForSeconds(syncInterval);
            }
        }

        public IEnumerator SyncServerTime()
        {
            double requestStart = Time.realtimeSinceStartupAsDouble;

            yield return serverAPI.GetServerTime((serverUnixTime) =>
            {
                if (serverUnixTime <= 0) return;

                double requestEnd = Time.realtimeSinceStartupAsDouble;
                double rtt = requestEnd - requestStart;

                double corrected = serverUnixTime + rtt * 0.5;
                serverOffset = corrected - requestEnd;

                DLog.Alert($"[TimeSync] Offset={serverOffset}", EDlogColor.BLUE, AppConstants.show_log_core);

                OnServerTimeSynced?.Invoke();
            });
        }

        public double GetServerNow()
            => Time.realtimeSinceStartupAsDouble + serverOffset;
        
        public int GetServerDay()
        {
            double adjusted = GetServerNow() - RESET_HOUR_UTC * 3600;
            return (int)(adjusted / 86400d);
        }
    }
}