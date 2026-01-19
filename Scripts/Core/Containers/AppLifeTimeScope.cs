using Architecture.GlobalModules;
using Architecture.Services;
using ChalkAndSteel.Services;
using Core.StateMachine;
using VContainer;
using VContainer.Unity;

public class AppLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Регистрация глобальных сервисов
        builder.Register<EventBus>(Lifetime.Singleton)
            .As<IEventBus>();
        builder.Register<UnityInputActionsWrapper>(Lifetime.Singleton)
            .As<IInputActionsWrapper>();
        builder.Register<InputService>(Lifetime.Singleton)
            .As<IInputService>();

        // 2. Регистрация всех состояний как отдельных сервисов
        builder.Register<MainMenuState>(Lifetime.Singleton);
        builder.Register<HubState>(Lifetime.Singleton);
        builder.Register<GameplayState>(Lifetime.Singleton);
        builder.Register<PauseState>(Lifetime.Singleton);
        builder.Register<GameOverState>(Lifetime.Singleton);
        builder.Register<LoadingState>(Lifetime.Singleton);

        // 3. Регистрация машины состояний
        builder.Register<GameStateMachine>(Lifetime.Singleton)
            .As<IGameStateMachine>();

        builder.Register<PlayerService>(Lifetime.Singleton)
            .As<IPlayerService>();

        // Исправленная регистрация DungeonConfigService (Вариант 2)
   

    }
}