## Правила
1.Все зависимости — через конструктор (C#) или [Inject] (MonoBehaviour).
2.Общение между модулями — через IEventBus.
3.MonoBehaviour — только для визуала и сбора ввода.
3.Сервисы — чистая логика без ссылок на Unity.
/// <summary>
/// Шаблон для создания нового события.
/// Описывает одно значимое изменение или действие в системе.
/// </summary>
public record [Имя]Event // Пример: PlayerHealthChangedEvent
{
    // Используйте свойства только для чтения (get;) для иммутабельности
    public ТипСвойства PropertyName { get; }
    public ТипСвойства AnotherProperty { get; }
    // Конструктор для инициализации всех свойств
    public [Имя]Event(ТипСвойства propertyName, ТипСвойства anotherProperty)
    {
        PropertyName = propertyName;
        AnotherProperty = anotherProperty;
    }
}
/// <summary>
/// Шаблон интерфейса сервиса.
/// Описывает контракт, который реализует конкретный сервис.
/// </summary>
public interface I[Имя]Service // Пример: IInventoryService, IProgressService
{
    // Определите публичные методы и свойства здесь.
    // Избегайте конкретных типов Unity в публичном API, где это возможно.
    ReturnType PerformAction(ParameterType parameter);
    event Action<RelevantEvent> OnSomethingHappened;
}
/// <summary>
/// Шаблон реализации сервиса.
/// Содержит бизнес-логику. Должен быть чистым C# (без MonoBehaviour).
/// </summary>
public class [Имя]Service : I[Имя]Service // Пример: ProgressService : IProgressService
{
    // 1. Зависимости - приватные поля readonly
    private readonly IEventBus _eventBus;
    private readonly IOtherService _dependency;
    // 2. Конструктор для инъекции зависимостей
    public ProgressService(
        IEventBus eventBus,
        IOtherService dependency)
    {
        // Валидация входных данных
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(dependency);

        _eventBus = eventBus;
        _dependency = dependency;

        // 3. Подписка на события при инициализации (опционально)
        _eventBus.Subscribe<RelevantEvent>(OnRelevantEvent);
    }
    // 4. Публичные методы, реализующие интерфейс
    public ReturnType PerformAction(ParameterType parameter)
    {
        // Основная логика
        // Публикация событий о результатах
        _eventBus.Publish(new ActionPerformedEvent(parameter));
    }
    // 5. Приватные методы-обработчики
    private void OnRelevantEvent(RelevantEvent e)
    {
        // Логика реакции на событие
    }
    // 6. Приватные вспомогательные методы
    private ResultType CalculateSomething(InputType input)
    {
        // Чистая, тестируемая логика
    }
}
/// <summary>
/// Шаблон для создания нового состояния игры.
/// Наследуйте от BaseGameState для автоматической публикации GameStateChangedEvent.
/// </summary>
public class [Имя]State : BaseGameState // Пример: DialogueState, ShopState
{
    public override GameStateType StateType => GameStateType.[НовыйТип]; // Нужно добавить enum
    // Возможные зависимости состояния
    private readonly IUIService _uiService;
    // Конструктор
    public DialogueState(
        IEventBus eventBus,
        IUIService uiService) : base(eventBus)
    {
        _uiService = uiService;
    }
    // Переопределите Enter для настройки при входе в состояние
    public override void Enter()
    {
        base.Enter(); // Важно! Публикует базовое событие
        // Ваша логика: показать UI, запустить таймер, загрузить данные
        _uiService.ShowDialogueWindow();
    }
    // Переопределите Exit для очистки при выходе из состояния
    public override void Exit()
    {
        base.Exit(); // Важно!
        // Ваша логика: скрыть UI, остановить процессы
        _uiService.HideDialogueWindow();
    }
}

using VContainer;
using VContainer.Unity;

/// <summary>
/// Установщик зависимостей для игры Chalk and Steel.
/// Наследуется от LifetimeScope, переопределяет Configure.
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Регистрация глобальных сервисов (Singleton)
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.Register<GameStateMachine>(Lifetime.Singleton).As<IGameStateMachine>();
        
        // 2. Регистрация менеджеров как сервисов
        builder.Register<ProgressService>(Lifetime.Singleton).As<IProgressService>();
        builder.Register<PlayerService>(Lifetime.Singleton).As<IPlayerService>();
        builder.Register<RoomService>(Lifetime.Singleton).As<IRoomService>();
        
        // 3. Регистрация всех состояний игры
        builder.Register<MenuState>(Lifetime.Singleton);
        builder.Register<GameplayState>(Lifetime.Singleton);
        builder.Register<PauseState>(Lifetime.Singleton);
        builder.Register<DeathScreenState>(Lifetime.Singleton);
        
        // 4. Регистрация обработчиков MonoBehaviour в иерархии
        // (Зарегистрируйте здесь компоненты, которые находятся на сцене)
        builder.RegisterComponentInHierarchy<PlayerViewHandler>();
        builder.RegisterComponentInHierarchy<UIMainMenuHandler>();
        builder.RegisterComponentInHierarchy<CameraController>();
        
        // 5. Регистрация фабрик, если необходимы
        builder.RegisterFactory<EnemyConfig, IEnemyAI>(resolver =>
        {
            return config => new EnemyAI(config, resolver.Resolve<IEventBus>());
        }, Lifetime.Scoped);
    }
}


using UnityEngine;
using VContainer;

/// <summary>
/// Обработчик [краткое назначение]. Только связывает Unity события с сервисами/EventBus.
/// </summary>
public class [Имя]Handler : MonoBehaviour // Пример: DoorInteractionHandler
{
    // === ЗАВИСИМОСТИ (инжектятся) ===
    private IEventBus _eventBus;
    private I[Соответствующий]Service _service; // Пример: IInteractionService

    // === ССЫЛКИ НА СЦЕНУ (настраиваются в инспекторе) ===
    [SerializeField] private Collider _interactionCollider;
    [SerializeField] private UnityEngine.UI.Button _actionButton;

    // === ИНЖЕКЦИЯ ===
    [Inject]
    private void Construct(IEventBus eventBus, I[Соответствующий]Service service)
    {
        _eventBus = eventBus;
        _service = service;
    }

    // === ПОДПИСКА/ОТПИСКА НА EVENTBUS ===
    private void OnEnable() => _eventBus.Subscribe<[RelevantEvent]>(On[RelevantEvent]);
    private void OnDisable() => _eventBus.Unsubscribe<[RelevantEvent]>(On[RelevantEvent]);

    // === ОБРАБОТКА UI СОБЫТИЙ ===
    private void Start() => _actionButton?.onClick.AddListener(OnActionButtonClicked);
    private void OnActionButtonClicked() => _service.PerformAction(); // Или _eventBus.Publish(...)

    // === ОБРАБОТКА ФИЗИКИ ===
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _service.HandleInteraction(); // Или _eventBus.Publish(new InteractionTriggeredEvent(...))
    }

    // === ОБРАБОТЧИКИ СОБЫТИЙ ОТ EVENTBUS ===
    private void On[RelevantEvent]([RelevantEvent] e)
    {
        // Реакция на событие из системы (например, обновить визуал)
        UpdateVisualState(e.SomeData);
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (ТОЛЬКО ПРЕДСТАВЛЕНИЕ) ===
    private void UpdateVisualState(object data) { /* анимация, звук, включение/выключение */ }
}