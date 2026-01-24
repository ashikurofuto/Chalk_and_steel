using Architecture.GlobalModules;
using UnityEngine;
using VContainer;

/// <summary>
/// Обработчик генерации комнат
/// </summary>
public class RoomGenerationHandler : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Transform containerTransform;
    
    // Поле для инъекции сервиса (в реальной реализации через VContainer)
    private IRoomGeneratorService roomGeneratorService;
    private IEventBus _eventBus;
    private RoomView _currentRoomView;
    private DoorDirection? _entryDirection = null; // Направление, откуда вошли в текущую комнату
    

    // === Внедрение зависимостей ===
    [Inject]
    public void Construct(
        IRoomGeneratorService generatorService,
        IEventBus eventBus)
    {
        roomGeneratorService = generatorService;
        _eventBus = eventBus;
        
        // Устанавливаем фабрику для создания экземпляров комнат
        roomGeneratorService.SetRoomFactory(CreateRoomInstance);
    }

    void Start()
    {
        // Проверяем, что все необходимые компоненты установлены
        if (roomGeneratorService == null)
        {
            Debug.LogError("IRoomGeneratorService не был внедрен в RoomGenerationHandler");
            return;
        }
        
        if (roomPrefab == null)
        {
            Debug.LogError("RoomPrefab не назначен в RoomGenerationHandler");
            return;
        }
        
        // Получаем компонент RoomView из префаба
        RoomView roomViewComponent = roomPrefab.GetComponent<RoomView>();
        if (roomViewComponent == null)
        {
            Debug.LogError("RoomPrefab не содержит компонент RoomView");
            return;
        }
        
        // Генерируем все комнаты сразу
        roomGeneratorService.GenerateAllRoomsAtOnce(roomViewComponent, 10); // Пример: 10 комнат

        // Инициализируем первую комнату с центральным спауном
        InitializeFirstRoom();
    }
    
    /// <summary>
    /// Создает экземпляр комнаты из префаба
    /// </summary>
    /// <param name="original">Оригинальный префаб комнаты</param>
    /// <returns>Новый экземпляр RoomView</returns>
    public RoomView CreateRoomInstance(RoomView original)
    {
        if (original == null)
        {
            Debug.LogError("Cannot create room instance from null prefab");
            return null;
        }
        
        // В Unity среде это будет инстанцирование префаба
        GameObject roomGameObject = Instantiate(original.gameObject, transform);
        RoomView roomView = roomGameObject.GetComponent<RoomView>();
        
        return roomView;
    }
    
    /// <summary>
    /// Загружает текущую комнату
    /// </summary>
    public void LoadCurrentRoom()
    {
        if (roomGeneratorService == null)
        {
            Debug.LogError("IRoomGeneratorService не инициализирован");
            return;
        }
        
        // Деактивируем предыдущую комнату, если она существует
        if (_currentRoomView != null)
        {
            _currentRoomView.gameObject.SetActive(false);
        }
        
        // Получаем текущую комнату из сервиса
        var currentRoomNode = roomGeneratorService.GetCurrentRoom();
        
        if (currentRoomNode != null)
        {
            // Используем RoomView из узла комнаты, который уже должен быть инстанцирован
            var roomView = currentRoomNode.Room;
            
            if (roomView != null)
            {
                // Проверяем, что тайлмапы инициализированы
                if (roomView.floorTilemap != null && roomView.wallTilemap != null)
                {
                    // Устанавливаем комнату как дочерний объект для правильного управления
                    if (containerTransform != null)
                    {
                        roomView.transform.SetParent(containerTransform);
                    }
                    else
                    {
                        roomView.transform.SetParent(transform);
                    }
                    
                    // Деактивируем все дочерние комнаты перед активацией новой
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        // Пропускаем новую комнату, чтобы не деактивировать её
                        if (child.gameObject != roomView.gameObject)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                    
                    roomView.gameObject.SetActive(true);
                    
                    // Сохраняем ссылку на текущую комнату
                    _currentRoomView = roomView;
                    
                    // Генерируем соседей для текущей комнаты по требованию
                    (roomGeneratorService as RoomGenerationService)?.GenerateRoomsIfNeeded();
                    
                    // Здесь можно передать данные из модели комнаты в представление
                    // В реальной реализации может потребоваться более сложная логика синхронизации
                    
                    // Публикуем событие о сгенерированной комнате с информацией о месте появления
                    // Если есть направление входа, используем его для позиции появления, иначе центр комнаты
                    Vector3 spawnPosition = CalculateSpawnPosition(roomView, _entryDirection);
                    Vector3Int gridPosition = new Vector3Int(Mathf.RoundToInt(spawnPosition.x), Mathf.RoundToInt(spawnPosition.y), 0);

                    var roomGeneratedEvent = new RoomGeneratedEvent(spawnPosition, roomView, gridPosition);

                    // Проверяем, что EventBus инициализирован перед публикацией события
                    if (_eventBus != null)
                    {
                        _eventBus.Publish(roomGeneratedEvent);
                        // Debug.Log($"Published RoomGeneratedEvent: SpawnPosition={spawnPosition}, RoomView={roomView}, EntryDirection={_entryDirection?.ToString() ?? "center"}"); // Закомментировано для уменьшения логов
                    }
                    else
                    {
                        Debug.LogWarning("EventBus is not initialized, cannot publish RoomGeneratedEvent");
                    }

                    // Сбрасываем направление входа после использования
                    _entryDirection = null;
                }
                else
                {
                    Debug.LogWarning("RoomView has null tilemaps, cannot calculate spawn position and generate event");
                }
            }
        }
    }
    
    /// <summary>
    /// Рассчитывает позицию появления игрока в комнате
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <param name="entryDirection">Направление входа в комнату (если null, используется центр комнаты)</param>
    /// <returns>Позиция появления игрока</returns>
    private Vector3 CalculateSpawnPosition(RoomView roomView, DoorDirection? entryDirection = null)
    {
        // Проверяем, что тайлмапы инициализированы
        if (roomView.floorTilemap == null)
        {
            Debug.LogWarning("Floor tilemap is not assigned, returning default position");
            return Vector3.zero;
        }

        // Если указано направление входа, вычисляем позицию у двери
        if (entryDirection.HasValue)
        {
            Vector3Int doorPosition = GetDoorPositionNearEdge(roomView, entryDirection.Value);

            // Проверяем, есть ли пол в позиции двери (это место двери в текущей комнате)
            if (roomView.floorTilemap.HasTile(doorPosition))
            {
                return roomView.floorTilemap.GetCellCenterWorld(doorPosition);
            }
            else
            {
                // Если в позиции двери нет пола, ищем ближайшую клетку с полом
                Vector3Int adjustedPosition = FindNearestFloorTile(roomView, doorPosition);
                return roomView.floorTilemap.GetCellCenterWorld(adjustedPosition);
            }
        }
        else
        {
            // Если направление входа не указано (например, для первой комнаты), используем центр
            return CalculateCenterSpawnPosition(roomView);
        }
    }

    /// <summary>
    /// Рассчитывает позицию появления игрока в центре комнаты
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <returns>Позиция появления игрока в центре комнаты</returns>
    private Vector3 CalculateCenterSpawnPosition(RoomView roomView)
    {
        // Получаем границы тайлмапа пола
        var bounds = roomView.floorTilemap.cellBounds;

        // Рассчитываем центр комнаты
        float centerX = bounds.center.x;
        float centerY = bounds.center.y;

        // Получаем центральную ячейку
        Vector3Int centerCell = new Vector3Int(Mathf.RoundToInt(centerX), Mathf.RoundToInt(centerY), 0);

        // Проверяем, есть ли пол в центральной ячейке, если нет - ищем ближайшую ячейку с полом
        if (roomView.floorTilemap.HasTile(centerCell))
        {
            return roomView.floorTilemap.GetCellCenterWorld(centerCell);
        }
        else
        {
            // Если в центре нет пола, ищем ближайшую ячейку с полом
            for (int radius = 1; radius < 10; radius++) // ограничиваем радиус поиска
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius) // только по периметру
                        {
                            Vector3Int testCell = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);
                            if (roomView.floorTilemap.HasTile(testCell))
                            {
                                return roomView.floorTilemap.GetCellCenterWorld(testCell);
                            }
                        }
                    }
                }
            }

            // Если не нашли пол, возвращаем центральную ячейку
            return roomView.floorTilemap.GetCellCenterWorld(centerCell);
        }
    }

    /// <summary>
    /// Вычисляет позицию двери у края комнаты в заданном направлении
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <param name="direction">Направление двери</param>
    /// <returns>Позиция двери в тайлмапе</returns>
    private Vector3Int GetDoorPositionNearEdge(RoomView roomView, DoorDirection direction)
    {
        var bounds = roomView.floorTilemap.cellBounds;

        switch (direction)
        {
            case DoorDirection.Top:
                // Верхняя сторона: центральная точка по X, чуть ниже верхней границы
                // Ищем реальную позицию двери в doorTilemap
                return FindActualDoorPosition(roomView, DoorDirection.Top);

            case DoorDirection.Bottom:
                // Нижняя сторона: центральная точка по X, чуть выше нижней границы
                return FindActualDoorPosition(roomView, DoorDirection.Bottom);

            case DoorDirection.Left:
                // Левая сторона: чуть правее левой границы, центральная по Y
                return FindActualDoorPosition(roomView, DoorDirection.Left);

            case DoorDirection.Right:
                // Правая сторона: чуть левее правой границы, центральная по Y
                return FindActualDoorPosition(roomView, DoorDirection.Right);

            default:
                // По умолчанию возвращаем центр
                return new Vector3Int(Mathf.RoundToInt(bounds.center.x), Mathf.RoundToInt(bounds.center.y), 0);
        }
    }

    /// <summary>
    /// Находит фактическую позицию двери в комнате
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <param name="direction">Направление, где искать дверь</param>
    /// <returns>Позиция двери в тайлмапе</returns>
    private Vector3Int FindActualDoorPosition(RoomView roomView, DoorDirection direction)
    {
        // Вычисляем центр комнаты
        Vector3Int center = new Vector3Int(
            Mathf.RoundToInt(roomView.floorTilemap.cellBounds.center.x),
            Mathf.RoundToInt(roomView.floorTilemap.cellBounds.center.y),
            0
        );

        int distanceFromCenter = 5; // Фиксированное расстояние от центра, как в RoomView

        // Определяем позицию двери в зависимости от направления
        Vector3Int doorPosition = Vector3Int.zero;

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

            default:
                return center;
        }

        // Проверяем, есть ли дверь в вычисленной позиции
        if (roomView.doorTilemap != null && roomView.doorTilemap.HasTile(doorPosition))
        {
            // Нашли дверь, теперь возвращаем позицию рядом с ней внутри комнаты
            switch (direction)
            {
                case DoorDirection.Top:
                    return new Vector3Int(doorPosition.x, doorPosition.y - 1, doorPosition.z); // Одна клетка вниз от двери

                case DoorDirection.Bottom:
                    return new Vector3Int(doorPosition.x, doorPosition.y + 1, doorPosition.z); // Одна клетка вверх от двери

                case DoorDirection.Left:
                    return new Vector3Int(doorPosition.x + 1, doorPosition.y, doorPosition.z); // Одна клетка вправо от двери

                case DoorDirection.Right:
                    return new Vector3Int(doorPosition.x - 1, doorPosition.y, doorPosition.z); // Одна клетка влево от двери

                default:
                    return doorPosition;
            }
        }
        else
        {
            // Если дверь не найдена в ожидаемой позиции, возвращаем центральную позицию
            return center;
        }
    }

    /// <summary>
    /// Находит ближайшую клетку с полом к заданной позиции
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <param name="targetPosition">Целевая позиция</param>
    /// <returns>Позиция ближайшей клетки с полом</returns>
    private Vector3Int FindNearestFloorTile(RoomView roomView, Vector3Int targetPosition)
    {
        // Проверяем сначала ближайшие клетки в порядке приоритета (ближайшие от двери)
        // Проверяем соседние клетки по одной за раз, начиная с самых близких

        // Проверяем саму целевую позицию
        if (roomView.floorTilemap.HasTile(targetPosition))
        {
            return targetPosition;
        }

        // Проверяем 4 ближайшие клетки (вверх, вниз, влево, вправо)
        Vector3Int[] nearbyPositions = {
            new Vector3Int(targetPosition.x, targetPosition.y + 1, targetPosition.z), // вверх
            new Vector3Int(targetPosition.x, targetPosition.y - 1, targetPosition.z), // вниз
            new Vector3Int(targetPosition.x - 1, targetPosition.y, targetPosition.z), // влево
            new Vector3Int(targetPosition.x + 1, targetPosition.y, targetPosition.z)  // вправо
        };

        foreach (Vector3Int pos in nearbyPositions)
        {
            if (roomView.floorTilemap.HasTile(pos))
            {
                return pos;
            }
        }

        // Если не нашли рядом, используем алгоритм поиска по радиусу
        for (int radius = 2; radius < 10; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius) // только по периметру
                    {
                        Vector3Int testPos = new Vector3Int(targetPosition.x + x, targetPosition.y + y, 0);

                        if (roomView.floorTilemap.HasTile(testPos))
                        {
                            return testPos;
                        }
                    }
                }
            }
        }

        // Если совсем не нашли, возвращаем центр комнаты
        var bounds = roomView.floorTilemap.cellBounds;
        Vector3Int centerCell = new Vector3Int(Mathf.RoundToInt(bounds.center.x), Mathf.RoundToInt(bounds.center.y), 0);

        if (roomView.floorTilemap.HasTile(centerCell))
        {
            return centerCell;
        }

        // Если и в центре нет пола, ищем там же
        return FindNearestFloorTileToCenter(roomView, centerCell);
    }

    /// <summary>
    /// Находит ближайшую клетку с полом к центральной позиции
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <param name="centerPosition">Центральная позиция</param>
    /// <returns>Позиция ближайшей клетки с полом</returns>
    private Vector3Int FindNearestFloorTileToCenter(RoomView roomView, Vector3Int centerPosition)
    {
        // Используем тот же алгоритм поиска по радиусу, начиная с центра
        for (int radius = 1; radius < 10; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius) // только по периметру
                    {
                        Vector3Int testPos = new Vector3Int(centerPosition.x + x, centerPosition.y + y, 0);

                        if (roomView.floorTilemap.HasTile(testPos))
                        {
                            return testPos;
                        }
                    }
                }
            }
        }

        // Если не нашли, возвращаем центральную позицию как последний вариант
        return centerPosition;
    }
    
    /// <summary>
    /// Удаляет текущую комнату
    /// </summary>
    private void DestroyCurrentRoom()
    {
        // Получаем текущую комнату из сервиса
        var currentRoomNode = roomGeneratorService.GetCurrentRoom();
        
        if (currentRoomNode != null && currentRoomNode.Room != null)
        {
            // Удаляем GameObject комнаты
            if (currentRoomNode.Room.gameObject != null)
            {
                Destroy(currentRoomNode.Room.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Переходит к следующей комнате в списке
    /// </summary>
    public void GoToNextRoom()
    {
        if (roomGeneratorService == null)
        {
            Debug.LogError("IRoomGeneratorService не инициализирован");
            return;
        }
        
        // Переходим к следующей комнате в списке
        var nextRoom = roomGeneratorService.MoveToNextRoom();
        
        if (nextRoom != null)
        {
            // Загружаем новую комнату
            LoadCurrentRoom();
        }
        else
        {
            Debug.Log("Больше нет комнат для перехода вперед");
        }
    }
    
    /// <summary>
    /// Переходит к предыдущей комнате в списке
    /// </summary>
    public void GoToPreviousRoom()
    {
        if (roomGeneratorService == null)
        {
            Debug.LogError("IRoomGeneratorService не инициализирован");
            return;
        }
        
        // Переходим к предыдущей комнате в списке
        var previousRoom = roomGeneratorService.MoveToPreviousRoom();
        
        if (previousRoom != null)
        {
            // Загружаем новую комнату
            LoadCurrentRoom();
        }
        else
        {
            Debug.Log("Больше нет комнат для перехода назад");
        }
    }
    
    /// <summary>
    /// Переходит к соседней комнате в указанном направлении
    /// Если комнаты не существует, генерирует её
    /// </summary>
    /// <param name="direction">Направление для перехода</param>
    public void GoToNeighborRoom(DoorDirection direction)
    {
        if (roomGeneratorService == null)
        {
            Debug.LogError("IRoomGeneratorService не инициализирован");
            return;
        }

        var currentRoom = roomGeneratorService.GetCurrentRoom();
        if (currentRoom != null)
        {
            // Пытаемся получить существующего соседа или генерируем новую комнату в этом направлении
            var neighbor = roomGeneratorService.GenerateRoomInDirection(direction);

            if (neighbor != null)
            {
                // Устанавливаем направление входа в новую комнату (противоположное направлению выхода)
                _entryDirection = GetOppositeDirection(direction);

                // Устанавливаем соседнюю комнату как текущую
                roomGeneratorService.SetCurrentRoom(neighbor);

                // Загружаем новую комнату
                LoadCurrentRoom();
            }
            else
            {
                Debug.Log($"Не удалось создать или получить комнату в направлении {direction}");
            }
        }
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
    /// Инициализирует первую комнату с центральным спауном (для начала игры)
    /// </summary>
    public void InitializeFirstRoom()
    {
        // Устанавливаем направление входа как null, чтобы использовать центр комнаты
        _entryDirection = null;
        LoadCurrentRoom();
    }

    /// <summary>
    /// Получает текущую комнату
    /// </summary>
    /// <returns>Текущая комната</returns>
    public RoomNode GetCurrentRoom()
    {
        if (roomGeneratorService != null)
        {
            return roomGeneratorService.GetCurrentRoom();
        }

        return null;
    }
    
    /// <summary>
    /// Получает соседей текущей комнаты
    /// </summary>
    /// <returns>Список соседей текущей комнаты</returns>
    public System.Collections.Generic.List<RoomNode> GetCurrentRoomNeighbors()
    {
        if (roomGeneratorService != null)
        {
            return roomGeneratorService.GetCurrentRoomNeighbors();
        }
        
        return new System.Collections.Generic.List<RoomNode>();
    }
    
    /// <summary>
    /// Перезапускает генерацию комнат
    /// </summary>
    public void RestartGeneration()
    {
        if (roomGeneratorService != null)
        {
            // Получаем компонент RoomView из префаба
            RoomView roomViewComponent = roomPrefab.GetComponent<RoomView>();
            if (roomViewComponent != null)
            {
                // Используем InitializeRooms вместо GenerateRooms для создания только первой комнаты
                roomGeneratorService.InitializeRooms(roomViewComponent, 10); // Пример: до 10 комнат
                InitializeFirstRoom();
            }
        }
    }
}