using System;
using UnityEngine;
using System.Collections;
using Galactic1.Core;

namespace Galactic1
{
    public class ServerConnectionChecker : IServerConnectionChecker
    {
        private readonly IServerAPI serverAPI;
        private readonly Coroutines _routine;

        public bool isConnected = false;
        public event Action<bool> OnConnectionChanged;

        private float checkInterval = 10f;
        private bool lastStatus = false;


        public ServerConnectionChecker(IServerAPI serverAPI, Coroutines routine)
        {
            this.serverAPI = serverAPI;
            _routine = routine;
        }


        public IEnumerator CheckRoutine()
        {
            while (true)
            {
                yield return _routine.StartCoroutine(serverAPI.PingServer((ok) =>
                {
                    isConnected = ok;
                    if (isConnected != lastStatus)
                    {
                        lastStatus = isConnected;
                        OnConnectionChanged?.Invoke(isConnected);
                    }

                    if (ok) DLog.Alert("✅ Сервер доступен");
                    else DLog.Alert("❌ Нет соединения с сервером!", EDlogColor.RED);
                }));

                yield return new WaitForSeconds(checkInterval);
            }
        }
    }
}