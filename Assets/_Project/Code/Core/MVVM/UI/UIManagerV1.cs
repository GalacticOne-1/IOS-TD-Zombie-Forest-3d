

namespace Galactic1
{
    public abstract class UIManagerV1
    {
        protected readonly DIContainer Container; // чтобы вытаскивать барахло, чтобы собирать вьюмодели окошек

        protected UIManagerV1(DIContainer container)
        {
            Container = container;
        }
    }
}