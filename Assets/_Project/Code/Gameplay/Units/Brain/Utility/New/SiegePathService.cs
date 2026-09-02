using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Code.Gameplay.Units.Movement;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// READ-ONLY наблюдатель результатов path calculation к HQ.
    ///
    /// ЖЁСТКИЙ ИНВАРИАНТ: этот класс НИКОГДА не вызывает UnitMover.MoveTo()
    /// и не инициирует движение никаким иным способом. Единственный владелец
    /// движения к HQ — SiegeAdvanceAction (через ChaseCommand).
    ///
    /// Обязанности:
    ///   1. подписаться на UnitMover.OnPathComputed (per-request данные);
    ///   2. отфильтровать результаты, не относящиеся к текущему HQ-маршруту
    ///      (по CurrentObjective и совпадению destination);
    ///   3. Complete  → PathBlocked=false, CurrentWall=null;
    ///   4. Partial + валидный endpoint + найдена стена → PathBlocked=true, CurrentWall=wall;
    ///   5. Partial + невалидный endpoint ИЛИ стена не найдена → состояние НЕ трогать;
    ///   6. Failed → состояние НЕ трогать (Failed не доказывает наличие стены);
    ///   7. подписка/отписка на IRaidFacilityRuntime.OnDestroyed текущей стены;
    ///   8. lifecycle: EnsureSubscribed идемпотентен, Unsubscribe снимает обе подписки.
    ///
    /// ИНВАРИАНТ PathBlocked==true ⇒ CurrentWall!=null соблюдён КОНСТРУКТИВНО:
    /// в этом файле есть ровно одна строка, пишущая PathBlocked=true, и она
    /// находится внутри if(wall != null), в той же ветке, что и SetCurrentWall(wall).
    /// </summary>
    public sealed class SiegePathService
    {
        private const float WallSearchRadius = 2.5f;
        private const float DestinationMatchThreshold = 0.5f;
        private const float CompleteMismatchThreshold = 1.0f;

        private GameLoopContext _gameLoopContext;
        private readonly BaseFacilityRepository _facilityRepository;

        
        public SiegePathService(
            GameLoopContext gameLoopContext,
            BaseFacilityRepository facilityRepository)
        {
            _gameLoopContext = gameLoopContext;
            _facilityRepository = facilityRepository;
        }

        /// <summary>Идемпотентна — безопасно вызывать каждый think-тик.</summary>
        public void EnsureSubscribed(UnitInstance unit, SiegeBlackboard blackboard)
        {
            if (blackboard.PathComputedUnsubscribe != null) return;

            void Handler(NavigationPathStatus status, Vector3 destination, uint requestId, Vector3 endpoint)
                => HandlePathComputed(blackboard, status, destination, endpoint);

            unit.Mover.OnPathComputed += Handler;
            blackboard.PathComputedUnsubscribe = () => unit.Mover.OnPathComputed -= Handler;
        }

        /// <summary>Вызывается из SiegeUtilityBrain.Dispose(). Снимает обе подписки.
        /// Безопасна для повторного вызова.</summary>
        public void Unsubscribe(SiegeBlackboard blackboard)
        {
            blackboard.PathComputedUnsubscribe?.Invoke();
            blackboard.PathComputedUnsubscribe = null;
            blackboard.WallDestroyedUnsubscribe?.Invoke();
            blackboard.WallDestroyedUnsubscribe = null;
        }

        private void HandlePathComputed(
            SiegeBlackboard blackboard, NavigationPathStatus status, Vector3 destination, Vector3 endpoint)
        {
            // Логическая фильтрация устаревших/нерелевантных результатов:
            // технически устаревшие (superseded) callbacks сюда никогда не попадут —
            // их отсекает requestId-guard внутри UnitMover ДО вызова события.
            if (blackboard.CurrentObjective != SiegeObjective.Headquarters) return;
            if (blackboard.Headquarters == null) return;
            
            Vector3 expectedDestination = blackboard.CurrentAttackPoint != null
                ? blackboard.CurrentAttackPoint.position
                : blackboard.Headquarters.Position; // fallback — HQ без attack points

            float sqDist = (destination - expectedDestination).sqrMagnitude;
            if (sqDist > DestinationMatchThreshold * DestinationMatchThreshold) return;

            switch (status)
            {
                case NavigationPathStatus.Complete:
                    
                    float sqEndpointGap = (endpoint - expectedDestination).sqrMagnitude;
                    if (!float.IsPositiveInfinity(endpoint.x) 
                        && sqEndpointGap > CompleteMismatchThreshold * CompleteMismatchThreshold)
                    {
                        var wall = FindNearestWall(endpoint);
                        if (wall != null)
                        {
                            blackboard.LastKnownBlockedPosition = endpoint;
                            blackboard.PathBlocked = true;
                            SetCurrentWall(blackboard, wall);
                            return;
                        }
                        // endpoint далеко от destination, но рядом нет зарегистрированной
                        // стены — не блокируем без подтверждённой цели (тот же принцип,
                        // что и в ветке Partial).
                    }

                    blackboard.PathBlocked = false;
                    SetCurrentWall(blackboard, null);
                    break;

                case NavigationPathStatus.Partial:
                {
                    if (float.IsPositiveInfinity(endpoint.x))
                        return; // нет валидного endpoint для ЭТОГО результата — недостаточно данных

                    var wall = FindNearestWall(endpoint);
                    if (wall != null)
                    {
                        blackboard.LastKnownBlockedPosition = endpoint;
                        blackboard.PathBlocked = true;
                        SetCurrentWall(blackboard, wall); // PathBlocked=true всегда идёт вместе с non-null wall
                    }
                    // wall == null → рядом с endpoint нет зарегистрированной стены.
                    // Намеренно НЕ устанавливаем PathBlocked=true без подтверждённой цели.
                }
                    break;

                case NavigationPathStatus.Failed:
                    // Failed НЕ доказывает наличие стены (может быть: юнит вне графа,
                    // временный graph issue, любая другая navigation failure).
                    // Ничего не меняем — ни PathBlocked, ни CurrentWall.
                    break;
            }
        }

        private void SetCurrentWall(SiegeBlackboard blackboard, ITargetInfo wall)
        {
            if (ReferenceEquals(blackboard.CurrentWall, wall)) return;

            blackboard.WallDestroyedUnsubscribe?.Invoke();
            blackboard.WallDestroyedUnsubscribe = null;
            blackboard.CurrentWall = wall;

            // ДОПУЩЕНИЕ (не подтверждено полным исходником scene-adapter'а стены):
            // предполагается, что во время рейда wall.Unit.RuntimeBase кастится
            // к IRaidFacilityRuntime. Если cast не пройдёт — просто не будет
            // event-based авто-repath после разрушения стены; AI останется
            // корректным благодаря независимому IsDead-guard в SiegeAIContextBuilder.
            if (wall?.Unit?.RuntimeBase is not IRaidFacilityRuntime facility) return;

            void OnWallDestroyed() => HandleWallDestroyed(blackboard);
            facility.OnDestroyed += OnWallDestroyed;
            blackboard.WallDestroyedUnsubscribe = () => facility.OnDestroyed -= OnWallDestroyed;
        }

        /// <summary>
        /// Стена уничтожена. Сбрасывает Siege path-состояние, но НЕ инициирует
        /// новое движение — на следующем think-тике SiegeObjectiveResolver
        /// увидит PathBlocked==false и вернёт objective=Headquarters,
        /// а SiegeAdvanceAction сам выдаст ChaseCommand(HQ).
        /// </summary>
        private void HandleWallDestroyed(SiegeBlackboard blackboard)
        {
            blackboard.WallDestroyedUnsubscribe?.Invoke();
            blackboard.WallDestroyedUnsubscribe = null;
            blackboard.CurrentWall = null;
            blackboard.PathBlocked = false;
        }

        /// <summary>ДОПУЩЕНИЕ: сцен-Runtime стены реализует ICombatFacilityRuntime
        /// (Type == FacilityType.Defense). Радиус маленький — endpoint это
        /// последний corner PARTIAL-пути, т.е. точка прямо у препятствия.</summary>
        private ITargetInfo FindNearestWall(Vector3 fromPosition)
        {
            FacilityInstance nearest = null;
            float bestSqDist = WallSearchRadius * WallSearchRadius;

            var def = _gameLoopContext.DefenseFacilities;

            if (def == null)
                return null;

            foreach (var f in def)
            {
                if (f == null)
                    continue;

                if (f.Type != FacilityType.Defense)
                    continue;

                Debug.Log(
                    $"[SiegePath] FindNearestWall: " +
                    $"id={f.Id}, type={f.Type}"
                );
                var result = _facilityRepository.TryGet(f.Id);
                Debug.Log(
                    $"[SiegePath] TryGet: " +
                    $"id={f.Id}, done={result.done}, instance={result.instance}"
                );

                if (!result.done || result.instance == null)
                    continue;

                var instance = result.instance;

                if (instance == null)
                    continue;

                float d =
                    (instance.transform.position - fromPosition).sqrMagnitude;

                if (d < bestSqDist)
                {
                    bestSqDist = d;
                    nearest = instance;
                }
            }

            return nearest != null
                ? nearest.GetComponent<TargetInfoBase>()
                : null;
        }
    }
}
