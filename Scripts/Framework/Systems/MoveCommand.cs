using UnityEngine;
using UnityEngine.Tilemaps;

namespace Architecture.GlobalModules.Systems
{
    /// <summary>
    /// Команда перемещения сущности
    /// </summary>
    public class MoveCommand : ICommand
    {
        private Vector3Int _direction;
        private PlayerReceiver _playerReceiver;

        /// <summary>
        /// Конструктор команды перемещения
        /// </summary>
        /// <param name="playerReceiver">Объект, осуществляющий перемещение</param>
        /// <param name="direction">Направление перемещения</param>
        public MoveCommand(PlayerReceiver playerReceiver, Vector3Int direction) 
        {
            this._playerReceiver = playerReceiver;
            this._direction = direction;
        }

        /// <summary>
        /// Выполняет команду перемещения
        /// </summary>
        public void Execute()
        {
            _playerReceiver.MoveInGrid(_direction);
        }

        /// <summary>
        /// Отменяет команду перемещения
        /// </summary>
        public void Undo()
        {
            _playerReceiver.UndoMove();
        }
    }
}