using ChalkAndSteel.Services;
using UnityEngine;

/// <summary>
/// Данные сгенерированной комнаты для передачи между системами.
/// </summary>
public record RoomData
{
    public GameObject RoomObject { get; }
    public Vector3Int GridPosition { get; }
    public RoomType RoomType { get; }
    public DoorDirections Doors { get; }

    public RoomData(GameObject roomObject, Vector3Int gridPosition, RoomType roomType, DoorDirections doors)
    {
        RoomObject = roomObject;
        GridPosition = gridPosition;
        RoomType = roomType;
        Doors = doors;
    }
}
