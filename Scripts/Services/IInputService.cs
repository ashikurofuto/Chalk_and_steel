using System;
using UnityEngine;
/// <summary>
/// ��������� ������� �����.
/// ������������� ������ � �������� ����� � ���������� �������� �����.
/// </summary>
public interface IInputService
{
    /// <summary>
    /// �������� ������� �����.
    /// </summary>
    void Enable();

    /// <summary>
    /// ��������� ������� �����.
    /// </summary>
    void Disable();

    /// <summary>
    /// ���������� ������� ��������������� ������ ��������.
    /// </summary>
    Vector2 GetMoveDirection();

    /// <summary>
    /// ���������, ���� �� ������ ������ �������������� � ������� �����.
    /// </summary>
    bool IsInteractPressed();

    /// <summary>
    /// ���������, ���� �� ������ ������ ��������� � ������� �����.
    /// </summary>
    bool IsInventoryPressed();

    /// <summary>
    /// ���������, ���� �� ������ ������ ����� � ������� �����.
    /// </summary>
    bool IsPausePressed();

    /// <summary>
    /// ���������, ���� �� ������ ������ ��������� (Ctrl+Z) � ������� �����.
    /// </summary>
    bool IsUndoPressed();
}
