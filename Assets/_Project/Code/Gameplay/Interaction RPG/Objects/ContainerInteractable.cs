
using Galactic1.Gameplay.Player.StateMachine;
using Galactic1.Systems.Inventory;
using Galactic1.Code.UI.Inventory;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    /// <summary>
    /// Универсальный контейнер (сундук, сейф, труп, мешок, коробка).
    /// Логика различается через ContainerConfig.
    /// </summary>
    public class ContainerInteractable : InteractableBase, ILongInteractable, IActionInteractable
    {
        [SerializeField] private ContainerConfig config;

        public float RequiredTime => config.openTime;
        public bool RequiresProgressBar => config.requiresProgressBar;

        public bool IsFinished { get; private set; }

        public override ActionType ActionType => ActionType.OpenContainer;
        
        
        public override void Interact(Transform interactor)
        {
            ServiceLocator.Current.Get<InventoryManagementWindow>().OnClosed += Close;

            switch (config.type)
            {
                case ContainerType.InstantOpen:
                    Open(interactor);
                    break;

                case ContainerType.TimedOpen:
                    // запуск из PlayerActionController!
                    Open(interactor);
                    break;

                case ContainerType.CodeLocked:
                    RequestCodePanel();
                    break;

                case ContainerType.Corpse:
                    OpenCorpse(interactor);
                    break;
            }
        }

        public virtual void Close()
        {
            ServiceLocator.Current.Get<InventoryManagementWindow>().OnClosed -= Close;
            
            switch (config.type)
            {
                case ContainerType.InstantOpen:
                    
                    break;

                case ContainerType.TimedOpen:
                    
                    break;

                case ContainerType.CodeLocked:
                   
                    break;

                // закрываем труп, если пустой
                case ContainerType.Corpse:
                    if (!GetComponent<IInventoryContainer>().Inventory.HaveItems())
                        enabledForInteraction = false;
                    break;
            }
        }

        
        


        private void RequestCodePanel()
        {
            // открыть UI ввода кода
            //ServiceLocator.Current.Get<UIController>().ShowCodePanel(config.correctCode, OnCodeEntered);
        }

        private void OnCodeEntered(bool correct)
        {
            if (correct)
                Open(null);
            else 
                Debug.Log("Wrong code!");
        }

        private void Open(Transform interactor)
        {
            IsFinished = true;

            Debug.Log($"Container {name} opened!");

            // тут вызывается лут UI
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Inventory, null,
                _ =>
                {
                    // _.GetComponent<InventoryManagementWindow>().Open(
                    //     ServiceLocator.Current.Get<InventoryRepository>().PlayerInventory,
                    //     GetComponent<IInventoryContainer>());
                });
        }

        private void OpenCorpse(Transform interactor)
        {
            IsFinished = true;
            

            Debug.Log("Opening Player Corpse");
            //ServiceLocator.Current.Get<LootSystem>().ShowLoot(config.corpseInventory);
        }

        public override InteractionInfo GetInfo()
        {
            return new InteractionInfo
            {
                // Name = config.DisplayName,
                // Icon = config.Icon,
                // IsAvailable = !isOpened
            };
        }

        
    }
}
