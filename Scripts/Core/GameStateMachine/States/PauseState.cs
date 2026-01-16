namespace Core.StateMachine
{
    /// <summary>
    /// Состояние паузы.
    /// </summary>
    public class PauseState : BaseState
    {
        public PauseState() : base(StateType.Pause)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // Будущая логика: остановить время, показать UI паузы
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: возобновить время, скрыть UI паузы
        }
    }
}