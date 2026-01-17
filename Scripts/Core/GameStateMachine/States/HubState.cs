using UnityEngine.SceneManagement;

namespace Core.StateMachine
{
    /// <summary>
    /// Состояние игрового хаба (карта или база).
    /// </summary>
    public class HubState : BaseState
    {
        public HubState() : base(StateType.Hub)
        {
        }

        public override void Enter()
        {
            SceneManager.LoadScene(2);
        }

        public override void Exit()
        {
            base.Exit();
            // Будущая логика: выгрузить хаб
        }
    }
}