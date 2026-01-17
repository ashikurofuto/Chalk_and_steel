using Architecture.GlobalModules;
using UnityEngine;
using VContainer;



//BOOSTRAP SCENE 
public class test : MonoBehaviour
{
    private IGameStateMachine gameStateMachine;

    [Inject]
    public void Construct(IGameStateMachine gameStateMachine)
    {
        this.gameStateMachine = gameStateMachine;
    }

    private void Start()
    {
        gameStateMachine.StartMenuState();
    }

}
