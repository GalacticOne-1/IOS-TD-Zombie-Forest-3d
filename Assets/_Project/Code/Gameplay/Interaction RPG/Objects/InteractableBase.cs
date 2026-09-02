
using Game.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    /// <summary>
    /// Базовый класс интеракта. Реализует регистрацию в реестре, базовый highlight,
    /// простую доступность и позицию.
    /// Наследуйся и переопределяй Interact/CanInteract/GetInfo.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        public enum InteractAccess
        {
            PlayerOnly,
            DragonOnly,
            Both
        }
        
        [field:Header("Base basicSettings")]
        [field: SerializeField] public InteractAccess Access { get; private set; }
        
        
        [Tooltip("Если false — интеракт всегда недоступен")]
        [SerializeField] protected bool enabledForInteraction = true;
        

        public IObjectContext IObjectContext => GetComponent<ObjectContext>();
        public Transform Tr => transform;
        public virtual Vector2 WorldPosition => transform.position;
        public virtual bool IsAvailable => enabledForInteraction;

        public abstract ActionType ActionType { get; }


        protected virtual void OnEnable()
        {
            InteractablesRegistry.Register(this);
        }

        protected virtual void OnDisable()
        {
            InteractablesRegistry.Unregister(this);
        }


        

        public virtual bool CanInteract(Transform interactor, bool isDragon)
        {
            return Access switch
            {
                InteractAccess.PlayerOnly => !isDragon && IsAvailable,
                InteractAccess.DragonOnly => isDragon && IsAvailable,
                InteractAccess.Both => IsAvailable,
                _ => IsAvailable
            };
        }

        public abstract void Interact(Transform interactor);
        public virtual InteractionInfo GetInfo()
        {
            return new InteractionInfo { Name = gameObject.name, Icon = null, IsAvailable = IsAvailable };
        }
        public virtual void OnFocus()
        {
            ServiceLocator.Current.Get<InteractionSystem>()._interactionHighlight.Show(GetComponent<IObjectContext>());
        }

        public virtual void OnFocusLost()
        {
            ServiceLocator.Current.Get<InteractionSystem>()._interactionHighlight.Hide();
        }
    }
}