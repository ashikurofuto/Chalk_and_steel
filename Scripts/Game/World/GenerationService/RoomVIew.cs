using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomView : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap doorTilemap;

    [Header("Tiles")]
    public TileBase wallTile;
    public TileBase doorTile;

    /// <summary>
    /// Устанавливает дверь в указанной стороне комнаты
    /// </summary>
    /// <param name="direction">Сторона, где должна быть установлена дверь</param>
    public void SetDoor(DoorDirection direction)
    {
        if (wallTilemap == null || doorTile == null)
        {
            Debug.LogWarning("Wall tilemap or door tile is not assigned");
            return;
        }
        
        // Получаем границы текущего тайлмапа стен
        BoundsInt bounds = wallTilemap.cellBounds;
        
        // Проверяем, что границы тайлмапа корректны
        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogWarning("Wall tilemap bounds are invalid, cannot set door");
            return;
        }
        
        // Вычисляем центральные координаты для каждой стороны
        Vector3Int doorPosition = Vector3Int.zero;

        // Устанавливаем дверь на фиксированное расстояние от центра (5 единиц)
        doorPosition = GetFixedDoorPosition(direction, bounds);

        // Удаляем стену в этой позиции
        wallTilemap.SetTile(doorPosition, null);

        // Устанавливаем дверь в эту позицию
        doorTilemap.SetTile(doorPosition, doorTile);

        // Debug.Log($"Door set at position {doorPosition} in direction {direction}, wall removed"); // Закомментировано для уменьшения логов
    }

    /// <summary>
    /// Возвращает фиксированную позицию двери на расстоянии 5 от центра комнаты
    /// </summary>
    /// <param name="direction">Направление двери</param>
    /// <param name="bounds">Границы тайлмапа</param>
    /// <returns>Позиция для двери</returns>
    private Vector3Int GetFixedDoorPosition(DoorDirection direction, BoundsInt bounds)
    {
        Vector3Int doorPosition = Vector3Int.zero;
        int distanceFromCenter = 5; // Фиксированное расстояние от центра

        // Вычисляем центр комнаты
        Vector3Int center = new Vector3Int(
            Mathf.RoundToInt(bounds.center.x),
            Mathf.RoundToInt(bounds.center.y),
            0
        );

        switch (direction)
        {
            case DoorDirection.Top:
                // Верхняя дверь: на 5 единиц выше центра
                doorPosition = new Vector3Int(center.x, center.y + distanceFromCenter, 0);
                break;

            case DoorDirection.Bottom:
                // Нижняя дверь: на 5 единиц ниже центра
                doorPosition = new Vector3Int(center.x, center.y - distanceFromCenter, 0);
                break;

            case DoorDirection.Left:
                // Левая дверь: на 5 единиц левее центра
                doorPosition = new Vector3Int(center.x - distanceFromCenter, center.y, 0);
                break;

            case DoorDirection.Right:
                // Правая дверь: на 5 единиц правее центра
                doorPosition = new Vector3Int(center.x + distanceFromCenter, center.y, 0);
                break;
        }

        return doorPosition;
    }

    /// <summary>
    /// Удаляет дверь из указанной стороны и восстанавливает стену
    /// </summary>
    /// <param name="direction">Сторона, где нужно удалить дверь</param>
    public void RemoveDoor(DoorDirection direction)
    {
        if (wallTilemap == null || wallTile == null)
        {
            Debug.LogWarning("Wall tilemap or wall tile is not assigned");
            return;
        }
        
        // Получаем границы текущего тайлмапа стен
        BoundsInt bounds = wallTilemap.cellBounds;
        
        // Проверяем, что границы тайлмапа корректны
        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogWarning("Wall tilemap bounds are invalid, cannot remove door");
            return;
        }
        
        // Вычисляем ту же позицию, где была дверь
        Vector3Int doorPosition = GetFixedDoorPosition(direction, bounds);

        // Удаляем дверь
        doorTilemap.SetTile(doorPosition, null);

        // Восстанавливаем стену
        wallTilemap.SetTile(doorPosition, wallTile);

        // Debug.Log($"Door removed at position {doorPosition} in direction {direction}, wall restored"); // Закомментировано для уменьшения логов
    }
}