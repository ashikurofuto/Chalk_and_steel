using Core.StateMachine;

/// <summary>
/// —обытие изменени€ состо€ни€ игры.
/// ѕубликуетс€ при каждом успешном переходе между состо€ни€ми.
/// </summary>
public record GameStateChangedEvent
{
    public StateType PreviousState { get; }
    public StateType NewState { get; }

    public GameStateChangedEvent(StateType previousState, StateType newState)
    {
        PreviousState = previousState;
        NewState = newState;
    }
}
