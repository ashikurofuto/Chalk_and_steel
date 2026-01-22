using System;
using UnityEngine;

namespace Architecture.GlobalModules.Systems
{
    /// <summary>
    /// Интерфейс системы управления очередностью ходов
    /// </summary>
    public interface ITurnSystem
    {
        /// <summary>
        /// Перечисление состояний игры
        /// </summary>
        public enum GameState
        {
            PlayerTurn,    // Ход игрока
            EnemyTurn,     // Ход врагов
            EventProcessing, // Обработка событий
            GameOver       // Игра окончена
        }

        /// <summary>
        /// Событие изменения состояния игры
        /// </summary>
        event System.Action<GameState> OnGameStateChange;

        /// <summary>
        /// Текущее состояние игры
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Вызывается при завершении хода игрока
        /// </summary>
        void OnPlayerTurnComplete();

        /// <summary>
        /// Проверяет условие поражения
        /// </summary>
        /// <returns>True, если игрок потерпел поражение</returns>
        bool CheckDefeatCondition();

        /// <summary>
        /// Проверяет условие победы
        /// </summary>
        /// <returns>True, если игрок одержал победу</returns>
        bool CheckVictoryCondition();
    }
}