namespace Galactic1.PoolObject
{
    // Инициализируется один раз при старте приложения
    // UI эффекты, звуки, общие партиклы
    // Никогда не дестроится
    public class GlobalPoolRegistry
    {
        public GlobalPoolRegistry(PoolManager poolManager, ObjectPoolConfigs poolConfigs)
        {
            // активация сервиса эффектов (пул заполняется в RaidPoolRegistry)
            ServiceLocator.Current.Get<EffectRequestSystem>().Initialize(poolConfigs.EffectConfigs);
            
            
            
        }
    }
}