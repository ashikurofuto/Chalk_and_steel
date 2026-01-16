using System;

namespace Core.StateMachine
{
    /// <summary>
    /// Базовый класс для всех состояний конечного автомата.
    /// Определяет жизненный цикл состояния без привязки к конкретному автомату.
    /// </summary>
    public abstract class BaseState
    {
        /// <summary>
        /// Уникальный идентификатор состояния.
        /// </summary>
        public readonly StateType StateID;

        /// <summary>
        /// Конструктор базового состояния.
        /// </summary>
        /// <param name="stateID">Идентификатор состояния</param>
        protected BaseState(StateType stateID)
        {
            StateID = stateID;
        }

        /// <summary>
        /// Вызывается при входе в состояние.
        /// </summary>
        public virtual void Enter()
        {
            // Базовая реализация пустая
        }

        /// <summary>
        /// Вызывается каждый кадр для обновления логики состояния.
        /// </summary>
        public virtual void Update()
        {
            // Базовая реализация пустая
        }

        /// <summary>
        /// Вызывается при выходе из состояния.
        /// </summary>
        public virtual void Exit()
        {
            // Базовая реализация пустая
        }
    }
}