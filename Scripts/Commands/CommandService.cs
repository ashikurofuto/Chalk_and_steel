using UnityEngine;

namespace Architecture.GlobalModules.Commands
{
    /// <summary>
    /// Сервис для управления командами перемещения игрока
    /// </summary>
    public class CommandService : ICommandService
    {
        private CommandInvoker _invoker;
        private PlayerReceiver _playerReceiver;
        private Transform _playerTransform;
        private Grid _grid;
        private int[,] _roomGrid;

        public CommandService()
        {
            _invoker = new CommandInvoker();
        }

        public void MovePlayerToWorldPosition(Vector3 worldPosition)
        {
            if (_playerReceiver != null)
            {
                _playerReceiver.MoveToWorldPosition(worldPosition);
            }
            else
            {
                Debug.LogWarning("Cannot move player to world position - PlayerReceiver is null");
            }
        }

        /// <summary>
        /// Инициализирует PlayerReceiver с необходимыми данными
        /// </summary>
        /// <param name="transform">Трансформ игрока</param>
        /// <param name="grid">Сетка для перемещения в одной комнате</param>
        /// <param name="roomGrid">Массив комнат для перемещения между комнатами</param>
        public void InitializePlayerReceiver(Transform transform, Grid grid = null, int[,] roomGrid = null)
        {
            _playerTransform = transform;
            _grid = grid;
            _roomGrid = roomGrid;
            _playerReceiver = new PlayerReceiver(transform, grid, roomGrid);
            Debug.Log("PlayerReceiver initialized with transform and grids");
        }

        /// <summary>
        /// Выполняет команду перемещения (автоматически выбирает между комнатами и сеткой)
        /// </summary>
        /// <param name="direction">Направление перемещения</param>
        public void ExecuteMoveCommand(Vector3Int direction)
        {
            Debug.Log($"ExecuteMoveCommand called with direction: {direction}");
            if (_playerReceiver != null)
            {
                ICommand moveCommand;
                
                // Выбираем тип команды в зависимости от доступных сеток
                if (_roomGrid != null)
                {
                    Debug.Log("Using RoomMoveCommand with room grid");
                    moveCommand = new RoomMoveCommand(_playerReceiver, direction);
                }
                else if (_grid != null)
                {
                    Debug.Log("Using MoveCommand with grid");
                    moveCommand = new MoveCommand(_playerReceiver, direction);
                }
                else
                {
                    Debug.LogWarning("No grid available, using RoomMoveCommand as fallback");
                    moveCommand = new RoomMoveCommand(_playerReceiver, direction);
                }
                
                _invoker.SetCommand(moveCommand);
                _invoker.ExecuteCommand();
                Debug.Log($"Move command executed for direction: {direction}");
            }
            else
            {
                Debug.LogWarning("Cannot execute move command - PlayerReceiver is null");
            }
        }

        /// <summary>
        /// Выполняет команду перемещения между комнатами (явно)
        /// </summary>
        /// <param name="direction">Направление перемещения</param>
        public void ExecuteRoomMoveCommand(Vector3Int direction)
        {
            Debug.Log($"ExecuteRoomMoveCommand called with direction: {direction}");
            if (_playerReceiver != null)
            {
                RoomMoveCommand roomMoveCommand = new RoomMoveCommand(_playerReceiver, direction);
                _invoker.SetCommand(roomMoveCommand);
                _invoker.ExecuteCommand();
                Debug.Log($"Room move command executed for direction: {direction}");
            }
            else
            {
                Debug.LogWarning("Cannot execute room move command - PlayerReceiver is null");
            }
        }

        /// <summary>
        /// Отменяет последнюю команду
        /// </summary>
        public void UndoLastCommand()
        {
            Debug.Log("UndoLastCommand called");
            _invoker.UndoLastCommand();
            Debug.Log("Last command undone");
        }

        /// <summary>
        /// Проверяет, можно ли выполнить перемещение в заданном направлении
        /// </summary>
        /// <param name="direction">Направление для проверки</param>
        /// <returns>True, если перемещение возможно</returns>
        public bool CanMoveTo(Vector3Int direction)
        {
            Debug.Log($"CanMoveTo called with direction: {direction}");
            if (_playerReceiver == null) 
            {
                Debug.LogWarning("Cannot move - PlayerReceiver is null");
                return false;
            }

            Debug.Log($"Grid state: _grid={_grid != null}, _roomGrid dimensions=({(_roomGrid != null ? $"{_roomGrid.GetLength(0)}x{_roomGrid.GetLength(1)}" : "null")})");

            // Проверка в зависимости от текущего типа перемещения
            if (_grid != null)
            {
                // Проверяем возможность перемещения в сетке
                Vector3Int currentPosition = _grid.WorldToCell(_playerTransform.position);
                Vector3Int targetPosition = currentPosition + direction;
                
                Debug.Log($"Checking move in grid from {currentPosition} to {targetPosition}");
                
                // Проверяем проходимость целевой ячейки
                // Здесь нужно получить тайлмап пола и стены и проверить, проходима ли целевая ячейка
                // Например:
                // bool isTargetPassable = floorTilemap.HasTile(targetPosition) && !wallTilemap.HasTile(targetPosition);
                // bool result = isTargetPassable;
                // Но поскольку у нас нет прямого доступа к тайлмапам, мы можем использовать _roomGrid, если он доступен
                // или использовать другой способ проверки проходимости

                // Заглушка: предположим, что _roomGrid содержит информацию о проходимости
                // и что _roomGrid синхронизирован с тайлмапами
                if (_roomGrid != null &&
                    targetPosition.x >= 0 && targetPosition.x < _roomGrid.GetLength(0) &&
                    targetPosition.y >= 0 && targetPosition.y < _roomGrid.GetLength(1))
                {
                    bool result = _roomGrid[targetPosition.x, targetPosition.y] != 0;
                    Debug.Log($"Grid move result (using _roomGrid): {result} at position ({targetPosition.x}, {targetPosition.y})");
                    return result;
                }
                else
                {
                    // Если _roomGrid не доступен, предполагаем, что все проходимо (не идеально, но лучше, чем всегда true)
                    // В идеале, здесь должна быть проверка через тайлмапы
                    Debug.LogWarning("CanMoveTo: _roomGrid not available for grid-based move validation, assuming passable");
                    return true;
                }
            }
            else if (_roomGrid != null)
            {
                // Проверяем возможность перемещения между комнатами
                Vector3Int currentPosition = new Vector3Int(Mathf.RoundToInt(_playerTransform.position.x),
                                                          Mathf.RoundToInt(_playerTransform.position.z), 0);
                Vector3Int targetPosition = currentPosition + direction;

                Debug.Log($"Checking move in room grid from {currentPosition} to {targetPosition}");

                if (targetPosition.x >= 0 && targetPosition.x < _roomGrid.GetLength(0) &&
                    targetPosition.y >= 0 && targetPosition.y < _roomGrid.GetLength(1))
                {
                    bool result = _roomGrid[targetPosition.x, targetPosition.y] != 0;
                    Debug.Log($"Room grid move result: {result} at position ({targetPosition.x}, {targetPosition.y})");
                    return result;
                }
                else
                {
                    Debug.Log($"Target position {targetPosition} is out of room grid bounds (grid size: {_roomGrid.GetLength(0)}x{_roomGrid.GetLength(1)})");
                }
            }
            else
            {
                // Если сетки еще нет, но PlayerReceiver есть, даем возможность двигаться
                // Это может быть валидным в начале игры до загрузки комнаты
                Debug.LogWarning("Both _grid and _roomGrid are null - allowing movement as fallback behavior");
                Debug.Log("This might happen before room grid is loaded - movement allowed as fallback");
                return true; // Разрешаем перемещение как fallback, пока сетка не загружена
            }

            Debug.Log("CanMoveTo returning false - no valid grid or position out of bounds");
            return false;
        }
    }
}