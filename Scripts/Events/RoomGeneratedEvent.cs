using UnityEngine;

/// <summary>
/// Событие генерации комнаты, содержащее информацию о месте появления игрока и ссылку на RoomView
/// </summary>
public record RoomGeneratedEvent
{
    public Vector3 SpawnPosition { get; }
    public RoomView RoomView { get; }
    public Vector3Int GridPosition { get; }

    public RoomGeneratedEvent(Vector3 spawnPosition, RoomView roomView, Vector3Int gridPosition = default)
    {
        SpawnPosition = spawnPosition;
        RoomView = roomView;
        GridPosition = gridPosition;
    }
}