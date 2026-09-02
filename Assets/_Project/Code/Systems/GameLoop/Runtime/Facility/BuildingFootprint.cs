
namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Описывает размер здания в клетках сетки.
    ///
    /// Например:
    /// 1x1 — сундук
    /// 2x2 — печь
    /// 3x3 — дом
    /// 1x4 — стена
    /// </summary>
    public class BuildingFootprint
    {
        public int Width { get; }
        public int Height { get; }

        public BuildingFootprint(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        public BuildingFootprint Rotate(int rotation)
        {
            rotation %= 4;

            // 90 / 270 меняют местами width/height
            if (rotation % 2 == 1)
                return new BuildingFootprint(Height, Width);

            return new BuildingFootprint(Width, Height);
        }
    }
}