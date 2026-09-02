
using System.Collections;
using System.Collections.Generic;
using Galactic1.Code.Notification;
using Galactic1.Core.Notifications;
using UnityEngine;

namespace Galactic1.UI.Notifications
{
    /// <summary>
    /// Live-service уровень ToastManager.
    /// Поддерживает очередь, стек, дедупликацию и throttle.
    /// </summary>
    public sealed class ToastManager : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Transform toastRoot;
        [SerializeField] private ToastView toastPrefab;

        [Header("Behaviour")]
        [SerializeField] private int maxVisible = 3;
        [SerializeField] private float defaultDuration = 2f;
        [SerializeField] private float spacing = 80f;
        [SerializeField] private bool collapseDuplicates = true;

        private readonly Queue<NotificationRequest> _queue = new();
        private readonly List<ToastRuntime> _active = new();
        private readonly HashSet<string> _activeIds = new();

        private INotificationService _notificationService;



        public void Initialize(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Show(NotificationRequest request)
        {
            if (collapseDuplicates && _activeIds.Contains(request.Id))
                return;

            _queue.Enqueue(request);
            TryProcess();
        }

        private void TryProcess()
        {
            while (_queue.Count > 0 && _active.Count < maxVisible)
            {
                var request = _queue.Dequeue();
                StartCoroutine(ShowRoutine(request));
            }
        }

        private IEnumerator ShowRoutine(NotificationRequest request)
        {
            _activeIds.Add(request.Id);

            var instance = Instantiate(toastPrefab, toastRoot);
            instance.transform.SetAsLastSibling();
            instance.GetComponent<ToastView>().MessageText.color = request.Style.TextColor;

            var duration = request.Duration > 0f 
                ? request.Duration 
                : defaultDuration;

            var runtime = new ToastRuntime
            {
                Request = request,
                View = instance
            };

            _active.Add(runtime);

            //RefreshLayout();

            yield return instance.PlayIn(request.Message);

            yield return new WaitForSeconds(duration);

            yield return instance.PlayOut();

            Remove(runtime);
        }

        private void Remove(ToastRuntime runtime)
        {
            _active.Remove(runtime);
            _activeIds.Remove(runtime.Request.Id);

            Destroy(runtime.View.gameObject);

            if (_notificationService is NotificationService concrete)
                concrete.Complete(runtime.Request);

            //RefreshLayout();
            TryProcess();
        }

        private void RefreshLayout()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                var rt = _active[i].View.RectTransform;
                rt.anchoredPosition = new Vector2(
                    rt.anchoredPosition.x,
                    -i * spacing
                );
            }
        }

        private struct ToastRuntime
        {
            public NotificationRequest Request;
            public ToastView View;
        }
    }
}