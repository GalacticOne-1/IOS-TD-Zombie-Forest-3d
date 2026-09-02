namespace Galactic1.Game.UI.Stats.DTO
{
    public abstract class StatDtoBase : IStatLayoutItemDto
    {
        readonly StatLayoutType _layoutType;
        public StatLayoutType LayoutType => _layoutType;
        
        
        protected StatDtoBase(StatLayoutType layoutType)
        {
            _layoutType = layoutType;
        }

        
    }
}