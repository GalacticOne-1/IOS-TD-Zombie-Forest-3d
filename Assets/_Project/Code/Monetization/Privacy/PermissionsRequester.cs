using System;
using System.Collections;
using Galactic1.Mobile;
using Unity.Advertisement.IosSupport;
using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using Unity.Advertisement.IosSupport;
#endif

namespace Galactic1.Core
{
    /// <summary>
    /// Значения совпадают с ATTrackingStatusBinding.AuthorizationTrackingStatus,
    /// поэтому безопасно кастуются в byte для GameConfig.SetStatusATT.
    /// </summary>
    public enum AttStatus : byte
    {
        NotDetermined = 0,
        Restricted = 1,
        Denied = 2,
        Authorized = 3
    }

    /// <summary>
    /// Последовательный запрос системных разрешений при старте приложения.
    /// Запускать: yield return coroutines.StartCoroutine(PermissionsRequester.RequestAll(...));
    /// </summary>
    public static class PermissionsRequester
    {
        private const float PollInterval = 0.1f;

        public static IEnumerator RequestAll(Action<AttStatus> onAttResolved = null)
        {
            // небольшая пауза: на iOS диалог ATT может не показаться,
            // если приложение ещё не стало полностью активным
            yield return null;

            // #1 ATT (iOS 14+)
            yield return RequestATT(onAttResolved);

            // #2 сюда добавлять другие разрешения (push, камера и т.д.)

            ScreenProfiler.AddMessage(">>> Request Permission Complete!");
            DLog.Alert(">>> Request Permission Complete!");
        }

        private static IEnumerator RequestATT(Action<AttStatus> onResolved)
        {
#if UNITY_IOS && !UNITY_EDITOR
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            ScreenProfiler.AddMessage($"ATT >>> {status}");

            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                ScreenProfiler.AddMessage("ATT >>> waiting for user response...");

                var wait = new WaitForSecondsRealtime(PollInterval);
                while ((status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus())
                       == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                {
                    yield return wait;
                }

                ScreenProfiler.AddMessage($"ATT >>> response: {status}");
            }

            onResolved?.Invoke((AttStatus)(byte)status);
#else
            // не iOS / редактор: ATT недоступен, статус не меняем
            yield break;
#endif
        }
    }
}