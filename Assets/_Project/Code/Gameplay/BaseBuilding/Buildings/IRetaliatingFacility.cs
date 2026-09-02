namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Реализуется боевыми сооружениями с пассивным уроном
    /// (колья, колючая проволока) — наносят ответный урон
    /// атакующему юниту в момент получения удара.
    /// </summary>
    public interface IRetaliatingFacility
    {
        bool TryGetRetaliationDamage(out float damage);
    }
}