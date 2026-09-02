
namespace Galactic1.PoolObject
{
    /// <summary>
    /// Типизированный сеттер конфига.
    /// Реализуй как IPoolItemConfig<EffectConfig> на конкретном компоненте.
    /// </summary>
    public interface IPoolItemConfig<in TConfig>
        where TConfig : UnityEngine.ScriptableObject
    {
        void SetConfig(TConfig config);
    }
}