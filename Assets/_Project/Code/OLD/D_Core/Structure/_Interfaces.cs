

namespace Galactic1
{
    
    public interface INewSaveData
    {
        /// <summary>
        /// Инициализация для новой игры
        /// </summary>
        void NewSaveData();
    }
    
    public interface IInit
    {
        /// <summary>
        /// Инициализация при старте
        /// </summary>
        void Init();
    }
    
    public interface IAsset
    {
        /// <summary>
        /// Инициализация при старте (раньше загрузки)
        /// </summary>
        void Init();
        
        /// <summary>
        /// Инициализация для новой игры
        /// </summary>
        void NewSaveData();
    }
    


    public interface IContentAccess
    {
        void LoadAccess(int level);
    }

    public interface IFlagController
    {
        void AddFlag(sbyte i);
        void RemoveFlag(sbyte i);
    }

    public interface IFlagWindow
    {
        short flag_request { set; get; }
        /// <summary>
        /// Состояние для флага главной кнопки виджета
        /// </summary>
        void SetStateFlag(bool add);
    }

    public interface IScreenFocus
    {
        /// <summary>
        /// Когда объект становится видимым на экране при переключении меню
        /// <br/>(Был выбран пунк меню в котором находится виджет/объект)
        /// </summary>
        void ScreenFocus();
        /// <summary>
        /// Экран сменился
        /// </summary>
        void ScreenFocusOut();
    }
    
    
    
    public interface IScreenRegular
    {
        void OpenWindow();
        void CloseWindow();
    }
    
    public interface IScreenT
    {
        void OpenWindow<T>(T data);
        void CloseWindow();
    }
    
    public interface IScreenMenu
    {
        void OpenWindow(sbyte id);
        void CloseWindow(sbyte id);
    }

    public interface IScreenStatic
    {
        void LoadContent();

        void ClearContent();
    }

    public interface IScreenRebuild
    {
        /// <summary>
        /// Простой возврат всех элементов виджета для нового включения
        /// </summary>
        void Rebuild();
    }
    
    
    
    
    
    // ------------------- SCENE  OBJECTS
    
    
    public interface IConstructActivator
    {
        /// <summary>
        /// Перевод объекта в рабочее состояние
        /// </summary>
        void SetActivated();
    }
    
    
    
    
    
}