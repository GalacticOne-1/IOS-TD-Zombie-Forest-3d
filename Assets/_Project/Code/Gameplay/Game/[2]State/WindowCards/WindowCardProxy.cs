
namespace Galactic1.Window
{
    public class WindowCardProxy
    {
        public WindowCardData Origin { get; }

        public int Id => Origin.Id;
        public string ConfigId => Origin.ConfigId;
        public EWindowCardType Type => Origin.Type;
        public int CardVariant => Origin.CardVariant;

        protected WindowCardProxy(WindowCardData origin)
        {
            Origin = origin;

            
        }
    }
}