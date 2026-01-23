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
    /// Генерирует все комнаты сразу
    /// </summary>
    /// <param name="prefab">Префаб комнаты</param>
    /// <param name="maxRooms">Максимальное количество комнат</param>
    public void GenerateAllRoomsAtOnce(RoomView prefab, int maxRooms)
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
            Debug.LogWarning("Failed to create first room instance, stopping generation");
            return;
        }
        
        RoomNode firstNode = new RoomNode(firstRoomView);
        
        // Добавляем первую комнату в список
        roomList.AddLast(firstNode);
        
        // Устанавливаем первую комнату как текущую
        roomList.SetCurrentNode(firstNode);
        
        // Создаем оставшиеся комнаты и соединяем их
        int remainingRooms = maxRooms - 1;
        var allNodes = new List<RoomNode> { firstNode }; // Список всех созданных комнат для дальнейшего соединения
        
        // Создаем все оставшиеся комнаты
        while (remainingRooms > 0 && allNodes.Count < maxRooms)
        {
            RoomView newRoomView = CreateRoomInstance(prefab);
            if (newRoomView == null)
            {
                Debug.LogWarning("Failed to create room instance, stopping generation");
                break;
            }
            
            RoomNode newNode = new RoomNode(newRoomView);
            allNodes.Add(newNode);
            
            // Добавляем новую комнату в список
            roomList.AddLast(newNode);
            
            remainingRooms--;
        }
        
        // Теперь соединяем комнаты между собой, чтобы образовать сеть
        ConnectRoomsInNetwork(allNodes);
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// Соединяет комнаты в сеть с учетом требований:
    /// 1. Избегаем прямой связи от первой к последней комнате
    /// 2. Последняя комната соединяется только с предпоследней
    /// </summary>
    /// <param name="nodes">Список комнат для соединения</param>
    private void ConnectRoomsInNetwork(List<RoomNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return;
        }

        var random = new System.Random();

        // Создаем основную цепочку комнат, соединяя каждую с последующей
        // Это создаст основной путь через подземелье
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            var currentRoom = nodes[i];
            var nextRoom = nodes[i + 1];

            var availableDirections = GetAvailableDirections(currentRoom);
            if (availableDirections.Length > 0)
            {
                // Используем случайное доступное направление
                var direction = availableDirections[random.Next(availableDirections.Length)];
                currentRoom.AddNeighbor(direction, nextRoom);
            }
        }

        // Теперь добавляем дополнительные соединения, избегая прямой связи от первой к последней
        for (int i = 0; i < nodes.Count; i++)
        {
            var currentRoom = nodes[i];

            // Определяем, сколько дополнительных соединений создать
            // Уменьшаем вероятность для первой и последней комнат, чтобы избежать прямой связи
            int maxAdditionalConnections = random.Next(0, 2); // 0-1 дополнительных соединений

            // Для первой комнаты ограничиваем соединения, чтобы избежать прямой связи с последней
            if (i == 0)
            {
                maxAdditionalConnections = random.Next(0, 1); // 0 или 1 дополнительное соединение
            }

            // Для последней комнаты не создаем дополнительных соединений, кроме связи с предпоследней
            if (i == nodes.Count - 1)
            {
                continue; // Пропускаем последнюю комнату
            }

            // Создаем дополнительные соединения
            for (int j = 0; j < maxAdditionalConnections; j++)
            {
                // Выбираем случайную комнату для соединения
                // Исключаем: текущую комнату, первую комнату (если текущая - не вторая), последнюю комнату
                int randomIndex;

                // Повторяем, пока не найдем подходящую комнату для соединения
                do
                {
                    randomIndex = random.Next(nodes.Count);

                    // Не соединяем с самим собой
                    if (randomIndex == i) continue;

                    // Не соединяем первую комнату напрямую с последней
                    if ((i == 0 && randomIndex == nodes.Count - 1) ||
                        (randomIndex == 0 && i == nodes.Count - 1)) continue;

                    // Не соединяем с соседом по основному пути (чтобы избежать циклов)
                    if ((i < nodes.Count - 1 && randomIndex == i + 1) ||
                        (i > 0 && randomIndex == i - 1)) continue;

                    // Проверяем, есть ли уже соединение
                    if (currentRoom.HasNeighborInAnyDirection(nodes[randomIndex])) continue;

                    // Проверяем, достигнут ли лимит соседей для текущей комнаты
                    if (currentRoom.IsMaxNeighborsReached()) break;

                    break; // Найдена подходящая комната

                } while (true);

                // Проверяем, достигнут ли лимит соседей
                if (currentRoom.IsMaxNeighborsReached()) break;

                // Проверяем, что нашли подходящую комнату
                if (randomIndex >= 0 && randomIndex < nodes.Count &&
                    randomIndex != i &&
                    !(i == 0 && randomIndex == nodes.Count - 1) &&
                    !currentRoom.HasNeighborInAnyDirection(nodes[randomIndex]))
                {
                    var targetRoom = nodes[randomIndex];

                    var availableDirections = GetAvailableDirections(currentRoom);
                    if (availableDirections.Length > 0)
                    {
                        var direction = availableDirections[random.Next(availableDirections.Length)];
                        currentRoom.AddNeighbor(direction, targetRoom);
                    }
                }
            }
        }

        // Убедимся, что последняя комната соединена только с предпоследней
        if (nodes.Count > 1)
        {
            var lastRoom = nodes[nodes.Count - 1];
            var prevLastRoom = nodes[nodes.Count - 2];

            // Удаляем все соединения у последней комнаты, кроме соединения с предпоследней
            var directionsToRemove = new List<DoorDirection>();
            var allDirections = Enum.GetValues(typeof(DoorDirection)) as DoorDirection[];

            foreach (var direction in allDirections)
            {
                if (lastRoom.HasNeighbor(direction))
                {
                    var neighbor = lastRoom.GetNeighbor(direction);
                    if (neighbor != prevLastRoom)
                    {
                        directionsToRemove.Add(direction);
                    }
                }
            }

            // Удаляем лишние соединения
            foreach (var direction in directionsToRemove)
            {
                lastRoom.RemoveNeighbor(direction);
            }

            // Убедимся, что соединение с предпоследней комнатой существует
            if (!lastRoom.HasNeighborInAnyDirection(prevLastRoom))
            {
                var availableDirections = GetAvailableDirections(lastRoom);
                if (availableDirections.Length > 0)
                {
                    var direction = availableDirections[0]; // Используем первое доступное направление
                    lastRoom.AddNeighbor(direction, prevLastRoom);
                }
            }
        }
    }
    
    /// <summary>
    /// Находит направление соединения между двумя комнатами
    /// </summary>
    /// <param name="fromRoom">Комната-источник</param>
    /// <param name="toRoom">Комната-назначение</param>
    /// <returns>Направление соединения или null, если комнаты не соединены</returns>
    private DoorDirection? GetConnectionDirection(RoomNode fromRoom, RoomNode toRoom)
    {
        var allDirections = Enum.GetValues(typeof(DoorDirection)) as DoorDirection[];
        foreach (var direction in allDirections)
        {
            if (fromRoom.HasNeighbor(direction) && fromRoom.GetNeighbor(direction) == toRoom)
            {
                return direction;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Деактивирует все комнаты, кроме первой
    /// </summary>
    /// <param name="nodes">Список всех комнат</param>
    private void DeactivateAllRoomsExceptFirst(List<RoomNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return;
        }
        
        for (int i = 1; i < nodes.Count; i++) // Начинаем с 1, чтобы оставить первую комнату активной
        {
            var roomView = nodes[i].Room;
            if (roomView != null && roomView.gameObject != null)
            {
                roomView.gameObject.SetActive(false);
            }
        }
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