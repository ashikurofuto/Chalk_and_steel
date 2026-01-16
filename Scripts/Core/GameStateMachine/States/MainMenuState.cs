using System;

namespace Core.StateMachine
{
    /// <summary>
    /// Состояние главного меню игры.
    /// </summary>
    public class MainMenuState : BaseState
    {
        public MainMenuState() : base(StateType.MainMenu)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // Будущая логика: показать UI главного меню
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: скрыть UI главного меню
        }
    }
}