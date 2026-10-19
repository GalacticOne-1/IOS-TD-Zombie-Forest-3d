using System;
using System.Collections;
using UnityEngine;
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
        const int INITIAL_ATTEMPTS = 3;

        private bool _autoSyncStarted;

        /// <summary>
        /// true после первой успешной синхронизации. До этого GetServerNow()/GetServerDay() возвращают мусор.
        /// </summary>
        public bool IsSynced { get; private set; }

        public event Action OnServerTimeSynced;


        public ServerTimeSync(DIContainer container, Coroutines routine)
        {
            serverAPI = container.Resolve<IServerAPI>();
            _routine = routine;
            // Цикл синхронизации здесь НЕ запускаем: см. StartAutoSync().
        }

        /// <summary>
        /// Запускает периодический синк. Вызывать один раз, когда стейт загружен
        /// и все правила суточного сброса зарегистрированы.
        /// </summary>
        public void StartAutoSync()
        {
            if (_autoSyncStarted) return;
            _autoSyncStarted = true;
            _routine.StartCoroutine(SyncLoop());
        }

        private IEnumerator SyncLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(syncInterval);
                yield return SyncServerTime();
            }
        }

        /// <summary>
        /// Пока не было ни одной успешной синхронизации — до INITIAL_ATTEMPTS попыток с паузами.
        /// После — одна попытка (периодический синк, при неудаче просто ждём следующего цикла).
        /// </summary>
        public IEnumerator SyncServerTime()
        {
            int attempts = IsSynced ? 1 : INITIAL_ATTEMPTS;

            for (int i = 0; i < attempts; i++)
            {
                double requestStart = Time.realtimeSinceStartupAsDouble;
                double serverUnixTime = -1;

                yield return serverAPI.GetServerTime(t => serverUnixTime = t);

                if (serverUnixTime > 0)
                {
                    double requestEnd = Time.realtimeSinceStartupAsDouble;
                    double rtt = requestEnd - requestStart;

                    double corrected = serverUnixTime + rtt * 0.5;
                    serverOffset = corrected - requestEnd;
                    IsSynced = true;

                    DLog.Alert($"[TimeSync] Offset={serverOffset}", EDlogColor.BLUE, AppConstants.show_log_core);

                    try
                    {
                        OnServerTimeSynced?.Invoke();
                    }
                    catch (Exception e)
                    {
                        // ошибка подписчика не должна ломать цикл синхронизации
                        Debug.LogException(e);
                    }

                    yield break;
                }

                if (i < attempts - 1)
                    yield return new WaitForSecondsRealtime(1f + i); // 1, 2 сек
            }

            DLog.Alert("[TimeSync] Sync failed", EDlogColor.ORANGE, AppConstants.show_log_core);
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
