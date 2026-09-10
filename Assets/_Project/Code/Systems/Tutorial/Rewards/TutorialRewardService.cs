using System;
using System.Collections.Generic;
using Galactic1.Code.Core.State;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.Tasks;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Game.Meta.Items;
using R3;

namespace Galactic1.Code.Systems.Tutorial.Rewards
{
    /// <summary>
    /// Единственная точка оркестрации Tutorial → Inbox для наград шагов. Не решает,
    /// завершён ли шаг (это TutorialService), не знает про UI, не хранит Inbox-логику
    /// сама — только конвертирует Authoring → InboxService.AddReward и ведёт
    /// персистентный claim-флаг.
    ///
    /// Идемпотентность: claimedRewardStepIds в CGameStateTutorial — тот же паттерн,
    /// что completedStepIds у TutorialRuntime. GrantIfNeeded безопасно вызывать
    /// многократно для одного и того же шага (debug complete, дубль-колбэк, resume) —
    /// после первого успешного вызова все последующие — no-op.
    ///
    /// Crash-safety: GrantIfNeeded вызывается TutorialService.ResolveTransition() СИНХРОННО
    /// в той же точке, что и _runtime.MarkStepCompleted() — обе мутации (completedStepIds
    /// и claimedRewardStepIds) живут в одном ReactiveProperty&lt;CGameStateTutorial&gt; и
    /// физически сохраняются одним и тем же вызовом SaveGameState() (единый JSON-снэпшот
    /// всего GameState, см. JsonGameStateProvider). Поэтому у выдачи награды нет
    /// отдельного crash-окна сверх уже существующего у завершения шага: либо оба флага
    /// (completed + claimed) переживают краш вместе, либо оба откатываются вместе, и шаг
    /// со всеми объективами и наградой корректно переигрывается при Restore().
    /// </summary>
    public sealed class TutorialRewardService
    {
        private InboxService _inbox;
        private readonly ReactiveProperty<CGameStateTutorial> _tutorialState;

        public TutorialRewardService(ReactiveProperty<CGameStateTutorial> tutorialState)
        {
            _tutorialState = tutorialState;
        }

        public bool IsClaimed(TutorialStepId stepId)
            => stepId != null
               && _tutorialState.Value.claimedRewardStepIds != null
               && _tutorialState.Value.claimedRewardStepIds.Contains(stepId.Guid);

        /// <summary>
        /// Выдаёт все настроенные награды шага в Inbox, если ещё не выданы. Вызывать
        /// ТОЛЬКО из точки генуинного завершения шага (TutorialService.ResolveTransition,
        /// ветка Completed) — не из Skip, не из простого показа шага.
        /// </summary>
        /// <returns>true, если награда была выдана именно этим вызовом.</returns>
        public bool GrantIfNeeded(TutorialStepDefinition stepDef)
        {
            if (stepDef?.stepId == null) return false;
            if (!stepDef.reward.HasRewards) return false;
            if (IsClaimed(stepDef.stepId)) return false;
            
            _inbox ??= ServiceLocator.Current.Get<InboxService>();

            foreach (var itemDef in stepDef.reward.items)
            {
                if (itemDef?.itemId == null) continue; // защитно; авторинг-Validate уже не пускает такое в билд
                _inbox.AddReward(itemDef.itemId, itemDef.amount, itemDef.durability, itemDef.ammoInMagazine);
            }

            MarkClaimed(stepDef.stepId);
            return true;
        }

        /// <summary>Read-only снэпшот наград шага для презентации — HUD не видит
        /// TutorialRewardItemDefinition/ItemConfig-авторинг напрямую.</summary>
        public IReadOnlyList<ScenarioTaskReward> GetRewardSnapshot(TutorialStepDefinition stepDef)
        {
            if (stepDef?.reward == null || !stepDef.reward.HasRewards)
                return Array.Empty<ScenarioTaskReward>();

            var list = new List<ScenarioTaskReward>(stepDef.reward.items.Count);
            foreach (var itemDef in stepDef.reward.items)
            {
                if (itemDef?.itemId == null) continue;

                GameContent.Items.TryGet(itemDef.itemId, out var config);
                
                if (config != null)
                    list.Add(new ScenarioTaskReward(
                        config,
                        itemDef.amount,
                        itemDef.durability,
                        itemDef.ammoInMagazine));
            }

            return list;
        }

        private void MarkClaimed(TutorialStepId stepId)
        {
            var guid = stepId.Guid;
            var current = _tutorialState.Value.claimedRewardStepIds ?? new List<string>();
            if (current.Contains(guid)) return;

            var updated = new List<string>(current) { guid };
            StateWriter.Write(_tutorialState, (ref CGameStateTutorial t) => t.claimedRewardStepIds = updated);
        }
    }
}