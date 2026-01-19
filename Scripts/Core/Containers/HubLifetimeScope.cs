using Architecture.GlobalModules;
using VContainer;
using VContainer.Unity;

public class HubLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация сервиса перемещения
        builder.Register<MovementService>(Lifetime.Scoped)
            .As<IMovementService>();
    }

   
}
