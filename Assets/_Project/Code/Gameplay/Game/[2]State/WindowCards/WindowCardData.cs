namespace Galactic1.Window
{
    public class WindowCardData
    {
        public int Id { get; set; }                     // уникальный ид 
        public string ConfigId { get; set; }            // Для поиска настроек
        public EWindowCardType Type { get; set; }       // Тип сущности, для быстрого понимания что это
        public int CardVariant { get; set; }            // вариант карточки внутри виджета
    }
}