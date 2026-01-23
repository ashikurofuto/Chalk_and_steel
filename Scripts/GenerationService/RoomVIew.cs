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

        switch (direction)
        {
            case DoorDirection.Top:
                // Верхняя сторона: центральная точка по X, максимальная по Y
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.max.y - 1, // -1 чтобы немного внутрь комнаты
                    0);
                break;
                
            case DoorDirection.Bottom:
                // Нижняя сторона: центральная точка по X, минимальная по Y
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.min.y, // Убрали +1, чтобы дверь была на краю
                    0);
                break;
                
            case DoorDirection.Left:
                // Левая сторона: минимальная точка по X, центральная по Y
                doorPosition = new Vector3Int(
                    bounds.min.x, // Убрали +1, чтобы дверь была на краю
                    Mathf.RoundToInt(bounds.center.y),
                    0);
                break;
                
            case DoorDirection.Right:
                // Правая сторона: максимальная точка по X, центральная по Y
                doorPosition = new Vector3Int(
                    bounds.max.x - 1, // -1 чтобы немного внутрь комнаты
                    Mathf.RoundToInt(bounds.center.y),
                    0);
                break;
        }

        // Удаляем стену в этой позиции
        wallTilemap.SetTile(doorPosition, null);
        
        // Устанавливаем дверь в эту позицию
        doorTilemap.SetTile(doorPosition, doorTile);
        
        Debug.Log($"Door set at position {doorPosition} in direction {direction}");
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
        Vector3Int doorPosition = Vector3Int.zero;

        switch (direction)
        {
            case DoorDirection.Top:
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.max.y - 1,
                    0);
                break;
                
            case DoorDirection.Bottom:
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.min.y + 1,
                    0);
                break;
                
            case DoorDirection.Left:
                doorPosition = new Vector3Int(
                    bounds.min.x + 1,
                    Mathf.RoundToInt(bounds.center.y),
                    0);
                break;
                
            case DoorDirection.Right:
                doorPosition = new Vector3Int(
                    bounds.max.x - 1,
                    Mathf.RoundToInt(bounds.center.y),
                    0);
                break;
        }

        // Удаляем дверь
        doorTilemap.SetTile(doorPosition, null);
        
        // Восстанавливаем стену
        wallTilemap.SetTile(doorPosition, wallTile);
        
        Debug.Log($"Door removed at position {doorPosition} in direction {direction}, wall restored");
    }
}