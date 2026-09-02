namespace Galactic1.Game.UI.Stats.DTO
{
    /// <summary>
    /// пустая строка
    /// </summary>
    public sealed class StatSpacerDto : StatDtoBase
    {
        
        public readonly float Height;

        public StatSpacerDto(float height = 0) : base(StatLayoutType.Spacer)
        {
            Height = height == 0 ? 20 : height;
        }
    }
}