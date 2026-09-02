using Galactic1.Game.UI.Stats.DTO;

namespace Galactic1.Game.UI.Stats
{
    public class SpacerStatView : StatViewBase
    {


        public override void Bind(StatDtoBase data)
        {
            base.Bind(data);
            
            if (data is StatSpacerDto spacer)
            {
                var sizeDelta = gameObject.CMP_RectTr().sizeDelta;
                sizeDelta.y = spacer.Height;
                gameObject.CMP_RectTr().sizeDelta = sizeDelta;
            }
        }

        public override void ResetView() {}
    }
}