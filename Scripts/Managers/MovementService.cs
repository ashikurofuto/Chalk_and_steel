using UnityEngine;
using UnityEngine.Tilemaps;

namespace Architecture.GlobalModules
{
    /// <summary>
    /// Сервис управления движением в хабе.
    /// Содержит логику проверки возможности перемещения по тайлмапу.
    /// </summary>
    public sealed class MovementService : IMovementService


    {
        private Tilemap _groundTilemap;
        private Tilemap _borderTilemap;
        private Vector3Int _currentPosition;

        public void Initialize(Tilemap groundTilemap, Tilemap borderTilemap, Vector3Int startPosition)
        {
            _groundTilemap = groundTilemap;
            _borderTilemap = borderTilemap;
            _currentPosition = startPosition;
        }

        /// <summary>
        /// Проверяет возможность перемещения на целевую позицию.
        /// </summary>
        public bool CanMoveTo(Vector3Int targetPosition)
        {
            // Проверяем наличие земли в целевой позиции
            bool hasGround = _groundTilemap.HasTile(targetPosition);

            // Проверяем отсутствие стены в целевой позиции
            bool hasBorder = _borderTilemap.HasTile(targetPosition);

            return hasGround && !hasBorder;
        }

        /// <summary>
        /// Перемещает игрока на целевую позицию.
        /// </summary>
        public void MoveTo(Vector3Int targetPosition)
        {
            _currentPosition = targetPosition;
        }

        /// <summary>
        /// Возвращает текущую позицию игрока в координатах тайлмапа.
        /// </summary>
        public Vector3Int GetCurrentPosition() => _currentPosition;
    }
}