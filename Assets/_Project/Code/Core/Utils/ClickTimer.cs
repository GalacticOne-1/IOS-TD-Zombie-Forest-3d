using System;

namespace Galactic1.Utility
{
    using System;

    public class ClickTimer
    {
        private float clickTimeout; // Время тайм-аута для клика
        private float timeSincePress; // Время с момента нажатия кнопки
        private bool isPressed; // Флаг, указывающий, что кнопка нажата

        // Конструктор класса
        public ClickTimer(float timeout)
        {
            clickTimeout = timeout;
            timeSincePress = 0f;
            isPressed = false;
        }

        // Вызывается при нажатии на кнопку
        public void OnPress()
        {
            timeSincePress = 0f; // Сбрасываем таймер при нажатии
            isPressed = true; // Флаг нажатия
        }

        // Вызывается при отпускании кнопки
        public bool OnRelease(float deltaTime)
        {
            if (isPressed)
            {
                timeSincePress += deltaTime; // Увеличиваем время с момента нажатия
                DLog.Alert($"click {timeSincePress}");
                if (timeSincePress > clickTimeout)
                {
                    // Если время с момента нажатия больше, чем clickTimeout, то клик не срабатывает
                    isPressed = false; // Сброс флага нажатия
                    return false;
                }

                // Если время прошло нормально, то клик срабатывает
                isPressed = false; // Сброс флага нажатия
                return true;
            }

            return false; // Если кнопка не была нажата, ничего не происходит
        }

        // Метод для обновления времени, если нужно
        public void UpdateTime(float deltaTime)
        {
            if (isPressed)
            {
                timeSincePress += deltaTime;
            }
        }
    }


}