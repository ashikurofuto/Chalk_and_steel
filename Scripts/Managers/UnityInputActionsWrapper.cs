using System;
using UnityEngine;




namespace Architecture.Services
{
    /// <summary>
    /// Реализация обертки над Unity Input System.
    /// Прямая работа с InputSystem_Actions.
    /// </summary>
    public sealed class UnityInputActionsWrapper : IInputActionsWrapper
    {
        private readonly InputSystem_Actions _inputActions;
        private Vector2 _moveVector;
        private bool _interactPressed;
        private bool _inventoryPressed;
        private bool _pausePressed;

        public Vector2 Move => _moveVector;
        public bool Interact => _interactPressed;
        public bool Inventory => _inventoryPressed;
        public bool Pause => _pausePressed;

        public event Action<Vector2> OnMoveChanged;
        public event Action OnInteract;
        public event Action OnInventory;
        public event Action OnPause;

        public UnityInputActionsWrapper()
        {
            _inputActions = new InputSystem_Actions();
            SetupCallbacks();
        }

        private void SetupCallbacks()
        {
            // Настройка движения
            _inputActions.Player.Move.performed += context =>
            {
                _moveVector = context.ReadValue<Vector2>();
                OnMoveChanged?.Invoke(_moveVector);
            };
            _inputActions.Player.Move.canceled += _ =>
            {
                _moveVector = Vector2.zero;
                OnMoveChanged?.Invoke(_moveVector);
            };

            // Настройка взаимодействия
            _inputActions.Player.Interact.started += _ =>
            {
                _interactPressed = true;
                OnInteract?.Invoke();
            };
            _inputActions.Player.Interact.canceled += _ => _interactPressed = false;

            // Настройка инвентаря
            _inputActions.Player.Inventory.started += _ =>
            {
                _inventoryPressed = true;
                OnInventory?.Invoke();
            };
            _inputActions.Player.Inventory.canceled += _ => _inventoryPressed = false;

            // Настройка паузы
            _inputActions.Player.Pause.started += _ =>
            {
                _pausePressed = true;
                OnPause?.Invoke();
            };
            _inputActions.Player.Pause.canceled += _ => _pausePressed = false;
        }

        public void Enable()
        {
            _inputActions.Player.Enable();
            _inputActions.UI.Enable();
        }

        public void Disable()
        {
            _inputActions.Player.Disable();
            _inputActions.UI.Disable();
        }

        public void Dispose()
        {
            _inputActions?.Dispose();
        }
    }
}