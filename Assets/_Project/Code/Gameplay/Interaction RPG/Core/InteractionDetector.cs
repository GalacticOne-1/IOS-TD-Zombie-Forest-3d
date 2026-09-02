
using System.Collections.Generic;
using Galactic1.Gameplay.Control;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    /// <summary>
    /// Определяет ближайший объект для взаимодействия или атаки.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float detectionRadius = 2f;
        [SerializeField] private LayerMask interactableLayer;
        
        
        
        private IInteractable nearestInteractable;
        private ContactFilter2D filtr;
        List<Collider2D> hits = new();

        
        
        
        private void Awake()
        {
            filtr.SetLayerMask(1 << AppConstants.layer_interaction_obj);
            filtr.useTriggers = true;
        }


        private void Update()
        {
            // 1. Ищем все коллайдеры вокруг игрока
            hits.Clear();
            if (Physics2D.OverlapCircle(transform.position, detectionRadius, filtr, hits) > 0)
            {
                IInteractable closest = null;
                float minDist = float.MaxValue;

                // 2. Сканируем все объекты
                foreach (var hit in hits)
                {
                    var interactable = hit.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        if (!interactable.CanInteract(transform, ControllableSwitcher.IsDragon))
                            continue;

                        float dist = Vector2.Distance(transform.position, hit.transform.position);

                        if (dist < minDist)
                        {
                            minDist = dist;
                            closest = interactable;
                        }
                    }
                }

                // 3. Смена текущего интеракта
                if (closest != nearestInteractable)
                {
                    nearestInteractable?.OnFocusLost();
                    nearestInteractable = closest;
                    nearestInteractable?.OnFocus();

                    ServiceLocator.Current.Get<InteractionSystem>().SetCurrentInteractable(nearestInteractable);
                }
            }
            else
            {
                // ничего не найдено — очистим
                if (nearestInteractable != null)
                {
                    nearestInteractable.OnFocusLost();
                    nearestInteractable = null;
                    ServiceLocator.Current.Get<InteractionSystem>().SetCurrentInteractable(null);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

    }
}