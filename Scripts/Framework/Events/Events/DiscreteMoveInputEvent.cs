using UnityEngine;

namespace Architecture.GlobalModules.Events
{
    /// <summary>
    /// Событие дискретного ввода движения (например, пошаговое перемещение)
    /// </summary>
    public class DiscreteMoveInputEvent
    {
        /// <summary>
        /// Направление движения
        /// </summary>
        public Vector3Int Direction { get; }

        /// <summary>
        /// Конструктор события
        /// </summary>
        /// <param name="direction">Направление движения</param>
        public DiscreteMoveInputEvent(Vector3Int direction)
        {
            Direction = direction;
        }
    }
}