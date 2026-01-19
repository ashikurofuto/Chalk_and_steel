// Core/GameStateMachine/States/GameplayState.cs
using Architecture.GlobalModules;
using ChalkAndSteel.Services;

namespace Core.StateMachine
{
    /// <summary>
    /// Основное игровое состояние.
    /// </summary>
    public class GameplayState : BaseState
    {
        private readonly IEventBus _eventBus;


        public GameplayState(
            IEventBus eventBus,
            IPlayerService playerService) : base(StateType.Gameplay)
        {
            _eventBus = eventBus;

        }

        public override void Enter()
        {
            base.Enter();
        
        }

     
    }
}