using UnityEngine;
using ChalkAndSteel.Services;

namespace Architecture.GlobalModules
{
    /// <summary>
    /// Сервис для перемещения сущностей в комнатной системе.
    /// Позволяет проверять возможность перемещения и выполнять перемещение.
    /// </summary>
    public sealed class RoomMovementService : IMovementService
    {
        private DualLayerTile[,] _roomGrid;
        private Vector3Int _currentPosition;

        /// <summary>
        /// Инициализирует сервис с комнатной сеткой и стартовой позицией.
        /// </summary>
        public void Initialize(DualLayerTile[,] roomGrid, Vector3Int startPosition)
        {
            _roomGrid = roomGrid;
            _currentPosition = startPosition;
        }

        /// <summary>
        /// Проверяет, можно ли переместиться в указанную позицию.
        /// </summary>
        public bool CanMoveTo(Vector3Int targetPosition)
        {
            // Проверяем, находится ли целевая позиция в пределах сетки
            if (targetPosition.x < 0 || targetPosition.x >= _roomGrid.GetLength(0) ||
                targetPosition.y < 0 || targetPosition.y >= _roomGrid.GetLength(1))
            {
                return false;
            }

            // Проверяем, является ли целевая клетка проходимой
            DualLayerTile targetTile = _roomGrid[targetPosition.x, targetPosition.y];
            if (targetTile != null && targetTile.Base != null)
            {
                // Проверяем, является ли базовый тайл проходимым (не стеной)
                return targetTile.Base.IsPassable;
            }

            return false;
        }

        /// <summary>
        /// Перемещает сущность в указанную позицию.
        /// </summary>
        public void MoveTo(Vector3Int targetPosition)
        {
            if (CanMoveTo(targetPosition))
            {
                _currentPosition = targetPosition;
            }
        }

        /// <summary>
        /// Возвращает текущую позицию сущности в сетке.
        /// </summary>
        public Vector3Int GetCurrentPosition() => _currentPosition;
    }
}