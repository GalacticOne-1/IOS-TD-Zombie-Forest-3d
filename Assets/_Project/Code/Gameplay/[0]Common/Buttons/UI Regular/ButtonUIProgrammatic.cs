
namespace Galactic1.UI.Core
{
    public class ButtonUIProgrammatic : BaseUIButton
    {
        public override void Initialize(DIContainer container = null)
        {
            originalScale = transform.localScale;
        }
    }
}