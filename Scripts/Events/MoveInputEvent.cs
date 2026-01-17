using UnityEngine;

namespace Architecture.Services
{
    /// <summary>
    /// Событие ввода движения игрока.
    /// Публикуется при изменении вектора движения.
    /// </summary>
    public record MoveInputEvent
    {
        public Vector2 Direction { get; }

        public MoveInputEvent(Vector2 direction)
        {
            Direction = direction;
        }
    }
}



