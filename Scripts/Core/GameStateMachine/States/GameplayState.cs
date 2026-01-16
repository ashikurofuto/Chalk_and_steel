namespace Core.StateMachine
{
    /// <summary>
    /// Основное игровое состояние (геймплей в подземелье).
    /// </summary>
    public class GameplayState : BaseState
    {
        public GameplayState() : base(StateType.Gameplay)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // Будущая логика: загрузить комнату, активировать игрока
        }

        public override void Update()
        {
            base.Update();
            // Будущая логика: обновление геймплея
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: выгрузить комнату
        }
    }
}