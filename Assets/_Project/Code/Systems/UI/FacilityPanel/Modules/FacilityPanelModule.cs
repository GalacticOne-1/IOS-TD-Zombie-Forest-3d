
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.UI.Buildings.DTO;
using UnityEngine;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Базовый класс любого UI-модуля панели здания.
    /// Каждый модуль отвечает только за одну механику.
    /// </summary>
    public abstract class FacilityPanelModule : MonoBehaviour
    {
        protected bool isBound;
        public bool IsBound => isBound;

        
        /// true - модуль включается сразу при открытии панели
        public virtual bool IsAutoActivate => true;

        /// <summary>
        /// Проверяет, поддерживается ли модуль данным зданием.
        /// </summary>
        public abstract bool IsSupported(FacilityDTO dto);
        
        public virtual void IsUpgradeable(FacilityDTO dto) {}

        /// <summary>
        /// Привязка данных здания к UI.
        /// </summary>
        public virtual void Bind(FacilityDTO dto, object sceneAdapter, FacilityUpgradeSceneAdapter upgradeAdapter)
        {
            isBound = true;
            if (IsAutoActivate)
                gameObject.SetActive(true);
        }

        
        /// <summary>
        /// Вызывается при изменении рантайм состояния
        /// <br/>Все связанное с ui делать в этом методе
        /// <br/>(Bind вызывается только один раз при открытии панели!!!)
        /// </summary>
        /// <param name="dto"></param>
        public abstract void Rebind(FacilityDTO dto);

        public virtual void Unbind()
        {
            isBound = false;
            gameObject.SetActive(false);
        }

        public void BindIfSupported(FacilityDTO dto, object sceneAdapter, FacilityUpgradeSceneAdapter upgradeAdapter)
        {
            if (IsSupported(dto))
                Bind(dto, sceneAdapter, upgradeAdapter);
            else
                Unbind();
        }
    }
}