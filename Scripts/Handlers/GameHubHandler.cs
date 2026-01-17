using Architecture.GlobalModules;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer;
using UnityEngine.InputSystem;

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
        private InputSystem_Actions _inputActions;

        // === ССЫЛКИ НА СЦЕНУ (настраиваются в инспекторе) ===
        [SerializeField] private Tilemap _groundTilemap;
        [SerializeField] private Tilemap _borderTilemap;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _moveSpeed = 5f;

        // === ВНУТРЕННИЕ СОСТОЯНИЯ ===
        private Vector3Int _targetGridPosition;
        private Vector3 _targetWorldPosition;
        private bool _isMoving;
        private Vector3Int _startPosition = Vector3Int.zero;

        // === ИНЖЕКЦИЯ ===
        [Inject]
        private void Construct(
            IEventBus eventBus,
            IHubMovementService movementService)
        {
            _eventBus = eventBus;
            _movementService = movementService;
        }

        // === ИНИЦИАЛИЗАЦИЯ ===
        private void Awake()
        {
            _movementService.Initialize(_groundTilemap, _borderTilemap, GetGridPositionFromWorld(_playerTransform.position));
            ValidateSceneReferences();
            InitializeInputSystem();
            InitializePlayerPosition();
        }

        private void OnEnable() => EnableInput();
        private void OnDisable() => DisableInput();

        // === ОСНОВНОЙ ЦИКЛ ===
        private void Update()
        {
            HandleMovementAnimation();
        }

        // === ОБРАБОТКА ВВОДА ===
        private void InitializeInputSystem()
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.Move.started += OnMoveStarted;
        }

        private void EnableInput() => _inputActions.Player.Enable();
        private void DisableInput() => _inputActions.Player.Disable();

        private void OnMoveStarted(InputAction.CallbackContext context)
        {
            if (_isMoving) return;

            Vector2 inputDirection = context.ReadValue<Vector2>();
            Vector3Int moveDirection = GetGridDirectionFromInput(inputDirection);

            if (moveDirection == Vector3Int.zero) return;

            Vector3Int currentPosition = _movementService.GetCurrentPosition();
            Vector3Int targetPosition = currentPosition + moveDirection;

            AttemptMove(targetPosition);
            Debug.Log("Sdelal shag");
        }

        // === ЛОГИКА ПЕРЕМЕЩЕНИЯ ===
        private void AttemptMove(Vector3Int targetPosition)
        {
            if (!_movementService.CanMoveTo(targetPosition)) return;

            Vector3Int previousPosition = _movementService.GetCurrentPosition();

            _movementService.MoveTo(targetPosition);
            _targetGridPosition = targetPosition;
            _targetWorldPosition = _groundTilemap.GetCellCenterWorld(targetPosition);
            _isMoving = true;

            _eventBus.Publish(new PlayerMovedEvent(previousPosition, targetPosition));
        }

        // === АНИМАЦИЯ ПЕРЕМЕЩЕНИЯ ===
        private void HandleMovementAnimation()
        {
            //if (!_isMoving) return;

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
            // Преобразуем аналоговый ввод в дискретные направления
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector3Int.right : Vector3Int.left;
            }
            else if (Mathf.Abs(input.y) > 0)
            {
                return input.y > 0 ? Vector3Int.up : Vector3Int.down;
            }

            return Vector3Int.zero;
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
        // === ОБРАБОТЧИКИ СОБЫТИЙ (опционально) ===
        private void OnPlayerMoved(PlayerMovedEvent e)
        {
            // Можно добавить реакцию на событие перемещения
            // Например: проиграть звук шагов, обновить UI и т.д.
        }
    }
}