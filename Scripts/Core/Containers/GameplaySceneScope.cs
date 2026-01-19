using ChalkAndSteel.Services;
using VContainer;
using VContainer.Unity;

public class GameplaySceneScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
       /* builder.Register<DungeonConfigService>(Lifetime.Singleton)
          .As<IDungeonConfigService>();
        builder.Register<DungeonGenerationService>(Lifetime.Singleton)
          .As<IDungeonGenerationService>();
       */
    }
}