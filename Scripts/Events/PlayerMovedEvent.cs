using UnityEngine;

/// Событие перемещения игрока в хабе.
/// Публикуется при успешном перемещении на соседнюю клетку.
/// </summary>
public record PlayerMovedEvent
{
    public Vector3Int PreviousPosition { get; }
    public Vector3Int NewPosition { get; }

    public PlayerMovedEvent(Vector3Int previousPosition, Vector3Int newPosition)
    {
        PreviousPosition = previousPosition;
        NewPosition = newPosition;
    }
}