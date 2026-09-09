using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.Gameplay.BaseBuilding
{
    // обычные здания не участвуюзие в орде
    public class CampFacilityInstance : FacilityInstance
    {

        [SerializeField] private GameObject interactionIndicator;
        
        
        public override void Entity_Setup<T>(T data)
        {

            // === для режима орды отключаем подсветку объектов если есть
            if (interactionIndicator != null)
            {
                EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
                {
                    var context = ServiceLocator.Current.Get<GameSession>().GameLoopContext;
                    interactionIndicator.SetActive(!context.IsRaidState);
                }));
            }
        }
    }
}