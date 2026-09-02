using System;
using UnityEngine;

namespace Galactic1.Core.Input
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        
        
        public Vector2 MoveDirection { get; private set; }
        public ControllableType ActiveControllable { get; private set; }

        public event Action<ControllableType> OnControllableChanged;
        public event Action OnAttack;
        public event Action OnInteract;
        public event Action OnInventory;

        
        
        
        
        void Awake()
        {
            Instance = this;
        }
        

        public void SetControllable(ControllableType type)
        {
            ActiveControllable = type;
            OnControllableChanged?.Invoke(type);
        }


        public void SetMovement(Vector2 v) => MoveDirection = v;

        public void AttackPressed() => OnAttack?.Invoke();
        public void InteractPressed() => OnInteract?.Invoke();
        public void InventoryPressed() => OnInventory?.Invoke();
    }
}