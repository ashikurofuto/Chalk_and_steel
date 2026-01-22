using Architecture.GlobalModules;
using Architecture.GlobalModules.Commands;
using ChalkAndSteel.Services;
using UnityEngine;
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

        // === Ссылки на сцену (настраиваются в Inspector) ===
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _moveDelay = 0.2f; // Задержка между движениями

        // === Состояния ===
        private bool _isInputEnabled = true;
        private DualLayerTile[,] _currentRoomGrid;
        private float _lastMoveTime = 0f; // Время последнего движения

        // === Внедрение зависимостей ===
        [Inject]
        public void Construct(
            IInputService inputService,
            ICommandService commandService)
        {
            _inputService = inputService;
            _commandService = commandService;
        }

        // === Инициализация ===
        private void Awake()
        {
            ValidateSceneReferences();
        }

        private void Start()
        {
            // Инициализируем командный сервис с данными игрока
            // В начале может не быть сетки комнаты, поэтому инициализируем с null
            _commandService.InitializePlayerReceiver(_playerTransform, null, GetRoomGridAsIntArray(_currentRoomGrid));
            
            Debug.Log($"GridMovementHandler.Start(): Initialized with player transform at {_playerTransform.position}, " +
                      $"current room grid is {(GetRoomGridAsIntArray(_currentRoomGrid) != null ? "available" : "not available")}");
        }

        private void OnDestroy()
        {
            // Очистка подписок, если необходимо
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
        }

        /// <summary>
        /// Устанавливает сетку текущей комнаты
        /// </summary>
        /// <param name="roomGrid">Сетка комнаты с тайлами</param>
        public void SetCurrentRoomGrid(DualLayerTile[,] roomGrid)
        {
            _currentRoomGrid = roomGrid;
            
            // Преобразуем DualLayerTile[,] в int[,] для командного сервиса
            int[,] intGrid = GetRoomGridAsIntArray(roomGrid);
            
            // Пересоздаем командный сервис с новой информацией о комнате
            _commandService.InitializePlayerReceiver(_playerTransform, null, intGrid);
            
            Debug.Log($"SetCurrentRoomGrid called: roomGrid dimensions = ({roomGrid.GetLength(0)}x{roomGrid.GetLength(1)}), " +
                      $"converted intGrid dimensions = ({(intGrid != null ? $"{intGrid.GetLength(0)}x{intGrid.GetLength(1)}" : "null")}), " +
                      $"player position = {_playerTransform.position}");
        }

        /// <summary>
        /// Преобразует DualLayerTile[,] в int[,] для использования в командном сервисе
        /// </summary>
        /// <param name="tileGrid">Сетка тайлов</param>
        /// <returns>Целочисленная сетка, где 1 - проходимо, 0 - непроходимо</returns>
        private int[,] GetRoomGridAsIntArray(DualLayerTile[,] tileGrid)
        {
            if (tileGrid == null) return null;

            int width = tileGrid.GetLength(0);
            int height = tileGrid.GetLength(1);
            int[,] intGrid = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Проверяем проходимость тайла: если базовый тайл проходим, то клетка проходима
                    if (tileGrid[x, y]?.Base?.IsPassable == true)
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

        public void EnableInput() => _isInputEnabled = true;
        public void DisableInput() => _isInputEnabled = false;
        public void SetPlayerPosition(Vector3 position)
        {
            _playerTransform.position = position;
        }
    }
}
