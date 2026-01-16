namespace Core.StateMachine
{
    /// <summary>
    /// Типы состояний игры.
    /// </summary>
    public enum StateType
    {
        None = 0,
        MainMenu,
        Hub,
        Gameplay,
        Pause,
        GameOver,
        Loading,
    }
}