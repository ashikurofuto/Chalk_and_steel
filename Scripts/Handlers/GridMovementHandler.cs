using Architecture.GlobalModules;
using Architecture.GlobalModules.Commands;
using ChalkAndSteel.Services;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;

namespace Architecture.GlobalModules.Handlers
{
    /// <summary>
    /// Обработчик движения по сетке с использованием новой системы команд
    /// </summary>
    public class GridMovementHandler : MonoBehaviour, IStartable
    {
        // === Зависимости (внедряются через VContainer) ===
        private IInputService _inputService;
        private ICommandService _commandService;
        private IEventBus _eventBus;
        private IRoomGeneratorService _roomGeneratorService; // Добавлено
        private RoomGenerationHandler _roomGenerationHandler; // Добавлено

        // === Ссылки на сцену (настраиваются в Inspector) ===
        [SerializeField] private Transform _playerTransform;

        // === Состояния ===
        private Tilemap _currentFloorTilemap;
        private Tilemap _currentWallTilemap;
        private RoomView _currentRoomView; // Добавлено
        private Vector3Int _currentTilePosition;
        private bool _isInputEnabled = true;
        private float _moveDelay = 0.2f; // Задержка между движениями
        private float _lastMoveTime = 0f; // Время последнего движения
        private bool _isInitialized = false; // Добавлено

        // === Внедрение зависимостей ===
        [Inject]
        public void Construct(
            IInputService inputService,
            ICommandService commandService,
            IEventBus eventBus,
            IRoomGeneratorService roomGeneratorService) // Убран RoomGenerationHandler
        {
            _inputService = inputService;
            _commandService = commandService;
            _eventBus = eventBus;
            _roomGeneratorService = roomGeneratorService; // Добавлено
        }

        // === Инициализация ===
        private void Awake()
        {
            ValidateSceneReferences();
        }

        // Реализация IStartable
        public void Start()
        {
            // Подписываемся на событие генерации комнаты
            _eventBus.Subscribe<RoomGeneratedEvent>(OnRoomGenerated);

            // Инициализируем командный сервис с данными игрока
            // В начале может не быть сетки комнаты, поэтому инициализируем с null
            _commandService.InitializePlayerReceiver(_playerTransform, null, null);

            // Отключаем ввод до тех пор, пока не будет получено событие генерации комнаты
            _isInputEnabled = false;

            // Находим RoomGenerationHandler после внедрения зависимостей
            _roomGenerationHandler = FindObjectOfType<RoomGenerationHandler>();

            if (_roomGenerationHandler == null)
            {
                Debug.LogError("RoomGenerationHandler not found in scene!");
            }

            Debug.Log($"GridMovementHandler.Start(): Initialized with player transform at {_playerTransform.position}");
        }



        private void OnDestroy()
        {
            // Отписываемся от события генерации комнаты
            _eventBus?.Unsubscribe<RoomGeneratedEvent>(OnRoomGenerated);
        }

        private void Update()
        {
            if (!_isInputEnabled || !_isInitialized) // Добавлено условие !_isInitialized
                return;

            // Обработка ввода движения
            Vector2 moveDirection = _inputService.GetMoveDirection();
            if (moveDirection.magnitude > 0.1f)
            {
                // Проверяем, прошло ли достаточно времени с последнего движения
                if (Time.time - _lastMoveTime < _moveDelay)
                {
                    return; // Слишком рано для следующего движения
                }

                Vector3Int direction = Vector3Int.RoundToInt(new Vector3(moveDirection.x, moveDirection.y, 0));

                // Преобразуем диагональные движения в ортогональные для пошаговой системы
                float Xsign = direction.x != 0 ? Mathf.Sign(direction.x) : 0;
                float Ysign = direction.y != 0 ? Mathf.Sign(direction.y) : 0;
                int signX = (int)Xsign;
                int signY = (int)Ysign;

                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    direction = new Vector3Int(signX, 0, 0);
                }
                else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
                {
                    direction = new Vector3Int(0, signY, 0);
                }
                else
                {
                    // Если x и y равны по модулю, выбираем одно направление
                    direction = new Vector3Int(signX, 0, 0);
                }

                if (direction != Vector3Int.zero)
                {
                    Debug.Log($"Attempting to move in direction: {direction}");

                    // Проверяем проходимость целевой позиции
                    if (CanMoveTo(direction))
                    {
                        Debug.Log($"Move to {direction} is valid, executing command");

                        // Вычисляем новую мировую позицию на основе тайлмапа
                        Vector3Int newTilePosition = _currentTilePosition + direction;
                        Vector3 newWorldPosition = _currentFloorTilemap.GetCellCenterWorld(newTilePosition);

                        // Перемещаем игрока напрямую
                        _commandService.MovePlayerToWorldPosition(newWorldPosition);

                        // Обновляем текущую позицию в тайлмапе
                        _currentTilePosition = newTilePosition;

                        _lastMoveTime = Time.time; // Обновляем время последнего движения
                    }
                    else
                    {
                        Debug.Log($"Move to {direction} is not valid");
                    }
                }
            }

            // Обработка других команд ввода
            if (_inputService.IsInteractPressed())
            {
                Debug.Log("Interact button pressed");
                // Обработка взаимодействия
            }

            if (_inputService.IsInventoryPressed())
            {
                Debug.Log("Inventory button pressed");
                // Обработка инвентаря
            }

            if (_inputService.IsPausePressed())
            {
                Debug.Log("Pause button pressed");
                // Обработка паузы
            }

            if (_inputService.IsUndoPressed())
            {
                Debug.Log("Undo button pressed, executing undo command");
                _commandService.UndoLastCommand();
            }
        }

        // === Управление состоянием ===
        private void ValidateSceneReferences()
        {
            if (_playerTransform == null)
                throw new System.NullReferenceException($"{nameof(_playerTransform)} is not assigned in the scene");
        }

        /// <summary>
        /// Обработчик события генерации комнаты
        /// </summary>
        /// <param name="event">Событие с информацией о сгенерированной комнате</param>
        private void OnRoomGenerated(RoomGeneratedEvent @event)
        {
            Debug.Log($"Received RoomGeneratedEvent: SpawnPosition={@event.SpawnPosition}, RoomView={@event.RoomView}");

            if (@event.RoomView == null)
            {
                Debug.LogWarning("RoomView is null in RoomGeneratedEvent, skipping room grid update");
                return;
            }

            // Обновляем тайлмапы
            _currentFloorTilemap = @event.RoomView.floorTilemap;
            _currentWallTilemap = @event.RoomView.wallTilemap;
            _currentRoomView = @event.RoomView; // Добавлено

            // Вычисляем текущую позицию в тайлмапе на основе спаун-позиции
            _currentTilePosition = _currentFloorTilemap.WorldToCell(@event.SpawnPosition);

            // Устанавливаем позицию игрока в точку появления
            _playerTransform.position = @event.SpawnPosition;

            // Создаем массив проходимости для текущей комнаты
            int[,] roomGrid = CreateRoomGridFromTilemaps(_currentFloorTilemap, _currentWallTilemap);

            // Обновляем PlayerReceiver в CommandService с новой информацией
            _commandService.InitializePlayerReceiver(_playerTransform, null, roomGrid);

            // Включаем ввод после инициализации
            _isInputEnabled = true;
            _isInitialized = true;

            Debug.Log($"Player position updated to {@event.SpawnPosition}, current tile position updated to {_currentTilePosition}, room grid updated, input enabled");
        }

        public void EnableInput() => _isInputEnabled = true;
        public void DisableInput() => _isInputEnabled = false;
        public void SetPlayerPosition(Vector3 position)
        {
            _playerTransform.position = position;
        }

        /// <summary>
        /// Проверяет, можно ли выполнить перемещение в заданном направлении
        /// </summary>
        /// <param name="direction">Направление для проверки</param>
        /// <returns>True, если перемещение возможно</returns>
        private bool CanMoveTo(Vector3Int direction)
        {
            if (_currentFloorTilemap == null)
            {
                Debug.LogWarning("CanMoveTo: Floor tilemap is not assigned, cannot validate movement");
                return false;
            }

            // Рассчитываем целевую позицию в тайлмапе
            Vector3Int targetTilePosition = _currentTilePosition + direction;

            Debug.Log($"Checking move from {_currentTilePosition} to {targetTilePosition}");

            // Проверяем, находится ли целевая позиция в пределах границ тайлмапа
            if (!IsWithinTilemapBounds(_currentFloorTilemap, targetTilePosition))
            {
                Debug.Log($"Target position {targetTilePosition} is out of floor tilemap bounds");
                return false;
            }

            // Проверяем, есть ли тайл пола в целевой позиции
            if (!_currentFloorTilemap.HasTile(targetTilePosition))
            {
                // Проверяем, может это дверь?
                if (IsDoor(targetTilePosition))
                {
                    Debug.Log($"Target position {targetTilePosition} does not have a floor tile but is a door tile, initiating transition");

                    // Определяем направление двери
                    DoorDirection doorDirection = DetermineDoorDirection(targetTilePosition, _currentFloorTilemap);
                    Debug.Log($"Determined door direction: {doorDirection} for tile {targetTilePosition} with floor bounds: {_currentFloorTilemap.cellBounds}");

                    // Вызываем переход
                    Debug.Log($"Calling GoToNeighborRoom with direction: {doorDirection}");
                    _roomGenerationHandler.GoToNeighborRoom(doorDirection);
                    Debug.Log($"GoToNeighborRoom called successfully");

                    // Возвращаем true, чтобы сигнализировать, что команда движения выполнена (на самом деле произошел переход)
                    return true;
                }
                else
                {
                    Debug.Log($"Target position {targetTilePosition} does not have a floor tile and is not a door");
                    return false;
                }
            }

            // Проверяем, есть ли тайл стены в целевой позиции
            if (_currentWallTilemap != null && _currentWallTilemap.HasTile(targetTilePosition))
            {
                Debug.Log($"Target position {targetTilePosition} has a wall tile");
                return false;
            }

            // Если все проверки пройдены, путь открыт
            Debug.Log($"Move to {targetTilePosition} is valid");
            return true;
        }

        /// <summary>
        /// Проверяет, является ли ячейка дверью
        /// </summary>
        /// <param name="tilePosition">Позиция ячейки для проверки</param>
        /// <returns>True, если ячейка является дверью</returns>
        private bool IsDoor(Vector3Int tilePosition)
        {
            return _currentRoomView != null &&
                   _currentRoomView.doorTilemap != null &&
                   _currentRoomView.doorTilemap.HasTile(tilePosition);
        }

        /// <summary>
        /// Проверяет, находится ли позиция в пределах границ тайлмапа
        /// </summary>
        /// <param name="tilemap">Тайлмап для проверки</param>
        /// <param name="position">Позиция для проверки</param>
        /// <returns>True, если позиция в пределах границ</returns>
        private bool IsWithinTilemapBounds(Tilemap tilemap, Vector3Int position)
        {
            BoundsInt bounds = tilemap.cellBounds;
            return position.x >= bounds.xMin && position.x < bounds.xMax &&
                   position.y >= bounds.yMin && position.y < bounds.yMax;
        }

        /// <summary>
        /// Создает целочисленную сетку проходимости на основе тайлмапов комнаты
        /// </summary>
        /// <param name="floorTilemap">Тайлмап пола</param>
        /// <param name="wallTilemap">Тайлмап стен</param>
        /// <returns>Целочисленная сетка, где 1 - проходимо, 0 - непроходимо</returns>
        private int[,] CreateRoomGridFromTilemaps(Tilemap floorTilemap, Tilemap wallTilemap)
        {
            if (floorTilemap == null)
            {
                Debug.LogWarning("Floor tilemap is not assigned, cannot create grid from tilemaps");
                return null;
            }

            // Получаем границы тайлмапа пола (основной ориентир для размера комнаты)
            BoundsInt floorBounds = floorTilemap.cellBounds;

            // Если тайлмап пола пустой, возвращаем null
            if (floorBounds.size.x <= 0 || floorBounds.size.y <= 0)
            {
                Debug.LogWarning("Floor tilemap bounds are invalid, cannot create grid");
                return null;
            }

            int width = floorBounds.size.x;
            int height = floorBounds.size.y;
            int[,] intGrid = new int[width, height];

            // Заполняем сетку на основе наличия тайлов пола и стен
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Преобразуем координаты сетки в координаты тайлмапа
                    Vector3Int tilePosition = new Vector3Int(floorBounds.xMin + x, floorBounds.yMin + y, 0);

                    // Проверяем, есть ли тайл пола в этой позиции
                    bool hasFloor = floorTilemap.HasTile(tilePosition);
                    // Проверяем, есть ли тайл стены в этой позиции
                    bool hasWall = wallTilemap != null && wallTilemap.HasTile(tilePosition);

                    // Клетка проходима, если есть пол и нет стены
                    if (hasFloor && !hasWall)
                    {
                        intGrid[x, y] = 1; // Проходимо
                    }
                    else
                    {
                        intGrid[x, y] = 0; // Непроходимо
                    }
                }
            }

            return intGrid;
        }

        /// <summary>
        /// Вспомогательный метод для определения направления двери
        /// </summary>
        /// <param name="tilePosition">Позиция ячейки двери</param>
        /// <param name="floorTilemap">Тайлмап пола для определения границ</param>
        /// <returns>Направление двери</returns>
        private DoorDirection DetermineDoorDirection(Vector3Int tilePosition, Tilemap floorTilemap)
        {
            BoundsInt bounds = floorTilemap.cellBounds;
            Debug.Log($"DetermineDoorDirection: Checking tile {tilePosition} against bounds {bounds}");

            // Проверяем, находится ли ячейка на границе тайлмапа
            if (tilePosition.x == bounds.xMin) // Левая граница
            {
                Debug.Log($"DetermineDoorDirection: Tile {tilePosition} is LEFT door");
                return DoorDirection.Left;
            }
            else if (tilePosition.x == bounds.xMax - 1) // Правая граница (xMax - 1, так как индексация с 0)
            {
                Debug.Log($"DetermineDoorDirection: Tile {tilePosition} is RIGHT door");
                return DoorDirection.Right;
            }
            else if (tilePosition.y == bounds.yMin) // Нижняя граница
            {
                Debug.Log($"DetermineDoorDirection: Tile {tilePosition} is BOTTOM door");
                return DoorDirection.Bottom;
            }
            else if (tilePosition.y == bounds.yMax - 1) // Верхняя граница (yMax - 1, так как индексация с 0)
            {
                Debug.Log($"DetermineDoorDirection: Tile {tilePosition} is TOP door");
                return DoorDirection.Top;
            }

            // Если ячейка не на границе, это не дверь (хотя мы уже проверили HasTile в doorTilemap)
            // Возвращаем Top как значение по умолчанию или бросаем исключение
            Debug.LogWarning($"DetermineDoorDirection: Tile at {tilePosition} is not on a boundary, cannot determine door direction. Bounds: {bounds}");
            return DoorDirection.Top; // Или throw new ArgumentException(...)
        }
    }
}
