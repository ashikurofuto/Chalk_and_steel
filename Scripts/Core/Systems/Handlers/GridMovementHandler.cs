using Architecture.GlobalModules;
using Architecture.GlobalModules.Systems;
using ChalkAndSteel.Services;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using VContainer.Unity;

namespace Architecture.GlobalModules.Systems
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

        // === Параметры перемещения ===
        [SerializeField] private float _moveDuration = 0.2f; // Время перемещения между тайлами

        // === Состояния ===
        private Tilemap _currentFloorTilemap;
        private Tilemap _currentWallTilemap;
        private RoomView _currentRoomView; // Добавлено
        private Vector3Int _currentTilePosition;
        private bool _isInputEnabled = true;
        private float _moveDelay = 0.2f; // Задержка между движениями
        private float _lastMoveTime = 0f; // Время последнего движения
        private bool _isInitialized = false; // Добавлено
        // Store the room grid separately for door detection
        private int[,] _localRoomGrid;

        // Состояние блокировки для предотвращения новых команд во время перехода
        private bool _isTransitioning = false;

        // Время последнего перехода между комнатами
        private float _lastTransitionTime = 0f;

        // Задержка после перехода, чтобы избежать неправильной обработки ввода
        private float _transitionDelay = 0.3f;

        // Состояние кнопки движения для одиночного перемещения
        private bool _isMoveButtonPressed = false;

        // Состояние перемещения для предотвращения новых команд во время анимации
        private bool _isMoving = false;

        // Делегат для уведомления о завершении перемещения
        private System.Action _onMoveCompleted;

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
            Debug.Log("GridMovementHandler.Start(): Starting initialization...");

            // Подписываемся на событие генерации комнаты
            _eventBus.Subscribe<RoomGeneratedEvent>(OnRoomGenerated);

            // Инициализируем командный сервис с данными игрока
            // В начале может не быть сетки комнаты, поэтому инициализируем с null
            _commandService.InitializePlayerReceiver(_playerTransform, this, null, null, OnMoveCompleted);

            // Устанавливаем длительность перемещения
            var playerReceiver = _commandService.GetPlayerReceiver();
            if (playerReceiver != null)
            {
                playerReceiver.SetMoveDuration(_moveDuration);
            }

            // Отключаем ввод до тех пор, как не будет получено событие генерации комнаты
            _isInputEnabled = false;
            _isInitialized = false;

            // Находим RoomGenerationHandler после внедрения зависимостей
            _roomGenerationHandler = Object.FindFirstObjectByType<RoomGenerationHandler>();

            if (_roomGenerationHandler == null)
            {
                Debug.LogError("RoomGenerationHandler not found in scene!");
            }
            else
            {
                Debug.Log($"GridMovementHandler.Start(): Found RoomGenerationHandler: {_roomGenerationHandler.name}");

                // Попробуем получить текущую комнату напрямую, если событие не пришло
                TryInitializeFromCurrentRoom();
            }

            Debug.Log($"GridMovementHandler.Start(): Initialized with player transform at {_playerTransform.position}");

            // Сбрасываем состояние кнопки движения при старте
            _isMoveButtonPressed = false;
        }

        /// <summary>
        /// Попытка инициализации из текущей комнаты, если событие не было получено
        /// </summary>
        private void TryInitializeFromCurrentRoom()
        {
            if (_roomGenerationHandler != null)
            {
                var currentRoomNode = _roomGenerationHandler.GetCurrentRoom();
                if (currentRoomNode != null && currentRoomNode.Room != null)
                {
                    Debug.Log($"GridMovementHandler.TryInitializeFromCurrentRoom(): Found current room: {currentRoomNode.Room.name}, trying to initialize directly");

                    // Создаем фейковое событие для инициализации
                    var roomView = currentRoomNode.Room;
                    var spawnPosition = CalculateSpawnPositionFromRoom(roomView);

                    var fakeEvent = new RoomGeneratedEvent(spawnPosition, roomView);
                    OnRoomGenerated(fakeEvent);
                }
            }

            // Сбрасываем состояние кнопки движения при попытке инициализации
            _isMoveButtonPressed = false;
        }

        /// <summary>
        /// Рассчитывает позицию спауна из текущей комнаты
        /// </summary>
        private Vector3 CalculateSpawnPositionFromRoom(RoomView roomView)
        {
            if (roomView.floorTilemap != null)
            {
                var bounds = roomView.floorTilemap.cellBounds;
                var center = bounds.center;
                var centerCell = new Vector3Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y), 0);

                // Проверяем, есть ли пол в центральной ячейке
                if (roomView.floorTilemap.HasTile(centerCell))
                {
                    return roomView.floorTilemap.GetCellCenterWorld(centerCell);
                }
                else
                {
                    // Ищем ближайшую ячейку с полом
                    for (int radius = 1; radius < 10; radius++)
                    {
                        for (int x = -radius; x <= radius; x++)
                        {
                            for (int y = -radius; y <= radius; y++)
                            {
                                if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
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
                }
            }

            // Возвращаем центральную позицию как fallback
            return Vector3.zero;
        }



        private void OnDestroy()
        {
            // Отписываемся от события генерации комнаты
            _eventBus?.Unsubscribe<RoomGeneratedEvent>(OnRoomGenerated);

            // Сбрасываем состояние кнопки движения при уничтожении объекта
            _isMoveButtonPressed = false;
        }

        private void Update()
        {
            // Добавляем отладку для проверки работы Update
            if (!_isInputEnabled || !_isInitialized || _isTransitioning || (Time.time - _lastTransitionTime < _transitionDelay))
            {
                // Добавляем периодическую отладку состояния
                if (Time.time % 2f < Time.deltaTime) // Лог каждые 2 секунды
                {
                    Debug.Log($"GridMovementHandler.Update(): Input enabled: {_isInputEnabled}, Initialized: {_isInitialized}, Transitioning: {_isTransitioning}, Time since transition: {Time.time - _lastTransitionTime}");
                    Debug.Log($"GridMovementHandler.Update(): Current player position: {_playerTransform.position}");
                    Debug.Log($"GridMovementHandler.Update(): Current tilemap: {_currentFloorTilemap != null}, Wall tilemap: {_currentWallTilemap != null}");

                    // Попробуем снова инициализироваться, если не инициализирован
                    if (!_isInitialized && _roomGenerationHandler != null)
                    {
                        TryInitializeFromCurrentRoom();
                    }
                }
                return;
            }

            // Обработка ввода движения
            Vector2 moveDirection = _inputService.GetMoveDirection();

            // Проверяем, нажата ли кнопка движения
            bool isMoveKeyPressed = moveDirection.magnitude > 0.1f;

            // Добавляем отладку для проверки получения ввода
            if (isMoveKeyPressed)
            {
                Debug.Log($"GridMovementHandler.Update(): Input detected - moveDirection: {moveDirection}");

                // Обрабатываем движение только при первом нажатии, а не при удержании
                // И только если игрок не находится в процессе перемещения
                if (!_isMoveButtonPressed && !_isMoving)
                {
                    _isMoveButtonPressed = true; // Помечаем, что кнопка нажата

                    // Проверяем, прошло ли достаточно времени с последнего движения
                    if (Time.time - _lastMoveTime < _moveDelay)
                    {
                        Debug.Log($"GridMovementHandler.Update(): Too early for next move, delay: {Time.time - _lastMoveTime}");
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
                        Debug.Log($"GridMovementHandler.Update(): Attempting to move in direction: {direction}");

                        // Используем внутренний метод CanMoveTo, который обрабатывает двери и переходы
                        if (CanMoveTo(direction))
                        {
                            Debug.Log($"GridMovementHandler.Update(): Move to {direction} is valid, executing command");

                            // Устанавливаем флаг перемещения, чтобы заблокировать новые команды до завершения текущего перемещения
                            _isMoving = true;

                            // Выполняем команду перемещения через систему команд
                            // Только если это не переход в другую комнату (в этом случае CanMoveTo уже вызвал переход)
                            Vector3Int targetTilePosition = _currentTilePosition + direction;

                            // Проверяем, является ли целевая позиция дверью
                            if (IsDoor(targetTilePosition))
                            {
                                Debug.Log($"GridMovementHandler.Update(): Target position {targetTilePosition} is a door, initiating transition");

                                // Устанавливаем флаг перехода, чтобы заблокировать новые команды до завершения перехода
                                _isTransitioning = true;

                                // Определяем направление двери
                                DoorDirection doorDirection = DetermineDoorDirection(targetTilePosition, _currentFloorTilemap);
                                Debug.Log($"GridMovementHandler.Update(): Determined door direction: {doorDirection} for tile {targetTilePosition} with floor bounds: {_currentFloorTilemap.cellBounds}");

                                // Вызываем переход
                                Debug.Log($"GridMovementHandler.Update(): Calling GoToNeighborRoom with direction: {doorDirection}");
                                if (_roomGenerationHandler != null)
                                {
                                    _roomGenerationHandler.GoToNeighborRoom(doorDirection);
                                    Debug.Log($"GridMovementHandler.Update(): GoToNeighborRoom called successfully");
                                }
                                else
                                {
                                    Debug.LogError("GridMovementHandler.Update(): RoomGenerationHandler is null, cannot transition to neighbor room");
                                    // Сбрасываем флаг, если не удалось выполнить переход
                                    _isTransitioning = false;

                                    // Также сбрасываем состояние кнопки движения при ошибке перехода
                                    _isMoveButtonPressed = false;

                                    // Сбрасываем флаг перемещения при ошибке
                                    _isMoving = false;
                                }

                                _lastMoveTime = Time.time; // Обновляем время последнего движения

                                // Сбрасываем состояние кнопки движения после выполнения перехода
                                _isMoveButtonPressed = false;
                            }
                            else
                            {
                                Debug.Log($"GridMovementHandler.Update(): Executing move command, current position: {_playerTransform.position}");

                                // Выполняем команду перемещения через систему команд
                                // Теперь, когда мы не передаем roomGrid, будет использоваться MoveCommand с Grid
                                _commandService.ExecuteMoveCommand(direction);

                                // Обновляем текущую позицию в тайлмапе
                                _currentTilePosition = targetTilePosition;

                                Debug.Log($"GridMovementHandler.Update(): Move executed, new position: {_playerTransform.position}, new tile position: {_currentTilePosition}");

                                _lastMoveTime = Time.time; // Обновляем время последнего движения

                                // Сбрасываем состояние кнопки движения после выполнения обычного перемещения
                                _isMoveButtonPressed = false;
                            }
                        }
                        else
                        {
                            Debug.Log($"GridMovementHandler.Update(): Move to {direction} is not valid");
                        }
                    }
                }
            }
            else
            {
                // Кнопка движения отпущена, сбрасываем флаг
                _isMoveButtonPressed = false;

                // Периодическая отладка, когда нет ввода
                if (Time.time % 5f < Time.deltaTime) // Лог каждые 5 секунд
                {
                    Debug.Log($"GridMovementHandler.Update(): Waiting for input... Player position: {_playerTransform.position}, Current tile position: {_currentTilePosition}");
                }
            }

            // Обработка других команд ввода
            if (_inputService.IsInteractPressed())
            {
                Debug.Log("GridMovementHandler.Update(): Interact button pressed");
                // Обработка взаимодействия
            }

            if (_inputService.IsInventoryPressed())
            {
                Debug.Log("GridMovementHandler.Update(): Inventory button pressed");
                // Обработка инвентаря
            }

            if (_inputService.IsPausePressed())
            {
                Debug.Log("GridMovementHandler.Update(): Pause button pressed");
                // Обработка паузы
            }

            if (_inputService.IsUndoPressed())
            {
                Debug.Log("GridMovementHandler.Update(): Undo button pressed, executing undo command");
                _commandService.UndoLastCommand();
            }

            // Если любая из других кнопок нажата, сбрасываем состояние кнопки движения
            if (_inputService.IsInteractPressed() || _inputService.IsInventoryPressed() || _inputService.IsPausePressed() || _inputService.IsUndoPressed())
            {
                _isMoveButtonPressed = false;
            }
        }

        // === Управление состоянием ===
        private void ValidateSceneReferences()
        {
            if (_playerTransform == null)
                throw new System.NullReferenceException($"{nameof(_playerTransform)} is not assigned in the scene");

            // Сбрасываем состояние кнопки движения при валидации ссылок
            _isMoveButtonPressed = false;
        }

        /// <summary>
        /// Обработчик события генерации комнаты
        /// </summary>
        /// <param name="event">Событие с информацией о сгенерированной комнате</param>
        private void OnRoomGenerated(RoomGeneratedEvent @event)
        {
            Debug.Log($"GridMovementHandler.OnRoomGenerated(): Received RoomGeneratedEvent: SpawnPosition={@event.SpawnPosition}, RoomView={@event.RoomView}");

            if (@event.RoomView == null)
            {
                Debug.LogWarning("GridMovementHandler.OnRoomGenerated(): RoomView is null in RoomGeneratedEvent, skipping room grid update");
                return;
            }

            // Обновляем тайлмапы
            _currentFloorTilemap = @event.RoomView.floorTilemap;
            _currentWallTilemap = @event.RoomView.wallTilemap;
            _currentRoomView = @event.RoomView; // Добавлено

            // Вычисляем текущую позицию в тайлмапе на основе спаун-позиции
            _currentTilePosition = _currentFloorTilemap.WorldToCell(@event.SpawnPosition);

            // Устанавливаем позицию игрока в центр ячейки сетки для правильного выравнивания
            Vector3 alignedSpawnPosition = _currentFloorTilemap.GetCellCenterWorld(_currentTilePosition);
            _playerTransform.position = alignedSpawnPosition;

            // Создаем массив проходимости для текущей комнаты
            _localRoomGrid = CreateRoomGridFromTilemaps(_currentFloorTilemap, _currentWallTilemap);

            // Обновляем PlayerReceiver в CommandService с новой информацией
            // Получаем Grid компонент из родителя тайлмапа
            Grid grid = _currentFloorTilemap.GetComponentInParent<Grid>();

            // Для движения в пределах комнаты используем только Grid, чтобы использовать MoveCommand
            // Это позволит оставаться на центрах ячеек сетки
            _commandService.InitializePlayerReceiver(_playerTransform, this, grid, null, OnMoveCompleted);

            // Устанавливаем длительность перемещения
            var playerReceiver = _commandService.GetPlayerReceiver();
            if (playerReceiver != null)
            {
                playerReceiver.SetMoveDuration(_moveDuration);
            }

            // Включаем ввод после инициализации
            _isInputEnabled = true;
            _isInitialized = true;

            // Сбрасываем флаг перехода, так как мы успешно вошли в новую комнату
            _isTransitioning = false;

            // Сбрасываем состояние кнопки движения при загрузке новой комнаты
            _isMoveButtonPressed = false;

            // Сбрасываем состояние перемещения при загрузке новой комнаты
            _isMoving = false;

            // Устанавливаем время последнего перехода
            _lastTransitionTime = Time.time;

            Debug.Log($"GridMovementHandler.OnRoomGenerated(): Player position updated to {alignedSpawnPosition} (cell center), current tile position updated to {_currentTilePosition}, room grid updated, input enabled: {_isInputEnabled}, initialized: {_isInitialized}, transitioning: {_isTransitioning}");
        }

        public void EnableInput()
        {
            _isInputEnabled = true;
            _isMoveButtonPressed = false; // Сбрасываем состояние кнопки при включении ввода
        }

        public void DisableInput()
        {
            _isInputEnabled = false;
            _isMoveButtonPressed = false; // Сбрасываем состояние кнопки при выключении ввода
        }
        public void SetPlayerPosition(Vector3 position)
        {
            _playerTransform.position = position;
            _isMoveButtonPressed = false; // Сбрасываем состояние кнопки при установке позиции игрока
        }

        /// <summary>
        /// Вызывается при завершении перемещения
        /// </summary>
        private void OnMoveCompleted()
        {
            // Сбрасываем флаг перемещения при завершении анимации
            _isMoving = false;
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

                // Если позиция за пределами границ, проверим, может быть это дверь?
                if (IsDoor(targetTilePosition))
                {
                    Debug.Log($"Target position {targetTilePosition} is out of bounds but is a door tile, allowing transition");
                    return true; // Позволяем переход через дверь
                }

                return false;
            }

            // Проверяем, есть ли тайл пола в целевой позиции
            if (!_currentFloorTilemap.HasTile(targetTilePosition))
            {
                // Проверяем, может это дверь?
                if (IsDoor(targetTilePosition))
                {
                    Debug.Log($"Target position {targetTilePosition} does not have a floor tile but is a door tile, allowing transition");
                    return true; // Позволяем переход через дверь
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

            // Проверяем проходимость через локальный roomGrid (если доступен)
            if (_localRoomGrid != null && _currentFloorTilemap != null)
            {
                // Преобразуем координаты тайлмапа в индексы массива roomGrid
                // Массив индексируется от (0,0) до (width-1, height-1)
                // Координаты тайлмапа могут быть отрицательными в зависимости от bounds
                var bounds = _currentFloorTilemap.cellBounds;
                int arrayX = targetTilePosition.x - bounds.xMin;
                int arrayY = targetTilePosition.y - bounds.yMin;

                // Проверяем, что позиция в пределах локального массива
                if (arrayX >= 0 && arrayX < _localRoomGrid.GetLength(0) &&
                    arrayY >= 0 && arrayY < _localRoomGrid.GetLength(1))
                {
                    // Проверяем, проходима ли клетка в локальной сетке
                    if (_localRoomGrid[arrayX, arrayY] == 0)
                    {
                        Debug.Log($"Target position {targetTilePosition} is marked as impassable in local room grid (array index [{arrayX},{arrayY}])");
                        return false;
                    }
                }
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

                    // Проверяем, есть ли дверь в этой позиции (двери также проходимы)
                    bool hasDoor = _currentRoomView != null &&
                                   _currentRoomView.doorTilemap != null &&
                                   _currentRoomView.doorTilemap.HasTile(tilePosition);

                    // Клетка проходима, если есть пол и нет стены (но есть дверь), или есть пол и нет ни стены, ни двери
                    if (hasFloor && !hasWall)
                    {
                        intGrid[x, y] = 1; // Проходимо
                    }
                    // Двери также считаются проходимыми
                    else if (hasDoor)
                    {
                        intGrid[x, y] = 1; // Проходимо (дверь)
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
