using Architecture.GlobalModules;
using Architecture.GlobalModules.Commands;
using ChalkAndSteel.Services;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;

namespace Architecture.GlobalModules.Handlers
{
    /// <summary>
    /// Обработчик движения по сетке с использованием новой системы команд
    /// </summary>
    public class GridMovementHandler : MonoBehaviour
    {
        // === Зависимости (внедряются через VContainer) ===
        private IInputService _inputService;
        private ICommandService _commandService;
        private IEventBus _eventBus;

        // === Ссылки на сцену (настраиваются в Inspector) ===
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _moveDelay = 0.2f; // Задержка между движениями
        [SerializeField] private RoomView _roomView; // Ссылка на RoomView для доступа к тайлмапам

        // === Состояния ===
        private bool _isInputEnabled = true;
        private float _lastMoveTime = 0f; // Время последнего движения

        // === Внедрение зависимостей ===
        [Inject]
        public void Construct(
            IInputService inputService,
            ICommandService commandService,
            IEventBus eventBus)
        {
            _inputService = inputService;
            _commandService = commandService;
            _eventBus = eventBus;
        }

        // === Инициализация ===
        private void Awake()
        {
            ValidateSceneReferences();
        }
        
        private void Start()
        {
            // Подписываемся на событие генерации комнаты
            _eventBus.Subscribe<RoomGeneratedEvent>(OnRoomGenerated);
            
            // Инициализируем командный сервис с данными игрока
            // В начале может не быть сетки комнаты, поэтому инициализируем с null
            _commandService.InitializePlayerReceiver(_playerTransform, null, GetRoomGridAsIntArray());
            
            Debug.Log($"GridMovementHandler.Start(): Initialized with player transform at {_playerTransform.position}, " +
                      $"current room grid is {(GetRoomGridAsIntArray() != null ? "available" : "not available")}");
        }



        private void OnDestroy()
        {
            // Отписываемся от события генерации комнаты
            _eventBus?.Unsubscribe<RoomGeneratedEvent>(OnRoomGenerated);
        }

        private void Update()
        {
            if (!_isInputEnabled)
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
                float Xsign =direction.x != 0 ? Mathf.Sign(direction.x) : 0;
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
                    if (_commandService.CanMoveTo(direction))
                    {
                        Debug.Log($"Move to {direction} is valid, executing command");
                        _commandService.ExecuteMoveCommand(direction);
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
            
            // _roomView может быть null до получения его через событие, поэтому не проверяем его здесь
        }

        /// <summary>
        /// Обновляет сетку текущей комнаты на основе тайлмапов
        /// </summary>
        public void UpdateCurrentRoomGrid()
        {
            // Преобразуем тайлмапы в int[,] для командного сервиса
            int[,] intGrid = GetRoomGridAsIntArray();
            
            // Обновляем командный сервис с новой информацией о комнате
            _commandService.InitializePlayerReceiver(_playerTransform, null, intGrid);
            
            if(intGrid != null)
            {
                Debug.Log($"UpdateCurrentRoomGrid called: converted intGrid dimensions = ({intGrid.GetLength(0)}x{intGrid.GetLength(1)}), " +
                          $"player position = {_playerTransform.position}");
            }
            else
            {
                Debug.Log($"UpdateCurrentRoomGrid called: intGrid is null");
            }
        }
        
        /// <summary>
        /// Устаревший метод, оставлен для совместимости
        /// </summary>
        /// <param name="roomGrid">Сетка комнаты с тайлами</param>
        public void SetCurrentRoomGrid(DualLayerTile[,] roomGrid)
        {
            // Вызываем обновленный метод, так как теперь мы используем тайлмапы для определения проходимости
            UpdateCurrentRoomGrid();
        }

        /// <summary>
        /// Создает целочисленную сетку проходимости на основе тайлмапов комнаты
        /// </summary>
        /// <returns>Целочисленная сетка, где 1 - проходимо, 0 - непроходимо</returns>
        private int[,] GetRoomGridAsIntArray()
        {
            if (_roomView == null)
            {
                Debug.LogWarning("RoomView is not assigned, cannot create grid from tilemaps");
                return null;
            }
    
            // Проверяем, что тайлмапы инициализированы
            if (_roomView.floorTilemap == null || _roomView.wallTilemap == null)
            {
                Debug.LogWarning("Tilemaps are not assigned in RoomView, cannot create grid from tilemaps");
                return null;
            }
    
            // Получаем границы тайлмапа пола (основной ориентир для размера комнаты)
            BoundsInt floorBounds = _roomView.floorTilemap.cellBounds;
            
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
                    bool hasFloor = _roomView.floorTilemap.HasTile(tilePosition);
                    // Проверяем, есть ли тайл стены в этой позиции
                    bool hasWall = _roomView.wallTilemap.HasTile(tilePosition);
                    
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
            
            // Обновляем ссылку на RoomView
            _roomView = @event.RoomView;
            
            // Устанавливаем позицию игрока в точку появления
            _playerTransform.position = @event.SpawnPosition;
            
            // Обновляем сетку комнаты на основе новых данных
            UpdateCurrentRoomGrid();
            
            Debug.Log($"Player position updated to #{@event.SpawnPosition}, room grid updated");
        }

        public void EnableInput() => _isInputEnabled = true;
        public void DisableInput() => _isInputEnabled = false;
        public void SetPlayerPosition(Vector3 position)
        {
            _playerTransform.position = position;
        }
    }
}
