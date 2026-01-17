using Architecture.GlobalModules;
using Architecture.Services;
using UnityEngine;

public sealed class InputService : IInputService
{
    // 1. Зависимости - приватные поля readonly
    private readonly IEventBus _eventBus;
    private readonly IInputActionsWrapper _inputWrapper;

    // 2. Внутреннее состояние
    private Vector2 _cachedMoveDirection;
    private bool _isEnabled;

    // 3. Конструктор для инъекции зависимостей
    public InputService(
        IEventBus eventBus,
        IInputActionsWrapper inputWrapper)
    {
        // Валидация входных данных

        _eventBus = eventBus;
        _inputWrapper = inputWrapper;

        // 4. Подписка на события ввода
        _inputWrapper.OnMoveChanged += OnMoveChanged;
        _inputWrapper.OnInteract += OnInteract;
        _inputWrapper.OnInventory += OnInventory;
        _inputWrapper.OnPause += OnPause;

        // Включаем ввод по умолчанию
        Enable();
    }

    // 5. Публичные методы интерфейса
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

    // 6. Приватные методы-обработчики событий ввода
    private void OnMoveChanged(Vector2 direction)
    {
        _cachedMoveDirection = direction;

        // Публикация события через EventBus
        _eventBus.Publish(new MoveInputEvent(direction));
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

    // 7. Очистка ресурсов
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