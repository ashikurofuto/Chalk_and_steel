using System;
using UnityEngine;

public interface IInputActionsWrapper : IDisposable
{
    /// <summary>
    /// Вектор движения игрока (нормализованный).
    /// </summary>
    Vector2 Move { get; }

    /// <summary>
    /// Состояние кнопки взаимодействия.
    /// </summary>
    bool Interact { get; }

    /// <summary>
    /// Состояние кнопки инвентаря.
    /// </summary>
    bool Inventory { get; }

    /// <summary>
    /// Состояние кнопки паузы.
    /// </summary>
    bool Pause { get; }

    /// <summary>
    /// Событие изменения движения.
    /// </summary>
    event Action<Vector2> OnMoveChanged;

    /// <summary>
    /// Событие нажатия кнопки взаимодействия.
    /// </summary>
    event Action OnInteract;

    /// <summary>
    /// Событие нажатия кнопки инвентаря.
    /// </summary>
    event Action OnInventory;

    /// <summary>
    /// Событие нажатия кнопки паузы.
    /// </summary>
    event Action OnPause;

    /// <summary>
    /// Включает систему ввода.
    /// </summary>
    void Enable();

    /// <summary>
    /// Выключает систему ввода.
    /// </summary>
    void Disable();
}