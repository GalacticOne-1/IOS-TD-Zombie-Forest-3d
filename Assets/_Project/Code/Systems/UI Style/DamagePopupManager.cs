using System.Collections;
using Galactic1.Configs;
using Galactic1.PoolObject;
using Gameplay;
using UnityEngine;

namespace Galactic1
{
    public class DamagePopupManager: IGameService
    {
        
        
        public void ShowDamage(
            string damageType,
            Vector3 worldPosition, 
            int damage, 
            bool isCritical = false)
        {
            var style = StyleManager.GetPopupStyle(damageType);
            if (style == null)
            {
                Debug.LogWarning($"⚠ No popup style found for {damageType}");
                return;
            }
            
            // берём из пула
            ServiceLocator.Current.Get<EffectRequestSystem>().Request(
                new EffectRequest()
                {
                    Id = null,//"Damage Popup",
                    Position = worldPosition + Vector3.up * 1f,
                },

                EffectPriority.Normal,

                _ =>
                {
                    var popup = _.GetComponent<DamagePopup>();
                    popup.gameObject.SetActive(true);
                    popup.transform.localScale = Vector3.one * 2f;


                    popup.gameObject.SetActive(true);

                    // переводим мировую позицию в экранные координаты
                    //Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1f);
                    //popup.transform.position = worldPosition + Vector3.up * 1f; //screenPos;

                    popup.Setup(style, damage, isCritical);
                    // popup.OnHide = () =>
                    // {
                    //     popup.gameObject.SetActive(false);
                    //     //popupPool.Enqueue(popup);
                    // };
                });
        }
    }
}