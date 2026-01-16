using Core.StateMachine;

namespace Architecture.GlobalModules
{
    /// <summary>
    /// Интерфейс машины состояний игры.
    /// Предоставляет методы для управления состояниями.
    /// Каждый метод соответствует конкретному состоянию для явного контроля.
    /// </summary>
    public interface IGameStateMachine
    {
        /// <summary>
        /// Возвращает текущее активное состояние
        /// </summary>
        BaseState GetCurrentState();

        /// <summary>
        /// Переключает на состояние главного меню
        /// </summary>
        void StartMenuState();

        /// <summary>
        /// Переключает на состояние хаба
        /// </summary>
        void StartHubState();

        /// <summary>
        /// Переключает на состояние геймплея
        /// </summary>
        void StartGameplayState();

        /// <summary>
        /// Переключает на состояние паузы
        /// </summary>
        void StartPauseState();

        /// <summary>
        /// Переключает на состояние экрана смерти
        /// </summary>
        void StartGameOverState();

        /// <summary>
        /// Переключает на состояние загрузки
        /// </summary>
        void StartLoadingState();
    }
}