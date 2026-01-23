using Architecture.GlobalModules;
using Architecture.Services;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputService : IInputService
{
    private readonly IEventBus _eventBus;
    private InputSystem_Actions _inputActions; // Добавлено

    private Vector2 _cachedMoveDirection;
    private bool _isEnabled;

    public InputService(
        IEventBus eventBus)
    {
        _eventBus = eventBus;

        // Инициализируем InputSystem_Actions
        _inputActions = new InputSystem_Actions();

        Enable();
    }

    public void Enable()
    {
        if (_isEnabled) return;
        _isEnabled = true;

        // Включаем Input Actions
        _inputActions.Player.Enable();
    }

    public void Disable()
    {
        if (!_isEnabled) return;

        _isEnabled = false;

        // Отключаем Input Actions
        _inputActions.Player.Disable();
    }

    public Vector2 GetMoveDirection()
    {
        // Обновляем кэшированное направление движения из InputSystem
        if (_isEnabled)
        {
            _cachedMoveDirection = _inputActions.Player.Move.ReadValue<Vector2>();

            // Публикуем событие, если направление изменилось
            if (_cachedMoveDirection.magnitude > 0.1f)
            {
                _eventBus.Publish(new MoveInputEvent(_cachedMoveDirection));
            }
        }

        return _cachedMoveDirection;
    }

    public bool IsInteractPressed()
    {
        if (_isEnabled)
        {
            return _inputActions.Player.Interact.triggered;
        }
        return false;
    }

    public bool IsInventoryPressed()
    {
        if (_isEnabled)
        {
            return _inputActions.Player.Inventory.triggered;
        }
        return false;
    }

    public bool IsPausePressed()
    {
        if (_isEnabled)
        {
            return _inputActions.Player.Pause.triggered;
        }
        return false;
    }

    public bool IsUndoPressed()
    {
        if (_isEnabled)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;

            return (keyboard.ctrlKey.isPressed || keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
                   && keyboard.zKey.wasPressedThisFrame;
        }
        return false;
    }
}