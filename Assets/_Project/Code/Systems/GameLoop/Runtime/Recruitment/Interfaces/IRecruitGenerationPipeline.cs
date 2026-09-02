using Galactic1.Game.Buildings.Proxy;

namespace Galactic1.Code.Systems.Runtime.Recruitment
{
    /// <summary>
    /// Интерфейс пайплайна генерации рекрутов.
    /// </summary>
    public interface IRecruitGenerationPipeline
    {
        RecruitOfferData Generate();
    }
}