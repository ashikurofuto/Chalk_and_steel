using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomView : MonoBehaviour
{
    public enum Direction
    {
        Top,
        Bottom,
        Left,
        Right
    }

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
    public void SetDoor(Direction direction)
    {
        // Получаем границы текущего тайлмапа стен
        BoundsInt bounds = wallTilemap.cellBounds;
        
        // Вычисляем центральные координаты для каждой стороны
        Vector3Int doorPosition = Vector3Int.zero;

        switch (direction)
        {
            case Direction.Top:
                // Верхняя сторона: центральная точка по X, максимальная по Y
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.max.y - 1, // -1 чтобы немного внутрь комнаты
                    0);
                break;
                
            case Direction.Bottom:
                // Нижняя сторона: центральная точка по X, минимальная по Y
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.min.y + 1, // +1 чтобы немного внутрь комнаты
                    0);
                break;
                
            case Direction.Left:
                // Левая сторона: минимальная точка по X, центральная по Y
                doorPosition = new Vector3Int(
                    bounds.min.x + 1, // +1 чтобы немного внутрь комнаты
                    Mathf.RoundToInt(bounds.center.y),
                    0);
                break;
                
            case Direction.Right:
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
    }
    
    /// <summary>
    /// Удаляет дверь из указанной стороны и восстанавливает стену
    /// </summary>
    /// <param name="direction">Сторона, где нужно удалить дверь</param>
    public void RemoveDoor(Direction direction)
    {
        // Получаем границы текущего тайлмапа стен
        BoundsInt bounds = wallTilemap.cellBounds;
        
        // Вычисляем ту же позицию, где была дверь
        Vector3Int doorPosition = Vector3Int.zero;

        switch (direction)
        {
            case Direction.Top:
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.max.y - 1,
                    0);
                break;
                
            case Direction.Bottom:
                doorPosition = new Vector3Int(
                    Mathf.RoundToInt(bounds.center.x),
                    bounds.min.y + 1,
                    0);
                break;
                
            case Direction.Left:
                doorPosition = new Vector3Int(
                    bounds.min.x + 1,
                    Mathf.RoundToInt(bounds.center.y),
                    0);
                break;
                
            case Direction.Right:
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
    }
}