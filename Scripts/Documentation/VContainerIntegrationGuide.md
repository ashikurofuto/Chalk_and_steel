# Интеграция с VContainer Dependency Injection

## Обзор

Данный документ описывает интеграцию системы пошагового передвижения на основе паттерна Command с VContainer для правильного управления зависимостями.

## Архитектура с VContainer

### Основные компоненты

- `CommandManager.cs` - центральный менеджер выполнения команд
- `TurnSystem.cs` - система управления очередностью ходов
- `GameManager.cs` - менеджер игрового процесса
- `RoomMoveCommand.cs` - команда перемещения для комнатной системы
- `RoomPlayerInputCommandHandler.cs` - обработчик ввода для комнатной системы
- `CommandManagerSetup.cs` - настройщик взаимодействия между менеджерами

## Регистрация компонентов

### В GameLifetimeScope

Добавьте регистрацию всех компонентов системы команд:

```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Регистрация существующих сервисов
        builder.Register<ProgressService>(Lifetime.Singleton).As<IProgressService>();
        builder.Register<PlayerService>(Lifetime.Singleton).As<IPlayerService>();
        // ... другие регистрации

        // 1. Регистрация менеджеров нашей командной системы
        builder.Register<CommandManager>(Lifetime.Singleton);
        builder.Register<TurnSystem>(Lifetime.Singleton);
        builder.Register<GameManager>(Lifetime.Singleton);
        
        // 2. Регистрация команд
        builder.Register<RoomMoveCommand>(Lifetime.Scoped);
        
        // 3. Регистрация обработчиков ввода
        builder.Register<RoomPlayerInputCommandHandler>(Lifetime.Scoped);
        
        // 4. Регистрация настройки взаимодействия между компонентами
        builder.RegisterEntryPoint<CommandManagerSetup>();
        
        // ... остальные регистрации
    }
}
```

## Внедрение зависимостей

### В обработчиках ввода

Используйте метод Construct для внедрения зависимостей:

```csharp
public class RoomPlayerInputCommandHandler : MonoBehaviour
{
    private CommandManager _commandManager;
    private TurnSystem _turnSystem;
    private IInputActionsWrapper _inputActions;

    [Inject]
    public void Construct(CommandManager commandManager, TurnSystem turnSystem, IInputActionsWrapper inputActions)
    {
        _commandManager = commandManager;
        _turnSystem = turnSystem;
        _inputActions = inputActions;
        
        // Подписываемся на изменение состояния игры
        _turnSystem.OnGameStateChange += OnGameStateChange;
        
        // Инициализируем систему ввода
        InitializeInputSystem();
    }
}
```

### В других компонентах

```csharp
public class PlayerMovementController : MonoBehaviour
{
    private CommandManager _commandManager;

    [Inject]
    public void Construct(CommandManager commandManager)
    {
        _commandManager = commandManager;
    }

    public void MovePlayer(Vector3Int direction, DualLayerTile[,] roomGrid)
    {
        var roomMoveCommand = new RoomMoveCommand(this, direction, roomGrid);
        _commandManager.ExecuteCommand(roomMoveCommand);
    }
}
```

## Настройка взаимодействия

Класс `CommandManagerSetup` автоматически настраивает взаимодействие между менеджерами:

```csharp
public class CommandManagerSetup : IStartable
{
    private readonly CommandManager _commandManager;
    private readonly GameManager _gameManager;

    public CommandManagerSetup(CommandManager commandManager, GameManager gameManager)
    {
        _commandManager = commandManager;
        _gameManager = gameManager;
    }

    public void Start()
    {
        // Подписываем GameManager на событие завершения хода от CommandManager
        _commandManager.OnPlayerTurnComplete += _gameManager.OnPlayerTurnComplete;
    }
}
```

## Использование команд

### Вместо статических вызовов

Было:
```csharp
CommandManager.Instance.ExecuteCommand(command);
```

Стало:
```csharp
// Команда должна быть внедрена через DI или получена из контекста
_commandManager.ExecuteCommand(command);
```

### Для комнатной системы

```csharp
public void MoveInRoom(Vector3Int direction, DualLayerTile[,] roomGrid)
{
    var roomMoveCommand = new RoomMoveCommand(this, direction, roomGrid);
    _commandManager.ExecuteCommand(roomMoveCommand);
}
```

## Проверка проходимости

Команда `RoomMoveCommand` автоматически проверяет проходимость через свойства `BaseTile.IsWalkable` в вашей системе `DualLayerTile`.

## Анимация перемещения

Анимация перемещения работает аналогично стандартной системе, но использует координаты вашей комнатной системы для вычисления целевых позиций.

## Запуск и тестирование

1. Убедитесь, что все компоненты зарегистрированы в соответствующем LifetimeScope
2. Проверьте, что зависимости правильно внедряются через `[Inject]` атрибуты
3. Протестируйте перемещение с помощью WASD или стрелок

Система полностью готова к использованию с VContainer Dependency Injection.