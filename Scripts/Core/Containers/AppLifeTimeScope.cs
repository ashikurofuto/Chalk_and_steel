using Architecture.GlobalModules;
using Core.StateMachine;
using VContainer;
using VContainer.Unity;

public class AppLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Регистрация глобальных сервисов
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        // 2. Регистрация всех состояний как отдельных сервисов
        builder.Register<MainMenuState>(Lifetime.Singleton);
        builder.Register<HubState>(Lifetime.Singleton);
        builder.Register<GameplayState>(Lifetime.Singleton);
        builder.Register<PauseState>(Lifetime.Singleton);
        builder.Register<GameOverState>(Lifetime.Singleton);
        builder.Register<LoadingState>(Lifetime.Singleton);
        // 3. Регистрация машины состояний
        builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();
    }
}
