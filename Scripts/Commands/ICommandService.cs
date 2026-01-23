using UnityEngine;

namespace Architecture.GlobalModules.Commands
{
    /// <summary>
    /// Интерфейс для сервиса команд, через который будут передаваться данные для PlayerReceiver
    /// </summary>
    public interface ICommandService
    {
        /// <summary>
        /// Инициализирует PlayerReceiver с необходимыми данными
        /// </summary>
        /// <param name="transform">Трансформ игрока</param>
        /// <param name="grid">Сетка для перемещения в одной комнате</param>
        /// <param name="roomGrid">Массив комнат для перемещения между комнатами</param>
        void InitializePlayerReceiver(Transform transform, Grid grid = null, int[,] roomGrid = null);

        /// <summary>
        /// Выполняет команду перемещения в сетке
        /// </summary>
        /// <param name="direction">Направление перемещения</param>
        void ExecuteMoveCommand(Vector3Int direction);

        /// <summary>
        /// Выполняет команду перемещения между комнатами
        /// </summary>
        /// <param name="direction">Направление перемещения</param>
        void ExecuteRoomMoveCommand(Vector3Int direction);

        /// <summary>
        /// Отменяет последнюю команду
        /// </summary>
        void UndoLastCommand();

        /// <summary>
        /// Проверяет, можно ли выполнить перемещение в заданном направлении
        /// </summary>
        /// <param name="direction">Направление для проверки</param>
        /// <returns>True, если перемещение возможно</returns>
        bool CanMoveTo(Vector3Int direction);

        /// <summary>
        /// Перемещает игрока в заданную мировую позицию
        /// </summary>
        /// <param name="worldPosition">Мировая позиция для перемещения</param>
        void MovePlayerToWorldPosition(Vector3 worldPosition);
    }
}