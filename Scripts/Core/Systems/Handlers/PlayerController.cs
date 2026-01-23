using UnityEngine;

namespace Architecture.GlobalModules.Systems
{
    /// <summary>
    /// Контроллер игрока для взаимодействия с интерактивными объектами
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private PlayerReceiver _playerReceiver;

        public int CurrentHardeningStage { get; set; } = 1;

        private void Start()
        {
            // Инициализируем PlayerReceiver с текущим трансформом
            _playerReceiver = new PlayerReceiver(transform);
        }

        public Vector2Int GetGridPosition()
        {
            // Возвращаем текущую позицию в сетке
            return new Vector2Int(
                Mathf.RoundToInt(transform.position.x),
                Mathf.RoundToInt(transform.position.y)
            );
        }

        /// <summary>
        /// Получает PlayerReceiver для использования в системе команд
        /// </summary>
        public PlayerReceiver GetPlayerReceiver()
        {
            return _playerReceiver;
        }
    }
}