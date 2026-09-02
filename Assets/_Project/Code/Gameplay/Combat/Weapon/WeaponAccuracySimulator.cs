using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    /// <summary>
    /// Симулятор точности оружия — чистая runtime-логика без Editor-зависимостей.
    ///
    /// Использует ту же формулу spread что и SpreadComponent.ComputeRangePenaltyStatic
    /// — без дублирования логики.
    ///
    /// Геометрическая баллистика:
    ///   Для каждой дробины/пули строится луч с угловым разбросом.
    ///   Попадание = пересечение луча с вертикальным цилиндром цели.
    ///   Математически эквивалентно Physics.RaycastAll с capsule-коллайдером,
    ///   но работает без сцены и MonoBehaviour.
    ///
    /// Дробовик:
    ///   ProjectilesPerShot > 1 — каждый выстрел = N независимых дробин.
    ///   Статистика per-shot: сколько дробин попало за один выстрел.
    ///   Это позволяет видеть распределение кучности, а не просто усреднённый hit rate.
    /// </summary>
    public static class WeaponAccuracySimulator
    {
        public const float DefaultTargetRadius = 0.4f;

        // ── Config ────────────────────────────────────────────────────────

        public readonly struct SimulationConfig
        {
            public readonly WeaponDefinitionData Weapon;
            public readonly float Distance;
            public readonly int ShotCount;
            public readonly float TargetRadius;

            public SimulationConfig(
                WeaponDefinitionData weapon,
                float distance,
                int shotCount,
                float targetRadius = DefaultTargetRadius)
            {
                Weapon = weapon;
                Distance = distance;
                ShotCount = shotCount;
                TargetRadius = targetRadius;
            }
        }

        // ── Results ───────────────────────────────────────────────────────

        /// <summary>
        /// Результат одиночного оружия (ProjectilesPerShot == 1).
        /// HitRate = доля выстрелов, попавших в цель.
        /// </summary>
        public readonly struct SimulationResult
        {
            public readonly float Distance;
            public readonly int ShotCount;
            public readonly int Hits;
            public readonly int Misses;
            public readonly float HitRate;

            public SimulationResult(float distance, int shotCount, int hits)
            {
                Distance = distance;
                ShotCount = shotCount;
                Hits = hits;
                Misses = shotCount - hits;
                HitRate = shotCount > 0 ? (float)hits / shotCount : 0f;
            }
        }

        /// <summary>
        /// Результат дробовика (ProjectilesPerShot > 1).
        ///
        /// Статистика per-shot:
        ///   AvgPelletsHit      — среднее число попавших дробин за выстрел
        ///   PelletDistribution — [k] = количество выстрелов с ровно k попаданиями
        /// </summary>
        public sealed class ShotgunSimulationResult
        {
            public readonly float Distance;
            public readonly int ShotCount;
            public readonly int PelletsPerShot;
            public readonly float AvgPelletsHit;
            public readonly float AvgPelletsMiss;
            public readonly float PelletHitRate; // AvgPelletsHit / PelletsPerShot

            /// <summary>distribution[k] = количество выстрелов с ровно k попаданиями.</summary>
            public readonly int[] PelletDistribution;

            public ShotgunSimulationResult(
                float distance,
                int shotCount,
                int pelletsPerShot,
                int totalPelletsHit,
                int[] distribution)
            {
                Distance = distance;
                ShotCount = shotCount;
                PelletsPerShot = pelletsPerShot;
                PelletDistribution = distribution;
                AvgPelletsHit = shotCount > 0 ? (float)totalPelletsHit / shotCount : 0f;
                AvgPelletsMiss = pelletsPerShot - AvgPelletsHit;
                PelletHitRate = pelletsPerShot > 0 ? AvgPelletsHit / pelletsPerShot : 0f;
            }

            /// <summary>Доля выстрелов (0..1) с ровно k попаданиями.</summary>
            public float DistributionFraction(int k)
            {
                if (k < 0 || k >= PelletDistribution.Length || ShotCount == 0) return 0f;
                return (float)PelletDistribution[k] / ShotCount;
            }
        }

        // ── Run ───────────────────────────────────────────────────────────

        /// <summary>
        /// Одиночное оружие. HitRate = процент выстрелов, попавших в цель.
        /// </summary>
        public static SimulationResult Run(SimulationConfig cfg)
        {
            float spreadDeg = GetSpreadDeg(cfg);
            var targetPos = new Vector3(0f, 0f, cfg.Distance);
            int hits = 0;

            for (int shot = 0; shot < cfg.ShotCount; shot++)
            {
                if (SimulatePellet(spreadDeg, targetPos, cfg.TargetRadius, cfg.Distance))
                    hits++;
            }

            return new SimulationResult(cfg.Distance, cfg.ShotCount, hits);
        }

        /// <summary>
        /// Дробовик. Для каждого выстрела считаем сколько дробин попало.
        /// Возвращает полную статистику: среднее и распределение по выстрелам.
        /// </summary>
        public static ShotgunSimulationResult RunShotgun(SimulationConfig cfg)
        {
            int pelletsPerShot = Mathf.Max(1, cfg.Weapon.ProjectilesPerShot);
            float spreadDeg = GetSpreadDeg(cfg);
            var targetPos = new Vector3(0f, 0f, cfg.Distance);

            var distribution = new int[pelletsPerShot + 1];
            int totalPelletsHit = 0;

            for (int shot = 0; shot < cfg.ShotCount; shot++)
            {
                int pelletsHit = 0;
                for (int p = 0; p < pelletsPerShot; p++)
                {
                    if (SimulatePellet(spreadDeg, targetPos, cfg.TargetRadius, cfg.Distance))
                        pelletsHit++;
                }

                distribution[pelletsHit]++;
                totalPelletsHit += pelletsHit;
            }

            return new ShotgunSimulationResult(
                cfg.Distance, cfg.ShotCount, pelletsPerShot, totalPelletsHit, distribution);
        }

        // ── Series ────────────────────────────────────────────────────────

        public static List<SimulationResult> RunSeries(
            WeaponDefinitionData weapon,
            IReadOnlyList<float> distances,
            int shotCount,
            float targetRadius = DefaultTargetRadius)
        {
            var results = new List<SimulationResult>(distances.Count);
            foreach (float d in distances)
                results.Add(Run(new SimulationConfig(weapon, d, shotCount, targetRadius)));
            return results;
        }

        public static List<ShotgunSimulationResult> RunShotgunSeries(
            WeaponDefinitionData weapon,
            IReadOnlyList<float> distances,
            int shotCount,
            float targetRadius = DefaultTargetRadius)
        {
            var results = new List<ShotgunSimulationResult>(distances.Count);
            foreach (float d in distances)
                results.Add(RunShotgun(new SimulationConfig(weapon, d, shotCount, targetRadius)));
            return results;
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static float GetSpreadDeg(SimulationConfig cfg)
        {
            var def = cfg.Weapon;
            return SpreadComponent.ComputeRangePenaltyStatic(
                cfg.Distance,
                def.EffectiveRange,
                def.MaxRange,
                def.MaxRangeSpreadPenalty) * def.BaseSpreadDeg;
        }

        private static bool SimulatePellet(
            float spreadDeg,
            Vector3 targetPos,
            float targetRadius,
            float maxDist)
        {
            float angleX = Random.Range(-spreadDeg, spreadDeg);
            float angleY = Random.Range(-spreadDeg, spreadDeg);
            Vector3 dir = Quaternion.Euler(angleX, angleY, 0f) * Vector3.forward;
            return RayHitsCylinder(Vector3.zero, dir, targetPos, targetRadius, maxDist);
        }

        // ── Geometry ──────────────────────────────────────────────────────

        private static bool RayHitsCylinder(
            Vector3 rayOrigin,
            Vector3 rayDir,
            Vector3 targetPos,
            float radius,
            float maxDist)
        {
            var o = new Vector2(rayOrigin.x, rayOrigin.z);
            var d = new Vector2(rayDir.x, rayDir.z);
            var c = new Vector2(targetPos.x, targetPos.z);
            var oc = o - c;

            float a = Vector2.Dot(d, d);
            float b = 2f * Vector2.Dot(oc, d);
            float disc = b * b - 4f * a * (Vector2.Dot(oc, oc) - radius * radius);

            if (disc < 0f) return false;
            float t = (-b - Mathf.Sqrt(disc)) / (2f * a);
            return t >= 0f && t <= maxDist;
        }
    }
}