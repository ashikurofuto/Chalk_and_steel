using Architecture.GlobalModules;
using Architecture.Services;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;

namespace Game.Handlers
{
    /// <summary>
    /// Обработчик движения в игровом хабе.
    /// Связывает ввод игрока с сервисом перемещения и визуальным представлением.
    /// </summary>
    public class GameHubHandler : MonoBehaviour
    {
        // === ЗАВИСИМОСТИ (инжектятся) ===
        private IEventBus _eventBus;
        private IHubMovementService _movementService;
        private IInputService _inputService;

        // === ССЫЛКИ НА СЦЕНУ (настраиваются в инспекторе) ===
        [SerializeField] private Tilemap _groundTilemap;
        [SerializeField] private Tilemap _borderTilemap;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _inputCooldown = 0.3f;

        // === ВНУТРЕННИЕ СОСТОЯНИЯ ===
        private Vector3Int _targetGridPosition;
        private Vector3 _targetWorldPosition;
        private bool _isMoving;
        private Vector3Int _startPosition = Vector3Int.zero;
        private Vector2 _lastInputDirection = Vector2.zero;
        private float _lastInputTime;
        private bool _isInputEnabled = true;

        // Флаги для предотвращения многократных вызовов на одно нажатие
        private bool _interactProcessed = false;
        private bool _inventoryProcessed = false;
        private bool _pauseProcessed = false;

        // === ИНЖЕКЦИЯ ===
        [Inject]
        private void Construct(
            IEventBus eventBus,
            IHubMovementService movementService,
            IInputService inputService)
        {
            _eventBus = eventBus;
            _movementService = movementService;
            _inputService = inputService;
        }

        // === ИНИЦИАЛИЗАЦИЯ ===
        private void Awake()
        {
            _movementService.Initialize(_groundTilemap, _borderTilemap, GetGridPositionFromWorld(_playerTransform.position));
            ValidateSceneReferences();
            InitializePlayerPosition();

            // Подписка на события от EventBus
            _eventBus.Subscribe<MoveInputEvent>(OnMoveInput);
        }

        private void OnEnable()
        {
            _inputService.Enable();
            _isInputEnabled = true;
        }

        private void OnDisable()
        {
            _inputService.Disable();
            _eventBus.Unsubscribe<MoveInputEvent>(OnMoveInput);
        }

        private void OnDestroy()
        {
            _eventBus.Unsubscribe<MoveInputEvent>(OnMoveInput);
        }

        // === ОСНОВНОЙ ЦИКЛ ===
        private void Update()
        {
            HandleMovementAnimation();

            // Опрос состояния кнопок с защитой от многократных вызовов
            CheckButtonInputs();
        }

        // === ОБРАБОТКА ВВОДА ЧЕРЕЗ EVENTBUS ===
        private void OnMoveInput(MoveInputEvent e)
        {
            if (!_isInputEnabled || _isMoving) return;

            if (e.Direction == Vector2.zero)
            {
                _lastInputDirection = Vector2.zero;
                return;
            }

            if (e.Direction == _lastInputDirection && Time.time - _lastInputTime < _inputCooldown)
                return;

            ProcessMovement(e.Direction);
        }

        // === ОПРОС КНОПОК С ЗАЩИТОЙ ОТ ПОВТОРНЫХ ВЫЗОВОВ ===
        private void CheckButtonInputs()
        {
            if (!_isInputEnabled) return;

            // Обработка взаимодействия (например, клавиша E)
            if (_inputService.IsInteractPressed())
            {
                if (!_interactProcessed)
                {
                    ProcessInteract();
                    _interactProcessed = true;
                }
            }
            else
            {
                _interactProcessed = false;
            }

            // Обработка инвентаря (например, клавиша Space)
            if (_inputService.IsInventoryPressed())
            {
                if (!_inventoryProcessed)
                {
                    ProcessInventory();
                    _inventoryProcessed = true;
                }
            }
            else
            {
                _inventoryProcessed = false;
            }

            // Обработка паузы (например, клавиша Escape)
            if (_inputService.IsPausePressed())
            {
                if (!_pauseProcessed)
                {
                    ProcessPause();
                    _pauseProcessed = true;
                }
            }
            else
            {
                _pauseProcessed = false;
            }
        }

        // === ОБРАБОТЧИКИ КНОПОК (вызываются только один раз на нажатие) ===
        private void ProcessInteract()
        {
            Debug.Log("Interact pressed in hub");
            _eventBus.Publish(new HubInteractionEvent(_movementService.GetCurrentPosition()));
        }

        private void ProcessInventory()
        {
            Debug.Log("Inventory opened in hub");
            _eventBus.Publish(new InventoryOpenedEvent());
        }

        private void ProcessPause()
        {
            Debug.Log("Pause pressed in hub");
            _eventBus.Publish(new GamePauseRequestedEvent());
        }

        // === ЛОГИКА ПЕРЕМЕЩЕНИЯ ===
        private void ProcessMovement(Vector2 inputDirection)
        {
            Vector3Int moveDirection = GetGridDirectionFromInput(inputDirection);
            if (moveDirection == Vector3Int.zero) return;

            Vector3Int currentPosition = _movementService.GetCurrentPosition();
            Vector3Int targetPosition = currentPosition + moveDirection;

            AttemptMove(targetPosition);

            _lastInputDirection = inputDirection;
            _lastInputTime = Time.time;
        }

        private void AttemptMove(Vector3Int targetPosition)
        {
            if (!_movementService.CanMoveTo(targetPosition)) return;

            Vector3Int previousPosition = _movementService.GetCurrentPosition();

            _movementService.MoveTo(targetPosition);
            _targetGridPosition = targetPosition;
            _targetWorldPosition = _groundTilemap.GetCellCenterWorld(targetPosition);
            _isMoving = true;

            _eventBus.Publish(new PlayerMovedEvent(previousPosition, targetPosition));
            Debug.Log($"Moved from {previousPosition} to {targetPosition}");
        }

        // === АНИМАЦИЯ ПЕРЕМЕЩЕНИЯ ===
        private void HandleMovementAnimation()
        {
            if (!_isMoving) return;

            _playerTransform.position = Vector3.MoveTowards(
                _playerTransform.position,
                _targetWorldPosition,
                _moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(_playerTransform.position, _targetWorldPosition) < 0.01f)
            {
                _playerTransform.position = _targetWorldPosition;
                _isMoving = false;
            }
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===
        private Vector3Int GetGridDirectionFromInput(Vector2 input)
        {
            float deadZone = 0.2f;
            if (input.magnitude < deadZone) return Vector3Int.zero;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector3Int.right : Vector3Int.left;
            }
            else
            {
                return input.y > 0 ? Vector3Int.up : Vector3Int.down;
            }
        }

        private void InitializePlayerPosition()
        {
            _targetGridPosition = _startPosition;
            _targetWorldPosition = _groundTilemap.GetCellCenterWorld(_startPosition);
            _playerTransform.position = _targetWorldPosition;
        }

        private void ValidateSceneReferences()
        {
            if (_groundTilemap == null)
                throw new System.NullReferenceException($"{nameof(_groundTilemap)} не назначен в инспекторе");

            if (_borderTilemap == null)
                throw new System.NullReferenceException($"{nameof(_borderTilemap)} не назначен в инспекторе");

            if (_playerTransform == null)
                throw new System.NullReferenceException($"{nameof(_playerTransform)} не назначен в инспекторе");
        }

        private Vector3Int GetGridPositionFromWorld(Vector3 worldPosition)
        {
            return _groundTilemap.WorldToCell(worldPosition);
        }

        // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ ===
        public void EnableInput() => _isInputEnabled = true;
        public void DisableInput() => _isInputEnabled = false;

        public void TeleportToPosition(Vector3Int gridPosition)
        {
            _movementService.MoveTo(gridPosition);
            _targetGridPosition = gridPosition;
            _targetWorldPosition = _groundTilemap.GetCellCenterWorld(gridPosition);
            _playerTransform.position = _targetWorldPosition;
            _isMoving = false;
        }
    }
}
