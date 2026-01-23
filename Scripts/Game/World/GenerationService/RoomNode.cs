using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Узел комнаты, содержащий ссылку на саму комнату и её соседей
/// </summary>
public class RoomNode
{
    /// <summary>
    /// Максимальное количество соседей
    /// </summary>
    private const int MAX_NEIGHBORS = 3;
    
    /// <summary>
    /// Комнаты, представленная этим узлом
    /// </summary>
    public RoomView Room { get; private set; }
    
    /// <summary>
    /// Соседние комнаты с указанием направления
    /// </summary>
    public Dictionary<DoorDirection, RoomNode> Neighbors { get; private set; }
    
    /// <summary>
    /// Предыдущий узел в последовательности
    /// </summary>
    public RoomNode Previous { get; set; }
    
    /// <summary>
    /// Следующий узел в последовательности
    /// </summary>
    public RoomNode Next { get; set; }
    
    /// <summary>
    /// Конструктор узла комнаты
    /// </summary>
    /// <param name="room">Комната, которую представляет этот узел</param>
    public RoomNode(RoomView room)
    {
        Room = room;
        Neighbors = new Dictionary<DoorDirection, RoomNode>();
        Previous = null;
        Next = null;
    }
    
    /// <summary>
    /// Добавляет соседнюю комнату в указанном направлении
    /// </summary>
    /// <param name="direction">Направление, в котором находится сосед</param>
    /// <param name="neighbor">Соседний узел комнаты</param>
    /// <param name="setReverseConnection">Указывает, нужно ли устанавливать обратную связь</param>
    /// <returns>True, если сосед был успешно добавлен, иначе false</returns>
    public bool AddNeighbor(DoorDirection direction, RoomNode neighbor, bool setReverseConnection = true)
    {
        // Проверяем, достигнут ли лимит соседей
        if (Neighbors.Count >= MAX_NEIGHBORS)
        {
            // Лимит соседей достигнут
            return false;
        }
        
        // Проверяем, существует ли уже сосед в этом направлении
        if (Neighbors.ContainsKey(direction))
        {
            // Заменяем существующего соседа
            Neighbors[direction] = neighbor;
        }
        else
        {
            // Добавляем нового соседа
            Neighbors.Add(direction, neighbor);
        }
        
        // Проверяем, что Room не равен null перед вызовом SetDoor
        if (Room != null)
        {
            Room.SetDoor(direction);
        }
        
        // Устанавливаем обратную связь, если требуется
        if (setReverseConnection && neighbor != null)
        {
            var oppositeDirection = GetOppositeDirection(direction);
            // Убеждаемся, что обратная связь не создаст зацикливания
            if (!neighbor.HasNeighbor(oppositeDirection))
            {
                neighbor.AddNeighbor(oppositeDirection, this, false); // false чтобы избежать бесконечной рекурсии
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Получает противоположное направление
    /// </summary>
    /// <param name="direction">Исходное направление</param>
    /// <returns>Противоположное направление</returns>
    private DoorDirection GetOppositeDirection(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.Top:
                return DoorDirection.Bottom;
            case DoorDirection.Bottom:
                return DoorDirection.Top;
            case DoorDirection.Left:
                return DoorDirection.Right;
            case DoorDirection.Right:
                return DoorDirection.Left;
            default:
                return direction;
        }
    }
    
    /// <summary>
    /// Удаляет соседа в указанном направлении
    /// </summary>
    /// <param name="direction">Направление для удаления соседа</param>
    public void RemoveNeighbor(DoorDirection direction)
    {
        if (Neighbors.ContainsKey(direction))
        {
            var neighbor = Neighbors[direction];
            Neighbors.Remove(direction);
            
            // Убираем дверь из комнаты в указанном направлении
            if (Room != null)
            {
                Room.RemoveDoor(direction);
            }
            
            // Удаляем обратную связь у соседа
            var oppositeDirection = GetOppositeDirection(direction);
            if (neighbor != null && neighbor.HasNeighbor(oppositeDirection))
            {
                neighbor.RemoveNeighbor(oppositeDirection);
            }
        }
    }
    
    /// <summary>
    /// Проверяет, есть ли сосед в указанном направлении
    /// </summary>
    /// <param name="direction">Направление для проверки</param>
    /// <returns>True, если есть сосед в указанном направлении, иначе False</returns>
    public bool HasNeighbor(DoorDirection direction)
    {
        return Neighbors.ContainsKey(direction);
    }
    
    /// <summary>
    /// Получает соседа в указанном направлении
    /// </summary>
    /// <param name="direction">Направление для получения соседа</param>
    /// <returns>Соседний узел комнаты или null, если соседа нет</returns>
    public RoomNode GetNeighbor(DoorDirection direction)
    {
        if (Neighbors.TryGetValue(direction, out RoomNode neighbor))
        {
            return neighbor;
        }
        return null;
    }
    
    /// <summary>
    /// Устанавливает случайного соседа как следующий узел
    /// </summary>
    /// <returns>True, если удалось выбрать случайного соседа, иначе False</returns>
    public bool SetRandomNeighborAsNext()
    {
        if (Neighbors.Count == 0)
        {
            return false;
        }
        
        var random = new Random();
        var neighborsList = Neighbors.Values.ToList();
        var randomIndex = random.Next(0, neighborsList.Count);
        
        var selectedNeighbor = neighborsList[randomIndex];
        
        // Устанавливаем связи
        this.Next = selectedNeighbor;
        selectedNeighbor.Previous = this;
        
        return true;
    }
    
    /// <summary>
    /// Получает список всех соседей
    /// </summary>
    /// <returns>Список соседей</returns>
    public List<RoomNode> GetAllNeighbors()
    {
        return Neighbors.Values.ToList();
    }
    
    /// <summary>
    /// Получает количество соседей
    /// </summary>
    /// <returns>Количество соседей</returns>
    public int GetNeighborsCount()
    {
        return Neighbors.Count;
    }
    
    /// <summary>
    /// Проверяет, достигнут ли лимит соседей
    /// </summary>
    /// <returns>True, если достигнут лимит соседей, иначе false</returns>
    public bool IsMaxNeighborsReached()
    {
        return Neighbors.Count >= MAX_NEIGHBORS;
    }

    /// <summary>
    /// Проверяет, является ли указанная комната соседом в любом направлении
    /// </summary>
    /// <param name="roomNode">Комната для проверки</param>
    /// <returns>True, если комната является соседом, иначе false</returns>
    public bool HasNeighborInAnyDirection(RoomNode roomNode)
    {
        foreach (var kvp in Neighbors)
        {
            if (kvp.Value == roomNode)
            {
                return true;
            }
        }
        
        return false;
    }

}