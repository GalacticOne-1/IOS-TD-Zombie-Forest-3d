using System;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  5. SuppressionReceiver — регистрирует факт
    //     подавления огнём. Вызывается из логики
    //     HeavyWeapon когда враг стреляет рядом.
    // ─────────────────────────────────────────────

    public sealed class SuppressionReceiver : MonoBehaviour
    {
        [SerializeField] private float suppressionDuration = 3f;
        [SerializeField] private float suppressionRadius = 8f;

        public bool IsSuppressed { get; private set; }
        public float SuppressionTimer { get; private set; }

        public event Action OnSuppressed;
        public event Action OnSuppressionLifted;

        private void Update()
        {
            if (!IsSuppressed) return;

            SuppressionTimer -= Time.deltaTime;
            if (SuppressionTimer <= 0f)
            {
                IsSuppressed = false;
                OnSuppressionLifted?.Invoke();
            }
        }

        /// <summary>
        /// Вызывается когда рядом с юнитом стреляют (из HeavyWeapon или AIDirector).
        /// shotPosition — откуда летит огонь подавления.
        /// </summary>
        public void ReceiveSuppression(Vector3 shotPosition)
        {
            float dist = Vector3.Distance(transform.position, shotPosition);
            if (dist > suppressionRadius) return;

            SuppressionTimer = suppressionDuration;

            if (!IsSuppressed)
            {
                IsSuppressed = true;
                OnSuppressed?.Invoke();
            }
        }

        /// <summary>Форсированно снять подавление (FallBack команда).</summary>
        public void ClearSuppression()
        {
            IsSuppressed = false;
            SuppressionTimer = 0f;
        }
    }
}