using System;
using UnityEngine.SceneManagement;

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
            SceneManager.LoadScene(1);
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: скрыть UI главного меню
        }
    }
}