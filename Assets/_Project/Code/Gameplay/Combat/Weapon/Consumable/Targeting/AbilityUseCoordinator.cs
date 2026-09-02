using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Abilities
{
    /// <summary>
    /// Оркестратор использования ability (Runtime уровень)
    /// </summary>
    public sealed class AbilityUseCoordinator : IGameService
    {
        private readonly SceneGameModeService _sceneGameModeService;
        private readonly ItemUseService _itemUse;

        public AbilityUseCoordinator(
            SceneGameModeService sceneGameModeService,
            ItemUseService itemUse)
        {
            _sceneGameModeService = sceneGameModeService;
            _itemUse = itemUse;
        }

        public void Use(ItemUseContext ctx)
        {
            switch (ctx.UseModule.Behaviour.ActivationType)
            {
                case UseActivationType.Instant:
                    _itemUse.Use(ctx);
                    break;

                case UseActivationType.Targeting:
                    StartTargeting(ctx);
                    break;
            }
        }

        private void StartTargeting(ItemUseContext ctx)
        {
            var slot = ctx.User.QuickSlot.GetSlot(
                ctx.User.InventorySource.Equipment,
                ctx.QuickSlotIndex);

            var request = new TargetingRequest
            {
                User = ctx.User,
                QuickSlotIndex = ctx.QuickSlotIndex,
                UseModule = slot.Item.GetModule<UseModule>(),

                OnConfirm = pos =>
                {
                    ctx.TargetPosition = pos;
                    
                    // ★ только здесь создаём и исполняем команду
                    var repo = ServiceLocator.Current.Get<SurvivorRepository>().TryGet(ctx.User.Id);
                    var instance = repo.instance;
                    if (instance != null)
                    {
                        var cmd = new AbilityCommand(ctx);

                        // ВАЖНО: FSM стартует ТОЛЬКО после подтверждения
                        instance.StateMachine.Execute(cmd);
                    }
                    
                    _itemUse.Use(ctx);
                    ctx.OnConfirmed?.Invoke();
                    _sceneGameModeService.SetMode(GameModeType.Raid);
                },

                OnCancel = () =>
                {
                    ctx.OnCancelled?.Invoke();
                    _sceneGameModeService.SetMode(GameModeType.Raid);
                }
            };

            var mode = _sceneGameModeService.Get<AbilityTargetingGameMode>(GameModeType.AbilityTargeting);

            mode.Setup(request);

            _sceneGameModeService.SetMode(GameModeType.AbilityTargeting);
        }
        
        public void Cancel()
        {
            _sceneGameModeService.SetMode(GameModeType.Raid);
        }
    }
}