using VContainer;
using VContainer.Unity;
using ChalkAndSteel.Services;
using Architecture.GlobalModules;
using ChalkAndSteel.UI;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Регистрация глобальных сервисов (Singleton)
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();

        // 2. Регистрация менеджеров как сервисов
        // builder.Register<ProgressService>(Lifetime.Singleton).As<IProgressService>(); // Закомментировано, если не существует
        builder.Register<PlayerService>(Lifetime.Singleton).As<IPlayerService>();
        builder.Register<InspectorDungeonGenerator>(Lifetime.Singleton).As<IDungeonGenerationService>();
        builder.Register<PathfindingValidator>(Lifetime.Singleton).As<IPathfindingValidator>();
        builder.Register<RoomGenerationService>(Lifetime.Singleton).As<IRoomGenerationService>();
        builder.Register<RoomService>(Lifetime.Singleton).As<IRoomService>(); // <-- Добавлено
        builder.Register<RoomGraphGenerator>(Lifetime.Singleton).AsSelf(); // <-- Добавлено для использования в генерации

        // 4. Регистрация обработчиков MonoBehaviour в иерархии
        builder.RegisterComponentInHierarchy<RoomSceneHandler>(); // <-- Добавлено
        builder.RegisterComponentInHierarchy<DungeonTestUI>();
    }
}