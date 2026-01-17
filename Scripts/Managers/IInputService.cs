using System;
using UnityEngine;
/// <summary>
/// Интерфейс сервиса ввода.
/// Предоставляет доступ к игровому вводу и управлению системой ввода.
/// </summary>
public interface IInputService
{
    /// <summary>
    /// Включает систему ввода.
    /// </summary>
    void Enable();

    /// <summary>
    /// Выключает систему ввода.
    /// </summary>
    void Disable();

    /// <summary>
    /// Возвращает текущий нормализованный вектор движения.
    /// </summary>
    Vector2 GetMoveDirection();

    /// <summary>
    /// Проверяет, была ли нажата кнопка взаимодействия в текущем кадре.
    /// </summary>
    bool IsInteractPressed();

    /// <summary>
    /// Проверяет, была ли нажата кнопка инвентаря в текущем кадре.
    /// </summary>
    bool IsInventoryPressed();

    /// <summary>
    /// Проверяет, была ли нажата кнопка паузы в текущем кадре.
    /// </summary>
    bool IsPausePressed();
}
