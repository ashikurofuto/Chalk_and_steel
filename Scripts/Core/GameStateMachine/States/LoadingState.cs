namespace Core.StateMachine
{
    /// <summary>
    /// Состояние загрузки (переход между сценами).
    /// </summary>
    public class LoadingState : BaseState
    {
        public LoadingState() : base(StateType.Loading)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // Будущая логика: показать экран загрузки
        }

        public override void Update()
        {
            base.Update();
            // Будущая логика: асинхронная загрузка сцены
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: скрыть экран загрузки
        }
    }
}