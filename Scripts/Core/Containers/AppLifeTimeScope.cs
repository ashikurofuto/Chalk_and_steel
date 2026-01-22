using Architecture.GlobalModules;
using Architecture.GlobalModules.Commands;
using ChalkAndSteel.Services;
using Core.StateMachine;
using VContainer;
using VContainer.Unity;

public class AppLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Регистрация основных сервисов
        builder.Register<EventBus>(Lifetime.Singleton)
            .As<IEventBus>();
        
        builder.Register<InputService>(Lifetime.Singleton)
            .As<IInputService>();

        // 2. Регистрация состояний игры
        builder.Register<MainMenuState>(Lifetime.Singleton);
        builder.Register<HubState>(Lifetime.Singleton);
        builder.Register<GameplayState>(Lifetime.Singleton);
        builder.Register<PauseState>(Lifetime.Singleton);
        builder.Register<GameOverState>(Lifetime.Singleton);
        builder.Register<LoadingState>(Lifetime.Singleton);

        // 3. Регистрация машин состояний
        builder.Register<GameStateMachine>(Lifetime.Singleton)
            .As<IGameStateMachine>();

        builder.Register<PlayerService>(Lifetime.Singleton)
            .As<IPlayerService>();

        // Регистрация сервиса команд
        builder.Register<ICommandService, CommandService>(Lifetime.Singleton);
    
    }
}