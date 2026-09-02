using Galactic1.Core;
using Galactic1.Configs;
using UnityEngine;

namespace Galactic1
{

    /*
     *      Методы для обучения
     */



    public class TUTORIAL_Check_Status
    {
        /// <summary>
        /// Закрывает обучение если оно было начато и не закончено
        /// <br/>! Должно вызываться в самом начале запуска системы !
        /// </summary>
        public TUTORIAL_Check_Status()
        {
            // if (ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Tutorial.Value.state == (byte)ETutorial.ACTIVE)
            // {
            //     //ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Tutorial.Value.state = (byte)ETutorial.FINISHED;
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Tutorial.Value =
            //         ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Tutorial.Value;
            // }
        }
    }


    public class TUTORIAL_Status
    {
        public TUTORIAL_Status(out bool not_active)
        {
            // var config = ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>();
            // not_active = !config.requiresTutorial || 
            //              ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.Tutorial.Value.state == (byte)ETutorial.FINISHED;
            not_active = true;
        }
    }


    public class TUTORIAL_Start
    {
        /// <summary>
        /// Для запуска обучение
        /// </summary>
        public TUTORIAL_Start()
        {
            //Tutorial.I.Activator();
            //TutorialFreeForm.I.Activator();
            //ServiceLocator.Current.Get<TutorialController>().Activate();
        }
    }

    public class TUTORIAL_TaskComplete
    {
        /// <summary>
        /// Для выполнения задания
        /// </summary>
        public TUTORIAL_TaskComplete()
        {
           // ServiceLocator.Current.Get<TutorialController>().TaskComplete();
        }
    }
    
    public class TUTORIAL_StepComplete
    {
        /// <summary>
        /// Для выполнения пункта в задании
        /// </summary>
        public TUTORIAL_StepComplete()
        {
            //ServiceLocator.Current.Get<TutorialController>().StepComplete();
        }
    }





    public class TUTORIAL_AvailButton
    {
        public TUTORIAL_AvailButton(GameObject button, out bool isTutorial)
        {
            isTutorial = false;
            //isTutorial = ServiceLocator.Current.Get<TutorialController>().AvailableClick(button);
        }
    }


}