namespace Galactic1.Code.WorldMap.Intel
{
    /// <summary>
    /// Расширение для получения числа активных иконок на UI.
    /// </summary>
    public static class LootVolumeExtensions
    {
        public static int GetActiveIcons(this int enumStage)
        {
            return enumStage switch
            {
                0 => -1,    // не использовать
                1 => 0,     // показать "?"
                _ => enumStage-1
            };
        }
    }
}