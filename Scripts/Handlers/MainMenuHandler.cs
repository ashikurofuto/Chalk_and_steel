using Architecture.GlobalModules;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Button StartGameBtn;
    [SerializeField] private Button SettingsBtn;
    [SerializeField] private Button ExitBtn;

    private IGameStateMachine gameStateMachine;

    [Inject]
    public void Construct(IGameStateMachine gameStateMachine)
    {
        this.gameStateMachine = gameStateMachine;
    }

    private void Start()
    {
        StartGameBtn.onClick.AddListener(StartGameHub);
    }

    private void StartGameHub()
    {
        gameStateMachine.StartHubState();
    }
}
