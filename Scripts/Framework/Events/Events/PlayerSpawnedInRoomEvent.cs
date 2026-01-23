using UnityEngine;

/// <summary>
/// Событие появления игрока в комнате после завершения генерации.
/// Публикуется, когда генерация комнаты завершена и игрок появляется в начальных координатах.
/// </summary>
public record PlayerSpawnedInRoomEvent
{
    public Vector3 Position { get; }
    public int RoomId { get; }
    public Vector3Int GridPosition { get; }

    public PlayerSpawnedInRoomEvent(Vector3 position, int roomId, Vector3Int gridPosition)
    {
        Position = position;
        RoomId = roomId;
        GridPosition = gridPosition;
    }
}