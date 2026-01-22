using UnityEngine;

namespace Architecture.GlobalModules.Commands
{
    /// <summary>
    /// Команда перемещения между комнатами
    /// </summary>
    public class RoomMoveCommand : ICommand
    {
        private Vector3Int _direction;
        private PlayerReceiver _playerReceiver;

        /// <summary>
        /// Конструктор команды перемещения между комнатами
        /// </summary>
        /// <param name="playerReceiver">Объект, осуществляющий перемещение</param>
        /// <param name="direction">Направление перемещения</param>
        public RoomMoveCommand(PlayerReceiver playerReceiver, Vector3Int direction) 
        {
            this._playerReceiver = playerReceiver;
            this._direction = direction;
        }

        /// <summary>
        /// Выполняет команду перемещения между комнатами
        /// </summary>
        public void Execute()
        {
            _playerReceiver.MoveBetweenRooms(_direction);
        }

        /// <summary>
        /// Отменяет команду перемещения между комнатами
        /// </summary>
        public void Undo()
        {
            _playerReceiver.UndoMove();
        }
    }
}