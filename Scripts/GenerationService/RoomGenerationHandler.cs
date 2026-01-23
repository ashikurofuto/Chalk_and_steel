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
        
        // Инициализируем генерацию - создаем только первую комнату
        roomGeneratorService.InitializeRooms(roomViewComponent, 10); // Пример: до 10 комнат
        
        // Загружаем текущую комнату
        LoadCurrentRoom();
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
                    // По умолчанию устанавливаем позицию появления в центр комнаты
                    Vector3 spawnPosition = CalculateSpawnPosition(roomView);
                    Vector3Int gridPosition = new Vector3Int(Mathf.RoundToInt(spawnPosition.x), Mathf.RoundToInt(spawnPosition.y), 0);
                    
                    var roomGeneratedEvent = new RoomGeneratedEvent(spawnPosition, roomView, gridPosition);
                    
                    // Проверяем, что EventBus инициализирован перед публикацией события
                    if (_eventBus != null)
                    {
                        _eventBus.Publish(roomGeneratedEvent);
                        Debug.Log($"Published RoomGeneratedEvent: SpawnPosition={spawnPosition}, RoomView={roomView}");
                    }
                    else
                    {
                        Debug.LogWarning("EventBus is not initialized, cannot publish RoomGeneratedEvent");
                    }
                }
                else
                {
                    Debug.LogWarning("RoomView has null tilemaps, cannot calculate spawn position and generate event");
                }
            }
        }
    }
    
    /// <summary>
    /// Рассчитывает позицию появления игрока в комнате (по умолчанию - центр комнаты)
    /// </summary>
    /// <param name="roomView">Компонент RoomView с тайлмапами</param>
    /// <returns>Позиция появления игрока</returns>
    private Vector3 CalculateSpawnPosition(RoomView roomView)
    {
        // Проверяем, что тайлмапы инициализированы
        if (roomView.floorTilemap == null)
        {
            Debug.LogWarning("Floor tilemap is not assigned, returning default position");
            return Vector3.zero;
        }
        
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
                LoadCurrentRoom();
            }
        }
    }
}