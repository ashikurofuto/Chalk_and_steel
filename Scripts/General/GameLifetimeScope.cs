using VContainer;
using VContainer.Unity;
using ChalkAndSteel.Services;
using Architecture.GlobalModules;
using ChalkAndSteel.UI;
using Architecture.GlobalModules.Systems;
using Architecture.GlobalModules.Handlers;
using Architecture.GlobalModules.Commands;

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

        // 3. Регистрация новой системы команд
        builder.Register<ICommandService, CommandService>(Lifetime.Scoped);
        
        // 4. Регистрация команд
        builder.Register<RoomMoveCommand>(Lifetime.Scoped);
        builder.Register<MoveCommand>(Lifetime.Scoped);
        
        // 5. Регистрация обработчиков ввода
        // builder.Register<RoomPlayerInputCommandHandler>(Lifetime.Scoped); // Убрано, так как это был старый обработчик
        builder.Register<GridMovementHandler>(Lifetime.Scoped);
        
        // 6. Регистрация обработчиков MonoBehaviour в иерархии
        builder.RegisterComponentInHierarchy<RoomSceneHandler>(); // <-- Добавлено
        builder.RegisterComponentInHierarchy<DungeonTestUI>();
    }
}