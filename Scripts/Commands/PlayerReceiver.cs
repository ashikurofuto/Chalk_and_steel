using UnityEngine;

namespace Architecture.GlobalModules.Commands
{
    /// <summary>
    /// Receiver - получатель команд, в данном случае объект, который перемещается
    /// </summary>
    public class PlayerReceiver
    {
        private Transform _transform;
        private Grid _grid;
        private int[,] _roomGrid;
        private Vector3 _lastPosition;

        public PlayerReceiver(Transform transform, Grid grid = null, int[,] roomGrid = null)
        {
            _transform = transform;
            _grid = grid;
            _roomGrid = roomGrid;
            _lastPosition = _transform.position;
        }

        public void MoveBetweenRooms(Vector3Int direction)
        {
            // Сохраняем текущую позицию перед перемещением
            _lastPosition = _transform.position;
            
            // Если сетка комнаты не установлена, просто двигаемся в направлении
            if (_roomGrid == null)
            {
                // В режиме без сетки просто двигаемся в указанном направлении
                Vector3 targetPosition = _transform.position + new Vector3(direction.x, direction.y, 0);
                _transform.position = targetPosition;
                Debug.Log($"Player moved without grid to: {targetPosition}");
            }
            else
            {
                Vector3Int currentPosition = new Vector3Int(Mathf.RoundToInt(_transform.position.x),
                                                          Mathf.RoundToInt(_transform.position.y), 0);
                Vector3Int targetPosition = currentPosition + direction;

                // Проверяем, находится ли целевая позиция в пределах сетки
                if (targetPosition.x >= 0 && targetPosition.x < _roomGrid.GetLength(0) &&
                    targetPosition.y >= 0 && targetPosition.y < _roomGrid.GetLength(1))
                {
                    // Проверяем, можно ли переместиться в целевую комнату
                    if (_roomGrid[targetPosition.x, targetPosition.y] != 0)
                    {
                        Vector3 targetWorldPos = new Vector3(targetPosition.x, targetPosition.y, _transform.position.z);
                        _transform.position = targetWorldPos;
                        Debug.Log($"Player moved between rooms to: {targetWorldPos}");
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot move to {targetPosition} - cell is not passable");
                        // Откатываем позицию, если клетка непроходима
                        _transform.position = _lastPosition;
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning($"Cannot move to {targetPosition} - out of grid bounds");
                    // Откатываем позицию, если за пределами сетки
                    _transform.position = _lastPosition;
                    return;
                }
            }
        }

        public void MoveInGrid(Vector3Int direction)
        {
            // Сохраняем текущую позицию перед перемещением
            _lastPosition = _transform.position;
            
            // Если сетка не установлена, просто двигаемся в направлении
            if (_grid == null)
            {
                // В режиме без сетки просто двигаемся в указанном направлении
                Vector3 targetPosition = _transform.position + new Vector3(direction.x, direction.y, 0);
                _transform.position = targetPosition;
                Debug.Log($"Player moved without grid to: {targetPosition}");
            }
            else
            {
                Vector3Int currentPosition = _grid.WorldToCell(_transform.position);
                Vector3Int targetPosition = currentPosition + direction;
                
                Vector3 targetWorldPos = _grid.GetCellCenterWorld(targetPosition);
                _transform.position = targetWorldPos;
                Debug.Log($"Player moved in grid to: {targetWorldPos}");
            }
        }

        public void UndoMove()
        {
            _transform.position = _lastPosition;
            Debug.Log($"Player move undone, returned to: {_lastPosition}");
        }

        public Vector3 GetPosition()
        {
            return _transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        public void MoveToWorldPosition(Vector3 worldPosition)
        {
            // Сохраняем текущую позицию перед перемещением
            _lastPosition = _transform.position;

            // Устанавливаем новую позицию
            _transform.position = worldPosition;

            Debug.Log($"Player moved to world position: {worldPosition}");
        }
    }
}