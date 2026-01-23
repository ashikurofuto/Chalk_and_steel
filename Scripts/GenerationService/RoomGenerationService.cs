using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Сервис генерации комнат
/// </summary>
public class RoomGenerationService : IRoomGeneratorService
{
    private RoomView roomPrefab;
    private RoomDoublyLinkedList roomList;
    private int maxRooms;
    private System.Random random;
    private System.Func<RoomView, RoomView> _roomFactory;
    private bool _isInitialized = false;
    private int _roomCounter = 0;

    public RoomDoublyLinkedList RoomList => roomList;

    public RoomGenerationService()
    {
        this.roomList = new RoomDoublyLinkedList();
        this.random = new System.Random();
    }

    /// <summary>
    /// Инициализирует генерацию комнат
    /// </summary>
    /// <param name="prefab">Префаб комнаты</param>
    /// <param name="maxRooms">Максимальное количество комнат</param>
    public void InitializeRooms(RoomView prefab, int maxRooms)
    {
        this.roomPrefab = prefab;
        this.maxRooms = maxRooms;

        // Очищаем список комнат
        roomList.Clear();

        // Проверяем, что нужно создавать хотя бы одну комнату
        if (maxRooms <= 0 || prefab == null)
        {
            return;
        }

        // Создаем первую комнату
        RoomView firstRoomView = CreateRoomInstance(prefab);
        if (firstRoomView == null)
        {
            Debug.LogWarning("Failed to create first room instance, stopping initialization");
            return;
        }
        
        RoomNode firstNode = new RoomNode(firstRoomView);
        
        // Добавляем первую комнату в список
        roomList.AddLast(firstNode);
        
        // Устанавливаем первую комнату как текущую
        roomList.SetCurrentNode(firstNode);
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// Генерирует комнату в указанном направлении от текущей комнаты
    /// </summary>
    /// <param name="direction">Направление для генерации новой комнаты</param>
    /// <returns>Новая комната или null, если генерация не удалась</returns>
    public RoomNode GenerateRoomInDirection(DoorDirection direction)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("RoomGenerationService is not initialized, call InitializeRooms first");
            return null;
        }
        
        var currentRoom = roomList.GetCurrentNode();
        if (currentRoom == null)
        {
            Debug.LogWarning("No current room available for generating neighbor");
            return null;
        }
        
        // Проверяем, есть ли уже сосед в этом направлении
        if (currentRoom.HasNeighbor(direction))
        {
            // Возвращаем существующего соседа
            return currentRoom.GetNeighbor(direction);
        }
        
        // Проверяем, достигнут ли лимит комнат
        if (roomList.Count >= maxRooms)
        {
            Debug.LogWarning("Maximum room count reached, cannot generate more rooms");
            return null;
        }
        
        // Создаем новую комнату
        RoomView newRoomView = CreateRoomInstance(roomPrefab);
        if (newRoomView == null)
        {
            Debug.LogWarning("Failed to create room instance, stopping generation");
            return null;
        }
        
        RoomNode newNode = new RoomNode(newRoomView);
        
        // Добавляем соседа к текущей комнате
        currentRoom.AddNeighbor(direction, newNode);
        
        // Добавляем новую комнату в список
        roomList.AddLast(newNode);
        
        return newNode;
    }
    
    /// <summary>
    /// Генерирует комнаты по требованию - создает недостающие комнаты для текущей
    /// </summary>
    public void GenerateRoomsIfNeeded()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("RoomGenerationService is not initialized, call InitializeRooms first");
            return;
        }
        
        var currentRoom = roomList.GetCurrentNode();
        if (currentRoom == null)
        {
            Debug.LogWarning("No current room available for generating neighbors");
            return;
        }
        
        // Генерируем случайное количество соседей для текущей комнаты
        var availableDirections = GetAvailableDirections(currentRoom);
        if (availableDirections.Length == 0)
        {
            return; // Нет доступных направлений
        }
        
        // Определяем, сколько соседей создать
        int remainingRooms = maxRooms - roomList.Count;
        if (remainingRooms <= 0) return; // Достигнут лимит
        
        int possibleNeighbors = Math.Min(availableDirections.Length, remainingRooms);
        int neighborCount = Math.Max(1, Math.Min(possibleNeighbors, random.Next(1, 4))); // 1-3 соседа
        
        for (int i = 0; i < neighborCount && remainingRooms > 0; i++)
        {
            // Выбираем случайное доступное направление
            var availableDirs = GetAvailableDirections(currentRoom);
            if (availableDirs.Length == 0) break;
            
            var randomDirection = availableDirs[random.Next(availableDirs.Length)];
            
            // Генерируем комнату в выбранном направлении
            GenerateRoomInDirection(randomDirection);
            
            remainingRooms = maxRooms - roomList.Count;
        }
    }
    
    /// <summary>
    /// Устанавливает фабрику для создания экземпляров комнат
    /// </summary>
    /// <param name="factory">Фабрика для создания экземпляров комнат</param>
    public void SetRoomFactory(System.Func<RoomView, RoomView> factory)
    {
        _roomFactory = factory;
    }
    
    /// <summary>
    /// Получает все сгенерированные комнаты
    /// </summary>
    /// <returns>Список всех сгенерированных комнат</returns>
    public List<RoomNode> GetAllRooms()
    {
        if (roomList == null)
        {
            Debug.LogWarning("Room list is not initialized, returning empty list");
            return new List<RoomNode>();
        }
        
        return roomList.GetAllNodes();
    }
    
    /// <summary>
    /// Генерирует комнаты в двусвязный список (остался для совместимости, но теперь вызывает InitializeRooms)
    /// </summary>
    /// <param name="prefab">Префаб комнаты</param>
    /// <param name="maxRooms">Максимальное количество комнат</param>
    public void GenerateRooms(RoomView prefab, int maxRooms)
    {
        // Для обратной совместимости вызываем инициализацию
        InitializeRooms(prefab, maxRooms);
    }
    
    /// <summary>
    /// Получает текущую комнату из двусвязного списка
    /// </summary>
    /// <returns>Текущая комната</returns>
    public RoomNode GetCurrentRoom()
    {
        return roomList.GetCurrentNode();
    }
    
    /// <summary>
    /// Получает всех соседей текущей комнаты
    /// </summary>
    /// <returns>Список соседей текущей комнаты</returns>
    public List<RoomNode> GetCurrentRoomNeighbors()
    {
        var currentRoom = roomList.GetCurrentNode();
        if (currentRoom == null)
        {
            return new List<RoomNode>();
        }
        
        return currentRoom.GetAllNeighbors();
    }
    
    /// <summary>
    /// Получает общее количество сгенерированных комнат
    /// </summary>
    /// <returns>Количество комнат</returns>
    public int GetTotalRoomCount()
    {
        return roomList.Count;
    }
    
    /// <summary>
    /// Устанавливает комнату как текущую
    /// </summary>
    /// <param name="room">Комната, которую нужно установить как текущую</param>
    public void SetCurrentRoom(RoomNode room)
    {
        roomList.SetCurrentNode(room);
    }
    
    /// <summary>
    /// Переходит к следующей комнате в списке
    /// </summary>
    /// <returns>Следующая комната или null, если следующей комнаты нет</returns>
    public RoomNode MoveToNextRoom()
    {
        return roomList.MoveToNext();
    }
    
    /// <summary>
    /// Переходит к предыдущей комнате в списке
    /// </summary>
    /// <returns>Предыдущая комната или null, если предыдущей комнаты нет</returns>
    public RoomNode MoveToPreviousRoom()
    {
        return roomList.MoveToPrevious();
    }

    /// <summary>
    /// Создает экземпляр комнаты из префаба
    /// </summary>
    /// <param name="original">Оригинальный префаб комнаты</param>
    /// <returns>Новый экземпляр RoomView</returns>
    private RoomView CreateRoomInstance(RoomView original)
    {
        // Используем фабрику для создания экземпляра комнаты, если она установлена
        RoomView roomView = null;
        if (_roomFactory != null)
        {
            roomView = _roomFactory(original);
        }
        else
        {
            // В противном случае возвращаем null, так как в чистом C# нельзя инстанцировать Unity объекты
            return null;
        }
        
        // Увеличиваем счетчик комнат и устанавливаем имя
        _roomCounter++;
        if (roomView != null && roomView.gameObject != null)
        {
            roomView.gameObject.name = $"Room_{_roomCounter}";
        }
        
        return roomView;
    }

    /// <summary>
    /// Получает доступные направления для соседа
    /// </summary>
    /// <param name="node">Узел, для которого ищутся доступные направления</param>
    /// <returns>Массив доступных направлений</returns>
    private DoorDirection[] GetAvailableDirections(RoomNode node)
    {
        var allDirections = Enum.GetValues(typeof(DoorDirection)) as DoorDirection[];
        var availableDirections = new List<DoorDirection>();

        foreach (var direction in allDirections)
        {
            if (!node.HasNeighbor(direction))
            {
                availableDirections.Add(direction);
            }
        }

        return availableDirections.ToArray();
    }
}