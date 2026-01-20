// Managers/InputService.cs (����������)
using Architecture.GlobalModules;
using Architecture.Services;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputService : IInputService
{
    private readonly IEventBus _eventBus;
    private readonly IInputActionsWrapper _inputWrapper;

    private Vector2 _cachedMoveDirection;
    private bool _isEnabled;

    // ��� ����������� �����
    private Vector2 _lastMoveInput;
    private bool _moveKeyWasPressed;

    public InputService(
        IEventBus eventBus,
        IInputActionsWrapper inputWrapper)
    {
        _eventBus = eventBus;
        _inputWrapper = inputWrapper;

        // �������� �� ������� �����
        _inputWrapper.OnMoveChanged += OnMoveChanged;
        _inputWrapper.OnInteract += OnInteract;
        _inputWrapper.OnInventory += OnInventory;
        _inputWrapper.OnPause += OnPause;

        Enable();
    }

    public void Enable()
    {
        if (_isEnabled) return;

        _inputWrapper.Enable();
        _isEnabled = true;
    }

    public void Disable()
    {
        if (!_isEnabled) return;

        _inputWrapper.Disable();
        _isEnabled = false;
    }

    public Vector2 GetMoveDirection() => _cachedMoveDirection;

    public bool IsInteractPressed() => _inputWrapper.Interact;

    public bool IsInventoryPressed() => _inputWrapper.Inventory;

    public bool IsPausePressed() => _inputWrapper.Pause;

    public bool IsUndoPressed()
    {
        // ��������� ���������� Ctrl+Z ����� ����� Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return false;

        return (keyboard.ctrlKey.isPressed || keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed) 
               && keyboard.zKey.wasPressedThisFrame;
    }

    // ����� �����: ���������, ���� �� ����� ������� �����������
    public bool TryGetDiscreteMove(out Vector2 direction)
    {
        direction = Vector2.zero;

        Vector2 currentInput = _cachedMoveDirection;
        bool isPressed = currentInput.magnitude > 0.1f;

        if (isPressed && !_moveKeyWasPressed)
        {
            // ������ ������� �������
            direction = currentInput;
            _moveKeyWasPressed = true;
            return true;
        }

        if (!isPressed)
        {
            // ������� ��������
            _moveKeyWasPressed = false;
        }

        return false;
    }

    private void OnMoveChanged(Vector2 direction)
    {
        _cachedMoveDirection = direction;
        _eventBus.Publish(new MoveInputEvent(direction));

        // ���������� �������� (������ ��� �������)
        if (TryGetDiscreteMove(out Vector2 discreteDirection))
        {
            //_eventBus.Publish(new DiscreteMoveInputEvent(discreteDirection));
        }
    }

    private void OnInteract()
    {
        _eventBus.Publish(new InteractInputEvent());
    }

    private void OnInventory()
    {
        _eventBus.Publish(new InventoryInputEvent());
    }

    private void OnPause()
    {
        _eventBus.Publish(new PauseInputEvent());
    }

    public void Dispose()
    {
        Disable();
        _inputWrapper.OnMoveChanged -= OnMoveChanged;
        _inputWrapper.OnInteract -= OnInteract;
        _inputWrapper.OnInventory -= OnInventory;
        _inputWrapper.OnPause -= OnPause;
        _inputWrapper.Dispose();
    }
}