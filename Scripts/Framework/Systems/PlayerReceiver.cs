using UnityEngine;
using System.Collections;

namespace Architecture.GlobalModules.Systems
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
        private MonoBehaviour _monoBehaviour;
        private Coroutine _currentMoveCoroutine;

        // Делегат для уведомления о завершении перемещения
        private System.Action _onMoveCompleted;

        public PlayerReceiver(Transform transform, MonoBehaviour monoBehaviour, Grid grid = null, int[,] roomGrid = null, System.Action onMoveCompleted = null)
        {
            _transform = transform;
            _monoBehaviour = monoBehaviour;
            _grid = grid;
            _roomGrid = roomGrid;
            _lastPosition = _transform.position;
            _onMoveCompleted = onMoveCompleted;
        }

        public void MoveBetweenRooms(Vector3Int direction)
        {
            // Отменяем предыдущее перемещение, если оно есть
            if (_currentMoveCoroutine != null)
            {
                _monoBehaviour.StopCoroutine(_currentMoveCoroutine);
            }

            // Сохраняем текущую позицию перед перемещением
            _lastPosition = _transform.position;

            // Если сетка комнаты не установлена, просто двигаемся в направлении
            if (_roomGrid == null)
            {
                // В режиме без сетки просто двигаемся в указанном направлении
                Vector3 targetPosition = _transform.position + new Vector3(direction.x, direction.y, 0);
                _currentMoveCoroutine = _monoBehaviour.StartCoroutine(SmoothMoveTo(targetPosition, () =>
                {
                    Debug.Log($"Player moved without grid to: {targetPosition}");
                }));
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
                        _currentMoveCoroutine = _monoBehaviour.StartCoroutine(SmoothMoveTo(targetWorldPos, () =>
                        {
                            Debug.Log($"Player moved between rooms to: {targetWorldPos}");
                        }));
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
            // Отменяем предыдущее перемещение, если оно есть
            if (_currentMoveCoroutine != null)
            {
                _monoBehaviour.StopCoroutine(_currentMoveCoroutine);
            }

            // Сохраняем текущую позицию перед перемещением
            _lastPosition = _transform.position;

            // Если сетка не установлена, просто двигаемся в направлении
            if (_grid == null)
            {
                // В режиме без сетки просто двигаемся в указанном направлении
                Vector3 targetPosition = _transform.position + new Vector3(direction.x, direction.y, 0);
                _currentMoveCoroutine = _monoBehaviour.StartCoroutine(SmoothMoveTo(targetPosition, () =>
                {
                    Debug.Log($"Player moved without grid to: {targetPosition}");
                }));
            }
            else
            {
                Vector3Int currentPosition = _grid.WorldToCell(_transform.position);
                Vector3Int targetPosition = currentPosition + direction;

                Vector3 targetWorldPos = _grid.GetCellCenterWorld(targetPosition);
                _currentMoveCoroutine = _monoBehaviour.StartCoroutine(SmoothMoveTo(targetWorldPos, () =>
                {
                    Debug.Log($"Player moved in grid to: {targetWorldPos}");
                }));
            }
        }

        private float _moveDuration = 0.2f; // Время перемещения

        public void SetMoveDuration(float duration)
        {
            _moveDuration = duration;
        }

        private IEnumerator SmoothMoveTo(Vector3 targetPosition, System.Action onComplete = null)
        {
            Vector3 startPosition = _transform.position;
            float elapsed = 0f;

            while (elapsed < _moveDuration)
            {
                float progress = elapsed / _moveDuration;
                _transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Убедимся, что точно достигли цели
            _transform.position = targetPosition;

            // Вызов коллбэка по завершении
            onComplete?.Invoke();

            // Уведомляем о завершении перемещения
            _onMoveCompleted?.Invoke();
        }

        public void UndoMove()
        {
            if (_currentMoveCoroutine != null)
            {
                _monoBehaviour.StopCoroutine(_currentMoveCoroutine);
                _currentMoveCoroutine = null;
            }

            _transform.position = _lastPosition;
            Debug.Log($"Player move undone, returned to: {_lastPosition}");
        }

        public Vector3 GetPosition()
        {
            return _transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            if (_currentMoveCoroutine != null)
            {
                _monoBehaviour.StopCoroutine(_currentMoveCoroutine);
                _currentMoveCoroutine = null;
            }

            _transform.position = position;
        }

        public void MoveToWorldPosition(Vector3 worldPosition)
        {
            if (_currentMoveCoroutine != null)
            {
                _monoBehaviour.StopCoroutine(_currentMoveCoroutine);
                _currentMoveCoroutine = null;
            }

            // Сохраняем текущую позицию перед перемещением
            _lastPosition = _transform.position;

            // Плавно перемещаемся в новую позицию
            _currentMoveCoroutine = _monoBehaviour.StartCoroutine(SmoothMoveTo(worldPosition, () =>
            {
                Debug.Log($"Player moved to world position: {worldPosition}");
            }));
        }
    }
}