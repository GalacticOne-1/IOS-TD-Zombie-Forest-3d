using Galactic1.Code.Systems.GameLoop;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Отвечает ТОЛЬКО за вычисление штрафа. Ничего не изменяет в инвентаре.
    /// Возвращает immutable результат (CampDefensePenaltyResult).
    /// </summary>
    public interface ICampDefensePenaltyCalculator
    {
        CampDefensePenaltyResult Calculate(GameLoopContext context);
    }
}