
using Galactic1.Code.Gameplay.Projectiles;
using Galactic1.Code.Gameplay.Weapons.View;
using Galactic1.Code.UI.HUD.Enemy;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;

namespace Galactic1.PoolObject
{
    public class RaidPoolRegistry
    {
        /// <summary>
        /// Временный пул для рейда, очищается при выходе из локации
        /// </summary>
        /// <param name="poolManager"></param>
        /// <param name="poolConfigs"></param>
        public RaidPoolRegistry(PoolManager poolManager, ObjectPoolConfigs poolConfigs)
        {
            // #1 vfx
            foreach (var config in poolConfigs.EffectConfigs)
            {
                poolManager.AutoRegisterFromResources<EffectPoolable, EffectConfig>(
                    config, 
                    config,
                    AppConstants.PATH_FX + config.PrefabPath);
            }
            
            // #2 unit indicator
            poolManager.AutoRegisterFromResources<UnitIndicatorWidget, UnitIndicatorWidgetConfig>(
                poolConfigs.UnitIndicatorWidgetConfig,
                poolConfigs.UnitIndicatorWidgetConfig,
                AppConstants.PATH_GAMEPLAY + poolConfigs.UnitIndicatorWidgetConfig.PrefabPath,
                PoolLifetime.Scene,
                ServiceLocator.Current.Get<UIManager>().TransformRoot.floatWorldRoot);
            
            
            // #3 grenade
            foreach (var config in poolConfigs.GrenadeConfigs)
            {
                poolManager.AutoRegisterFromResources<GrenadeProjectile, ItemConfig>(
                    config,
                    config,
                    AppConstants.PATH_ITEMS + config.PrefabPath);
            }
            
            // #4 bullets
            foreach (var config in poolConfigs.BulletConfigs)
            {
                poolManager.AutoRegisterFromResources<BaseProjectile, AmmoDefinition>(
                    config,
                    config,
                    AppConstants.PATH_ITEMS + config.PrefabPath);
            }
        }
    }

}