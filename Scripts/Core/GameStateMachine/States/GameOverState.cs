namespace Core.StateMachine
{
    /// <summary>
    /// Состояние экрана завершения игры (смерть/проигрыш).
    /// </summary>
    public class GameOverState : BaseState
    {
        public GameOverState() : base(StateType.GameOver)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // Будущая логика: показать экран смерти, статистику
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: скрыть экран смерти
        }
    }
}