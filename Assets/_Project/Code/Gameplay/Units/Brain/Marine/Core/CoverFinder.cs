using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    // ─────────────────────────────────────────────
    //  4. CoverFinder — ищет ближайший свободный
    //     CoverPoint в радиусе, предпочитая те
    //     что находятся ЗА юнитом относительно врага.
    // ─────────────────────────────────────────────

    public sealed class CoverFinder : MonoBehaviour
    {
        [SerializeField] private float searchRadius = 15f;
        [SerializeField] private LayerMask coverLayer;

        private readonly Collider[] _buffer = new Collider[16];

        /// <summary>
        /// Найти лучшее укрытие относительно угрозы threatPos.
        /// Возвращает null если ничего нет.
        /// </summary>
        public CoverPoint FindBest(Vector3 fromPos, Vector3 threatPos, SurvivorInstance requestor)
        {
            int count = Physics.OverlapSphereNonAlloc(
                fromPos, searchRadius, _buffer, coverLayer);

            CoverPoint best = null;
            float bestScore = float.MinValue;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].TryGetComponent<CoverPoint>(out var cp)) continue;
                if (cp.IsOccupied && cp.Occupant != requestor) continue;

                float score = ScoreCover(cp.Position, fromPos, threatPos);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = cp;
                }
            }

            return best;
        }

        // Чем дальше укрытие от угрозы и чем ближе к юниту — тем лучше
        private static float ScoreCover(Vector3 coverPos, Vector3 unitPos, Vector3 threatPos)
        {
            float distToUnit = Vector3.Distance(coverPos, unitPos);
            float distToThreat = Vector3.Distance(coverPos, threatPos);

            // Хотим: далеко от угрозы, близко к юниту
            return distToThreat - distToUnit * 2f;
        }
    }
}