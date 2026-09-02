using Galactic1;

namespace Galactic1.Gameplay.Player
{
    /// <summary>
    /// Аналог HeroStateMachine из LDoE.
    /// Управляет активным контроллером игрока (пеший, дракон, пустой).
    /// Логика:
    /// - хранит текущий контроллер
    /// - выключает старый перед переключением
    /// - включает новый
    /// </summary>
    public class HeroStateMachine : IGameService
    {
        public CHARACTER_CONTROLLER Current { get; private set; }
        
        private readonly CHARACTER_CONTROLLER_Player_Unit unitController;
        private readonly CHARACTER_CONTROLLER_Player_Dragon dragonController;
        private readonly CHARACTER_CONTROLLER_Empty emptyController;

        public enum EPlayerController
        {
            Empty,
            Unit,
            Dragon
        }

        public bool IsDragon { get; private set; }

        
        public HeroStateMachine()
        {
            unitController   = new CHARACTER_CONTROLLER_Player_Unit();
            dragonController = new CHARACTER_CONTROLLER_Player_Dragon();
            emptyController  = new CHARACTER_CONTROLLER_Empty();
        }

        
        public void ChangeState(EPlayerController type)
        {
            // Выход из предыдущего контроллера
            Current?.Exit();

            switch (type)
            {
                case EPlayerController.Empty:
                    Current = emptyController;
                    IsDragon = false;
                    break;
                
                case EPlayerController.Unit:
                    Current = unitController;
                    IsDragon = false;
                    break;
                
                case EPlayerController.Dragon:
                    Current = dragonController;
                    IsDragon = true;
                    break;
            }

            // Вход в новый контроллер
            Current.Enter();
        }
    }
}