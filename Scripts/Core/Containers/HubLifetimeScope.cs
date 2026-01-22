using Architecture.GlobalModules;
using VContainer;
using VContainer.Unity;

public class HubLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрируем новую реализацию для комнатной системы
        builder.Register<RoomMovementService>(Lifetime.Scoped)
            .As<IMovementService>();
    }
}
