using System;
using System.Collections.Generic;
using Galactic1.Code.Core;
using Galactic1.Code.Core.State;
using Galactic1.Code.Systems.Tutorial.Analytics;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Objectives;
using Galactic1.Code.Systems.Tutorial.Presentation;
using Galactic1.Core;
using R3;
using Galactic1.Window;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Production API. Debug-операции сюда намеренно не входят.</summary>
    public interface ITutorialService : IGameService
    {
        bool IsActive { get; }
        void StartTutorial(TutorialCampaignId campaignId);
        void Restore();
        void StopTutorial();
        TutorialProgress GetProgress();
        bool IsStepActive(TutorialStepId stepId);
        bool IsStepCompleted(TutorialStepId stepId);
    }

    /// <summary>Debug/QA API. Операции здесь намеренно нарушают обычную прогрессию —
    /// регистрировать в ServiceLocator только под UNITY_EDITOR/dev-сборками.</summary>
    public interface ITutorialDebugService : IGameService
    {
        void RestartTutorial();
        void CompleteStepDebug();
        void SkipStepDebug();
        void ForceStep(TutorialStepId stepId);
        void ClearProgress();
    }

    /// <summary>
    /// Центральный оркестратор тутора. По структуре ответственности — аналог
    /// MissionObjectiveService: реагирует на завершение текущего шага и продвигает граф.
    /// НЕ содержит switch/if по stepId — поведение целиком из Definition-графа.
    ///
    /// Живёт в root DIContainer, переживает смену сцен Camp→WorldMap→Raid.
    ///
    /// campaignId — RuntimeId-ассет (TutorialCampaignId), как и stepId/chapterId/targetId —
    /// не строка. Резолв кампании из typed-ссылки (например переданной в StartTutorial) —
    /// через TutorialCampaignRegistry.GetCampaign(TutorialCampaignId). Резолв из персистентного
    /// guid (CGameStateTutorial.campaignId, границa save/restore, см. Restore()) — через
    /// GetCampaignByGuid(string), по тому же принципу, что и TutorialDefinition.GetStepByGuid.
    /// В ITutorialAnalytics (внешний репортинг-контракт, остался string-based) campaignId/
    /// stepId/chapterId передаются как .Guid — стабильный id для аналитики.
    ///
    /// Единственные две точки Persist() во всём классе:
    ///   1) ActivateStep — сразу после того, как currentStepId в снапшоте стал
    ///      действительно указывать на реально активный шаг (presentation уже показана).
    ///   2) CompleteCampaign — после того, как completed=true уже записан.
    /// Никакого отложенного/флагового персиста (_pendingCheckpointPersist убран).
    /// </summary>
    public sealed class TutorialService : ITutorialService, ITutorialDebugService, IGameService
    {
        private readonly TutorialCampaignRegistry _registry;
        private readonly TutorialObjectiveFactory _objectiveFactory;
        private readonly TutorialGraphNavigator _navigator = new();
        private readonly TutorialCheckpointService _checkpointService;
        private readonly IGameLoopStateQuery _gameLoopStateQuery;
        private readonly TutorialInputPolicyService _inputPolicyService;
        private readonly ITutorialPresentationService _presentation;
        private readonly ITutorialAnalytics _analytics;
        private readonly IGameStateProvider _gameStateProvider;
        private readonly ReactiveProperty<CGameStateTutorial> _tutorialState;

        private TutorialRuntime _runtime;
        private TutorialStepRuntimeState _activeStep;

        public bool IsActive => _runtime != null && _runtime.IsActive;

        public TutorialService(
            TutorialCampaignRegistry registry,
            TutorialObjectiveFactory objectiveFactory,
            TutorialCheckpointService checkpointService,
            IGameLoopStateQuery gameLoopStateQuery,
            TutorialInputPolicyService inputPolicyService,
            ITutorialPresentationService presentation,
            ITutorialAnalytics analytics,
            IGameStateProvider gameStateProvider,
            ReactiveProperty<CGameStateTutorial> tutorialState)
        {
            _registry = registry;
            _objectiveFactory = objectiveFactory;
            _checkpointService = checkpointService;
            _gameLoopStateQuery = gameLoopStateQuery;
            _inputPolicyService = inputPolicyService;
            _presentation = presentation;
            _analytics = analytics;
            _gameStateProvider = gameStateProvider;
            _tutorialState = tutorialState;
        }

        // =========================================================
        // PRODUCTION API
        // =========================================================

        public void Restore()
        {
            var snapshot = _tutorialState.Value;
            if (string.IsNullOrEmpty(snapshot.campaignId) || snapshot.completed)
                return;

            // Граница save/restore — campaignId в снапшоте это сырой guid-string
            // (TutorialCampaignId.Guid), резолвим через GetCampaignByGuid, а не GetCampaign.
            var definition = _registry.GetCampaignByGuid(snapshot.campaignId);
            if (definition == null)
            {
                Debug.LogError($"[TutorialService] Campaign '{snapshot.campaignId}' не найдена — резюм невозможен.");
                return;
            }

            _runtime = new TutorialRuntime(definition, _tutorialState);

            var decision = _checkpointService.ResolveResume(
                definition, snapshot, _gameLoopStateQuery.CurrentDomain, _navigator);

            switch (decision.Mode)
            {
                case TutorialResumeMode.ResumeCurrent:
                    ActivateStep(decision.StepId);
                    break;

                case TutorialResumeMode.ResumeFromCheckpoint:
                case TutorialResumeMode.ContinueFromResolvedProgress:
                    Debug.LogWarning($"[TutorialService] Резюм '{snapshot.campaignId}' через " +
                                      $"{decision.Mode} → '{decision.StepId?.DebugKey}'.");
                    _analytics.TutorialResumed(snapshot.campaignId);
                    ActivateStep(decision.StepId);
                    break;

                case TutorialResumeMode.Restart:
                    var ctx = decision.FallbackContext.Value;
                    Debug.LogWarning(
                        $"[TutorialService] Forced resume fallback for '{snapshot.campaignId}': " +
                        $"savedCurrentStepId='{ctx.SavedCurrentStepId}', checkpointStepId='{ctx.CheckpointStepId}', " +
                        $"currentDomain={ctx.CurrentDomain}, reason=\"{ctx.Reason}\", fallback=Restart");

                    _analytics.TutorialResumeFallback(
                        snapshot.campaignId, ctx.SavedCurrentStepId, ctx.CheckpointStepId,
                        ctx.CurrentDomain.ToString(), ctx.Reason);

                    // Fix: раньше сбрасывался только currentStepId — checkpointStepId и
                    // completedStepIds оставались от старого прогона и утекали в новый.
                    // Restart обязан означать по-настоящему свежий прогресс.
                    ResetProgression(definition.campaignId, resetStartedTimestamp: false);
                    ActivateStep(definition.entryStepId);
                    break;
            }
        }

        public void StartTutorial(TutorialCampaignId campaignId)
        {
            if (IsActive)
            {
                Debug.LogWarning("[TutorialService] StartTutorial вызван при уже активной кампании — игнор.");
                return;
            }

            var definition = _registry.GetCampaign(campaignId);
            if (definition == null)
            {
                Debug.LogError($"[TutorialService] Campaign '{campaignId?.DebugKey ?? "?"}' не найдена.");
                return;
            }

            ResetProgression(campaignId, resetStartedTimestamp: true);

            _runtime = new TutorialRuntime(definition, _tutorialState);
            _analytics.TutorialStarted(campaignId.Guid);

            // ActivateStep сам персистит, когда реально приземлится на активный шаг
            // (или через CompleteCampaign, если вся кампания состоит из мгновенных завершений).
            ActivateStep(definition.entryStepId);
        }

        /// <summary>Единая точка сброса прогресса кампании — используется и StartTutorial
        /// (свежий старт), и Restore() при вынужденном Restart (см. Fix 4: раньше Restart-ветка
        /// сбрасывала только currentStepId, оставляя checkpointStepId/completedStepIds от
        /// предыдущего прогона). campaignId — typed-ссылка; в персистентный снапшот пишется
        /// её .Guid (та же граница, что у currentStepId/checkpointStepId).</summary>
        private void ResetProgression(TutorialCampaignId campaignId, bool resetStartedTimestamp)
        {
            var guid = campaignId?.Guid;
            StateWriter.Write(_tutorialState, (ref CGameStateTutorial t) =>
            {
                t.campaignId = guid;
                t.currentStepId = null;
                t.checkpointStepId = null;
                t.completedStepIds = new List<string>();
                t.completed = false;
                if (resetStartedTimestamp)
                    t.startedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            });
        }

        public void StopTutorial()
        {
            if (_runtime == null) return;
            _analytics.TutorialAbandoned(_runtime.CampaignId, _activeStep?.Definition.stepId?.Guid);
            _activeStep?.Stop();
            _presentation.Hide();
            _inputPolicyService.Reset();
            _activeStep = null;
            _runtime = null;
        }

        public TutorialProgress GetProgress() => _runtime?.ToProgress(_activeStep?.Definition.chapterId);
        public bool IsStepActive(TutorialStepId stepId) => stepId != null && _activeStep?.Definition.stepId == stepId;
        public bool IsStepCompleted(TutorialStepId stepId) => _runtime?.IsStepCompleted(stepId) ?? false;

        // =========================================================
        // DEBUG API
        // =========================================================

        public void CompleteStepDebug() => FinishActiveStepAndAdvance(TutorialStepOutcome.Completed);
        public void SkipStepDebug() => FinishActiveStepAndAdvance(TutorialStepOutcome.Skipped);

        public void ForceStep(TutorialStepId stepId)
        {
            if (_runtime == null) return;
            _activeStep?.Stop();
            _presentation.Hide();
            _activeStep = null;
            ActivateStep(stepId);
        }

        public void RestartTutorial()
        {
            if (_runtime == null) return;
            var campaignId = _runtime.Definition.campaignId;
            StopTutorial();
            StartTutorial(campaignId);
        }

        public void ClearProgress()
        {
            StopTutorial();
            StateWriter.Write(_tutorialState, (ref CGameStateTutorial t) => t = default);
            Persist();
        }

        // =========================================================
        // INTERNAL
        // =========================================================

        /// <summary>
        /// Итеративный обход графа (никакой рекурсии). Порядок: сначала stepState.Start()
        /// (чистая оценка объективов, presentation ещё не тронута) — если шаг завершился
        /// мгновенно, presentation/input policy/analytics/currentStepId НЕ трогаются вообще
        /// и цикл идёт к следующему шагу. Только для реально активного шага применяются
        /// видимые эффекты и происходит единственный Persist() для этой ветки.
        /// </summary>
        private void ActivateStep(TutorialStepId stepId)
        {
            while (stepId != null)
            {
                var stepDef = _runtime.Definition.GetStep(stepId);
                if (stepDef == null)
                {
                    Debug.LogError($"[TutorialService] Step '{stepId.DebugKey}' не найден — остановка тутора.");
                    StopTutorial();
                    return;
                }

                var stepState = BuildStepState(stepDef);
                bool alreadyComplete = stepState.Start();

                if (alreadyComplete)
                {
                    stepState.Stop();
                    stepId = ResolveTransition(stepDef, TutorialStepOutcome.Completed);
                    continue;
                }

                stepState.OnStepCompleted += HandleAsyncStepCompleted;
                _activeStep = stepState;
                _runtime.SetActiveStep(stepState);

                _inputPolicyService.Apply(stepDef.presentation.inputPolicy);
                _presentation.Show(stepDef.presentation);
                _analytics.StepStarted(_runtime.CampaignId, stepDef.chapterId?.Guid, stepDef.stepId.Guid, stepDef.analyticsStepIndex);

                Persist();
                return;
            }
        }

        private TutorialStepRuntimeState BuildStepState(TutorialStepDefinition stepDef)
        {
            var objectiveStates = new List<TutorialObjectiveRuntimeState>();
            foreach (var objDef in stepDef.objectives.objectives)
            {
                var runtimeObjective = _objectiveFactory.Create(objDef);
                objectiveStates.Add(new TutorialObjectiveRuntimeState(runtimeObjective, objDef.ObjectiveTypeId));
            }
            return new TutorialStepRuntimeState(stepDef, objectiveStates);
        }

        /// <summary>Срабатывает строго асинхронно — из EventBus-колбэка реального игрового
        /// события, никогда не изнутри ActivateStep.</summary>
        private void HandleAsyncStepCompleted() => FinishActiveStepAndAdvance(TutorialStepOutcome.Completed);

        private void FinishActiveStepAndAdvance(TutorialStepOutcome outcome)
        {
            if (_activeStep == null) return;

            var stepDef = _activeStep.Definition;
            var finishedState = _activeStep;

            finishedState.OnStepCompleted -= HandleAsyncStepCompleted;
            _presentation.Hide();
            finishedState.Stop();
            _activeStep = null;

            var nextStepId = ResolveTransition(stepDef, outcome);
            if (nextStepId != null)
                ActivateStep(nextStepId);
        }

        /// <summary>
        /// Чистое разрешение перехода: completed/skipped-учёт + чекпоинт + граф-навигация.
        /// НИКОГДА не персистит сама (кроме терминального случая через CompleteCampaign).
        /// </summary>
        private TutorialStepId ResolveTransition(TutorialStepDefinition stepDef, TutorialStepOutcome outcome)
        {
            if (outcome == TutorialStepOutcome.Completed)
            {
                _runtime.MarkStepCompleted(stepDef.stepId);
                _analytics.StepCompleted(_runtime.CampaignId, stepDef.chapterId?.Guid, stepDef.stepId.Guid, stepDef.analyticsStepIndex);
            }
            else
            {
                // Skip НЕ пишется в completedStepIds — только аналитика.
                _analytics.StepSkipped(_runtime.CampaignId, stepDef.chapterId?.Guid, stepDef.stepId.Guid, stepDef.analyticsStepIndex);
            }

            if (stepDef.isCheckpoint)
            {
                _checkpointService.MarkCheckpoint(_tutorialState, stepDef.stepId);
                _analytics.CheckpointReached(_runtime.CampaignId, stepDef.stepId.Guid);
            }

            // Fix: Terminal и NoTransitionMatched раньше были неразличимы (оба — null),
            // из-за чего NoTransitionMatched ошибочно завершал кампанию.
            var result = _navigator.Resolve(stepDef);

            switch (result.Result)
            {
                case TutorialGraphResult.Terminal:
                    CompleteCampaign();
                    return null;

                case TutorialGraphResult.NoTransitionMatched:
                    // НЕ завершаем кампанию — это ошибка авторинга (см. TutorialStepDefinition.Validate,
                    // правило "unconditional transition must be last"), а не валидный терминальный шаг.
                    Debug.LogError(
                        $"[TutorialService] Step '{stepDef.stepId.DebugKey}' resolved but no transition condition matched " +
                        "and no unconditional fallback exists. Tutorial progression stalled at this step — " +
                        "this is a graph authoring error, fix TutorialStepDefinition.transitions.");
                    return null;

                default: // NextStep
                    return result.NextStepId;
            }
        }

        private void CompleteCampaign()
        {
            _activeStep = null;
            _runtime.MarkCampaignCompleted();
            _inputPolicyService.Reset();
            _analytics.TutorialCompleted(_runtime.CampaignId);
            Persist();
        }

        private void Persist() => _gameStateProvider.SaveGameState();

        private enum TutorialStepOutcome { Completed, Skipped }
    }
}
