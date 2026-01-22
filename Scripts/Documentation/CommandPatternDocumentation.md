# Документация: Система пошагового передвижения на основе Command Pattern

## Обзор

Система реализует пошаговую игровую механику с использованием паттерна Command, обеспечивающего:

- Масштабируемость и поддерживаемость
- Поддержка истории команд
- Возможность отмены/повтора действий
- Сложное взаимодействие сущностей
- Пошаговую очередь выполнения

## Архитектура

### Компоненты системы

#### 1. Базовые интерфейсы и классы

- **ICommand** - Интерфейс команды с методами `CanExecute`, `Execute`, `Undo`, `Redo`
- **Command** - Абстрактный базовый класс команды с общими функциями

#### 2. Конкретные команды

- **MoveCommand** - Команда перемещения сущности (для тайлмапов)
- **RoomMoveCommand** - Команда перемещения сущности (для комнатной системы с DualLayerTile)
- **AttackCommand** - Команда атаки сущности
- **WaitCommand** - Команда пропуска хода
- **CompositeCommand** - Композитная команда для группировки нескольких команд

#### 3. Системы управления

- **CommandManager** - Управление выполнением команд
- **HistoryManager** - Управление историей команд
- **TurnSystem** - Система очередности ходов
- **CommandPool** - Пул команд для оптимизации

#### 4. Обработчики ввода

- **GridMovementHandler** - Основной обработчик движения по сетке с использованием комнатной системы (предпочтительный)

## Использование

### Создание и выполнение команды перемещения

Для тайлмапов:
```csharp
// В вашем компоненте
private CommandManager _commandManager;

[Inject]
public void Construct(CommandManager commandManager)
{
    _commandManager = commandManager;
}

public void MovePlayer(Vector3Int direction)
{
    var moveCommand = new MoveCommand(this, direction, grid, walkableTilemap);
    _commandManager.ExecuteCommand(moveCommand);
}
```

Для комнатной системы (ваша система):
```csharp
// В вашем компоненте
public void MovePlayerInRoom(Vector3Int direction, DualLayerTile[,] roomGrid)
{
    var roomMoveCommand = new RoomMoveCommand(this, direction, roomGrid);
    // CommandManager должен быть внедрен через Dependency Injection
    _commandManager.ExecuteCommand(roomMoveCommand); // где _commandManager внедрен через [Inject]
}
```

### Создание и выполнение композитной команды

```csharp
// Создание последовательности команд
var compositeCommand = new CompositeCommand();

compositeCommand.AddCommand(new MoveCommand(this, Vector3Int.right, grid, tilemap));
compositeCommand.AddCommand(new MoveCommand(this, Vector3Int.up, grid, tilemap));
compositeCommand.AddCommand(new WaitCommand(this));

_commandManager.ExecuteCommand(compositeCommand); // где _commandManager внедрен через [Inject]
```

### Отмена и повтор команд

```csharp
// Отмена последней команды
if (_commandManager.CanUndo)
{
    _commandManager.UndoLastCommand();
}

// Повтор последней отмененной команды
if (_commandManager.CanRedo)
{
    _commandManager.RedoLastCommand();
}
```

### Интеграция с системой ввода

Создайте компонент, который будет обрабатывать ввод и создавать соответствующие команды:

```csharp
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _walkableTilemap;

    private CommandManager _commandManager;

    [Inject]
    public void Construct(CommandManager commandManager)
    {
        _commandManager = commandManager;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            var moveCommand = new MoveCommand(this, Vector3Int.up, _grid, _walkableTilemap);
            _commandManager.ExecuteCommand(moveCommand);
        }
    }
}
```

## Интеграция с существующей архитектурой

### Связь с MovementService

Система команд может работать поверх существующего `MovementService`. Для этого используйте внедрение зависимостей:

```csharp
public class MovementCommandAdapter : MonoBehaviour
{
    private IMovementService _movementService;

    [Inject]
    public void Construct(IMovementService movementService)
    {
        _movementService = movementService;
    }

    public void ExecuteMove(Vector3Int direction)
    {
        // Проверяем возможность перемещения через MovementService
        var targetPosition = _movementService.GetCurrentPosition() + direction;
        
        if (_movementService.CanMoveTo(targetPosition))
        {
            var moveCommand = new MoveCommand(this, direction, grid, walkableTilemap);
            // CommandManager будет внедряться через DI
            CommandManager commandManager = /* получите через DI */;
            commandManager.ExecuteCommand(moveCommand);
        }
    }
}
```

### Связь с EventBus

Для уведомления других систем о выполнении команд используйте EventBus:

```csharp
public override IEnumerator Execute()
{
    if (!CanExecute())
    {
        yield break;
    }

    // Сохраняем состояние
    SaveState();

    // Выполняем основное действие
    yield return AnimateMovement(_targetPosition);

    // Уведомляем другие системы
    EventBus.Instance.Publish(new PlayerMovedEvent(_previousPosition, _targetPosition));
}
```

## Оптимизация

### Использование пула команд

Для оптимизации частого создания/уничтожения команд используйте CommandPool:

```csharp
private CommandPool _commandPool = new CommandPool(50);

private CommandManager _commandManager;

[Inject]
public void Construct(CommandManager commandManager)
{
    _commandManager = commandManager;
}

private void CreateMoveCommand(Vector3Int direction)
{
    var moveCommand = _commandPool.GetCommand<MoveCommand>(this, direction, grid, tilemap);
    _commandManager.ExecuteCommand(moveCommand);
    
    // После выполнения команды возвращаем её в пул
    // _commandPool.ReturnCommand(moveCommand);
}
```

## Примеры использования

### Пример 1: Простое перемещение игрока

```csharp
public class SimplePlayerController : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private Tilemap _walkableTilemap;
    
    private CommandManager _commandManager;

    [Inject]
    public void Construct(CommandManager commandManager)
    {
        _commandManager = commandManager;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var moveCommand = new MoveCommand(this, Vector3Int.up, _grid, _walkableTilemap);
            _commandManager.ExecuteCommand(moveCommand);
        }
    }
}
```

### Пример 2: Последовательность действий

```csharp
public void ExecuteSequence()
{
    var sequence = new CompositeCommand();
    
    sequence.AddCommand(new MoveCommand(this, Vector3Int.right, grid, tilemap));
    sequence.AddCommand(new WaitCommand(this));
    sequence.AddCommand(new MoveCommand(this, Vector3Int.up, grid, tilemap));
    
    _commandManager.ExecuteCommand(sequence); // где _commandManager внедрен через [Inject]
}
```

## Тестирование

Для тестирования системы используйте предоставленные юнит-тесты или создайте свои:

```csharp
[Test]
public void MoveCommand_ValidMovement_ShouldExecuteSuccessfully()
{
    var command = new MoveCommand(executor, Vector3Int.right, grid, tilemap);
    Assert.IsTrue(command.CanExecute());
}
```

## Заключение

Система команд предоставляет гибкую архитектуру для реализации пошаговой игровой механики с возможностью отмены действий, поддержки истории и расширения новыми типами команд.


## Интеграция с комнатной системой (DualLayerTile)

Для интеграции с вашей системой генерации подземелий на основе двумерных массивов `DualLayerTile[,]`:

### Использование RoomMoveCommand

Вместо стандартной MoveCommand используйте RoomMoveCommand, которая работает с вашей комнатной системой:


```csharp
// Для вашей комнатной системы
public void MovePlayerInRoom(Vector3Int direction, DualLayerTile[,] roomGrid)
{
    var roomMoveCommand = new RoomMoveCommand(this, direction, roomGrid);
    // CommandManager должен быть внедрен через DI
    CommandManager commandManager = /* получите через DI */;
    commandManager.ExecuteCommand(roomMoveCommand);
}
```

### Настройка на персонаже

На персонаже поместите `RoomPlayerInputCommandHandler` вместо `PlayerInputCommandHandler`:
- `RoomPlayerInputCommandHandler.cs` - для обработки ввода с учетом комнатной системы

#### Настройка в Inspector:
- RoomGrid: перетащите ссылку на двумерный массив `DualLayerTile[,]` вашей текущей комнаты
- PlayerTransform: перетащите трансформ объекта игрока

### Проверка проходимости

Команда RoomMoveCommand автоматически проверяет проходимость через свойства `BaseTile.IsWalkable` в вашей системе `DualLayerTile`.

### Анимация перемещения

Анимация перемещения работает аналогично стандартной MoveCommand, но использует координаты вашей комнатной системы для вычисления целевых позиций.

## Использование VContainer для Dependency Injection

Все компоненты системы команд интегрированы с VContainer:

### Регистрация в LifetimeScope:
```csharp
builder.Register<CommandManager>(Lifetime.Singleton);
builder.Register<TurnSystem>(Lifetime.Singleton);
builder.Register<RoomMoveCommand>(Lifetime.Scoped);
builder.Register<GridMovementHandler>(Lifetime.Scoped);
builder.RegisterEntryPoint<CommandManagerSetup>();
```

### Внедрение зависимостей:
```csharp
[Inject]
public void Construct(CommandManager commandManager, TurnSystem turnSystem)
{
    _commandManager = commandManager;
    _turnSystem = turnSystem;
}
```