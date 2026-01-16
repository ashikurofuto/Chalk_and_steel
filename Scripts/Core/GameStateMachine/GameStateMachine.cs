using System;
using System.Collections.Generic;
using Core.StateMachine;

namespace Architecture.GlobalModules
{

    /// <summary>
    /// Глобальный модуль уровня 1 для управления состояниями игры.
    /// Реализует конечный автомат с публикацией событий о смене состояний.
    /// Изолирован от Unity API для тестируемости.
    /// </summary>
    public sealed class GameStateMachine : IGameStateMachine
    {
        private readonly IEventBus _eventBus;
        private readonly Dictionary<StateType, BaseState> _states;
        private BaseState _currentState;

        /// <summary>
        /// Конструктор машины состояний.
        /// Инициализирует все возможные состояния игры.
        /// </summary>
        public GameStateMachine(
            IEventBus eventBus,
            MainMenuState mainMenuState,
            HubState hubState,
            GameplayState gameplayState,
            PauseState pauseState,
            GameOverState gameOverState,
            LoadingState loadingState)
        {
            _eventBus = eventBus;

            // Инициализация всех состояний
            _states = new Dictionary<StateType, BaseState>
            {
                { StateType.MainMenu, mainMenuState },
                { StateType.Hub, hubState },
                { StateType.Gameplay, gameplayState },
                { StateType.Pause, pauseState },
                { StateType.GameOver, gameOverState },
                { StateType.Loading, loadingState }
            };
        }

        /// <summary>
        /// Переключает на состояние указанного типа.
        /// Вызывает Exit у текущего состояния и Enter у нового.
        /// Публикует GameStateChangedEvent через EventBus.
        /// </summary>
        /// <param name="stateType">Тип состояния для переключения</param>
        /// <exception cref="KeyNotFoundException">Если состояние не найдено</exception>
        private void ChangeState(StateType stateType)
        {
            if (!_states.TryGetValue(stateType, out var newState))
            {
                throw new KeyNotFoundException($"State with type {stateType} not found");
            }

            if (_currentState != null && _currentState.StateID == stateType)
            {
                // Уже в этом состоянии, ничего не делаем
                return;
            }

            var previousStateType = _currentState?.StateID ?? StateType.None;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();

            _eventBus.Publish(new GameStateChangedEvent(previousStateType, stateType));
        }

        /// <summary>
        /// Возвращает текущее активное состояние.
        /// Если состояние не установлено, возвращает null.
        /// </summary>
        public BaseState GetCurrentState() => _currentState;

        /// <summary>
        /// Возвращает тип текущего активного состояния.
        /// Если состояние не установлено, возвращает StateType.None.
        /// </summary>
        public StateType GetCurrentStateType() => _currentState?.StateID ?? StateType.None;

        /// <summary>
        /// Переключает на состояние главного меню
        /// </summary>
        public void StartMenuState() => ChangeState(StateType.MainMenu);

        /// <summary>
        /// Переключает на состояние хаба
        /// </summary>
        public void StartHubState() => ChangeState(StateType.Hub);

        /// <summary>
        /// Переключает на состояние геймплея
        /// </summary>
        public void StartGameplayState() => ChangeState(StateType.Gameplay);

        /// <summary>
        /// Переключает на состояние паузы
        /// </summary>
        public void StartPauseState() => ChangeState(StateType.Pause);

        /// <summary>
        /// Переключает на состояние экрана смерти
        /// </summary>
        public void StartGameOverState() => ChangeState(StateType.GameOver);

        /// <summary>
        /// Переключает на состояние загрузки
        /// </summary>
        public void StartLoadingState() => ChangeState(StateType.Loading);
    }
}