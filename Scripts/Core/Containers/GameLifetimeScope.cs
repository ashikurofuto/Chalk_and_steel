using VContainer;
using VContainer.Unity;
using ChalkAndSteel.Services;
using Architecture.GlobalModules;
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

        // Регистрация сервиса генерации комнат
        builder.Register<RoomGenerationService>(Lifetime.Singleton).As<IRoomGeneratorService>();


        // 3. Регистрация новой системы команд
        builder.Register<ICommandService, CommandService>(Lifetime.Scoped);
        
        // 4. Регистрация команд
        builder.Register<RoomMoveCommand>(Lifetime.Scoped);
        builder.Register<MoveCommand>(Lifetime.Scoped);
        
        // 5. Регистрация обработчиков ввода
        // builder.Register<RoomPlayerInputCommandHandler>(Lifetime.Scoped); // Убрано, так как это был старый обработчик
        builder.Register<GridMovementHandler>(Lifetime.Scoped);
        
        // 6. Регистрация UI контроллеров
        builder.Register<RoomTransitionUIController>(Lifetime.Scoped);


    }
}