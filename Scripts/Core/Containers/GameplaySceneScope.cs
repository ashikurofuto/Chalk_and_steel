using ChalkAndSteel.Handlers;
using ChalkAndSteel.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplaySceneScope : LifetimeScope
{


    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация сервиса генерации подземелья (только на сцене геймплея)
        builder.Register<DungeonGenerationService>(Lifetime.Singleton)
            .As<IDungeonGenerationService>();

    }
}