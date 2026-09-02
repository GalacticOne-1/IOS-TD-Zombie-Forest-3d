using Galactic1;

namespace Galactic1
{

    public interface ISceneActivator
    {
        /// <summary>
        /// Запуск метода по готовности сцены
        /// <br/>Для методов запускающих игру
        /// </summary>
        void Activator();
    }


    public interface IWidgetController
    {
        void ShowWidget();
        void HideWidget();
    }


    public interface IServerTimeDependance
    {
        /// <summary>
        /// Запуск из TimeManagement для проверки состояния,
        /// если есть зависимость от времени
        /// </summary>
        void UpdateStatus();
    }


    public interface IWidgetCard
    {
        /// <summary>
        /// Загружает карточку для виджета информацией
        /// </summary>
        /// <param name="card"></param>
        /// <typeparam name="T"></typeparam>
        void LoadInfoCard<T>(T card);
    }
    
    
    
    
    
    
    // ---------------------- DEFAULT

  

    

    public interface IAbilityCtrl
    {
        /// <summary>
        /// Принудиткельно Останавливает действие способности
        /// </summary>
        void Clear();
    }
}