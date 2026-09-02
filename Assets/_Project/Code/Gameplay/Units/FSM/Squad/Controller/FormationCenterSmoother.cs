using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Единственная ответственность:
    ///   NavigationCenter → FormationCenter
    ///
    /// FormationCenter плавно догоняет NavigationCenter.
    /// Никогда не телепортируется.
    /// Не знает про путь, сегменты, агентов, слоты.
    ///
    /// Вызывается из SquadMovementSystem.TickPipeline() до FormationFollower.
    /// </summary>
    public sealed class FormationCenterSmoother
    {
        // ── Config ───────────────────────────────────────────────────────────

        /// <summary>
        /// Максимальная скорость догоняния FormationCenter, м/с.
        /// Должна быть чуть выше максимальной скорости агентов (runSpeed),
        /// чтобы FormationCenter не отставал при длительном беге.
        /// </summary>
        private const float CatchUpSpeed = 7f;

        /// <summary>
        /// Если FormationCenter отстал дальше этого расстояния —
        /// догоняет с максимальной скоростью без ускорения.
        /// Предотвращает бесконечное преследование при телепорте навигации.
        /// </summary>
        private const float MaxLag = 8f;

        private readonly SquadFormationRuntime _runtime;

        public FormationCenterSmoother(SquadFormationRuntime runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// Инициализирует FormationCenter при первом IssueMove.
        /// Вызывать один раз после bootstrap.
        /// </summary>
        public void Snap()
        {
            _runtime.FormationCenter = _runtime.NavigationCenter;
        }

        public void Tick(float deltaTime)
        {
            Vector3 nav = _runtime.NavigationCenter;
            Vector3 form = _runtime.FormationCenter;

            Vector3 delta = nav - form;
            float dist = delta.magnitude;

            if (dist < 0.001f) return;

            // Скорость догоняния: линейна до MaxLag, затем зажата сверху.
            float speed = Mathf.Min(dist / deltaTime, CatchUpSpeed);

            // Если отстали слишком сильно — двигаемся с потолком скорости,
            // но всё равно плавно, без телепорта.
            if (dist > MaxLag)
                speed = CatchUpSpeed;

            _runtime.FormationCenter = Vector3.MoveTowards(
                form, nav, speed * deltaTime);
        }
    }
}