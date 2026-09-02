namespace Galactic1.Code.Systems.Runtime.Building
{
    public static class FootprintRotation
    {
        public static (int width, int height) Rotate(
            int width,
            int height,
            int rotation)
        {
            rotation %= 4;

            if (rotation == 1 || rotation == 3)
                return (height, width);

            return (width, height);
        }
    }
}