# Инструкции по интеграции системы команд с комнатной системой

## Обзор

Данный документ описывает интеграцию системы пошагового передвижения на основе паттерна Command с вашей системой генерации подземелий на основе `DualLayerTile[,]`.

## Компоненты системы

### Основные компоненты
- `CommandManager.cs` - центральный менеджер выполнения команд
- `RoomMoveCommand.cs` - команда перемещения для комнатной системы
- `GridMovementHandler.cs` - обработчик ввода для комнатной системы
- `TurnSystem.cs` - система управления очередностью ходов

## Интеграция с вашей комнатной системой

### 1. Настройка на уровне сцены

#### В GameLifetimeScope или главный Scene Context:
- `CommandManager.cs` - как Singleton
- `TurnSystem.cs` - как Singleton

### 2. Настройка на персонаже/игроке

#### Добавьте на объект игрока:
- `GridMovementHandler.cs` - основной обработчик ввода для комнатной системы

#### Настройка в Inspector:
- `RoomGrid`: перетащите ссылку на двумерный массив `DualLayerTile[,]` текущей комнаты
- `PlayerTransform`: перетащите трансформ объекта игрока
- `MoveSpeed`: (опционально) настройте скорость анимации перемещения

### 3. Получение ссылки на сетку комнаты

Для получения ссылки на `DualLayerTile[,]` текущей комнаты вы можете:

#### Вариант 1: Через RoomService
```csharp
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private GridMovementHandler _commandHandler;
    
    private IRoomService _roomService;
    
    private void Start()
    {
        _roomService = ServiceLocator.GetService<IRoomService>();
        UpdateRoomGrid();
    }
    
    private void UpdateRoomGrid()
    {
        var currentRoom = _roomService.GetCurrentRoom();
        if (currentRoom != null && _commandHandler != null)
        {
            _commandHandler.SetCurrentRoomGrid(currentRoom.Grid);
        }
    }
}
```

#### Вариант 2: Напрямую через ссылку
```csharp
public class DungeonRoomController : MonoBehaviour
{
    [SerializeField] private GridMovementHandler _playerInputHandler;
    private DualLayerTile[,] _roomGrid;
    
    public void SetPlayerInRoom(DualLayerTile[,] roomGrid, Transform playerTransform)
    {
        _roomGrid = roomGrid;
        
        if (_playerInputHandler != null)
        {
            _playerInputHandler.SetCurrentRoomGrid(_roomGrid);
            _playerInputHandler.SetPlayerPosition(playerTransform.position);
        }
    }
}
```

### 4. Дополнительный метод для GridMovementHandler

Для удобства интеграции используйте методы из `GridMovementHandler.cs`:

```csharp
public void SetCurrentRoomGrid(DualLayerTile[,] roomGrid)
{
    _currentRoomGrid = roomGrid;
    // Инициализируем движок с новой сеткой
    if (_movementService != null)
    {
        var startPosition = new Vector3Int(roomGrid.GetLength(0) / 2, roomGrid.GetLength(1) / 2, 0); // центр комнаты
        _movementService.Initialize(roomGrid, startPosition);
    }
}
```

### 5. Настройка ввода

Система поддерживает два варианта ввода:

#### Через существующую систему ввода (Unity Input System):
- Автоматическая интеграция через `IInputActionsWrapper`

#### Через прямой ввод (WASD/стрелки):
- Работает как fallback
- Горячие клавиши:
  - WASD или стрелки: движение
  - Ctrl+Z: отмена последней команды
  - Ctrl+Y или Ctrl+Shift+Z: повтор команды
  - Space или . (точка): пропуск хода

### 6. Проверка проходимости

Команда `RoomMoveCommand` автоматически проверяет проходимость через:
- `BaseTile.IsWalkable` - для проверки проходимости базового тайла
- Границы сетки комнаты (проверка на выход за пределы массива)

### 7. Анимация перемещения

Анимация перемещения будет работать автоматически, преобразуя координаты сетки комнаты в мировые координаты.

## Пример интеграции с GameHubHandler

Если вы используете `GameHubHandler`, добавьте интеграцию следующим образом:

```csharp
public class GameHubHandler : MonoBehaviour
{
    [SerializeField] private GridMovementHandler _commandHandler;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private IRoomService _roomService;
    
    private void Start()
    {
        InitializePlayerInRoom();
    }
    
    private void InitializePlayerInRoom()
    {
        var currentRoom = _roomService.GetCurrentRoom();
        if (currentRoom != null && _commandHandler != null)
        {
            _commandHandler.SetCurrentRoomGrid(currentRoom.Grid);
            // GridMovementHandler работает с Transform напрямую через _playerTransform
        }
    }
    
    public void OnRoomChanged(Room newRoom)
    {
        if (_commandHandler != null)
        {
            _commandHandler.SetCurrentRoomGrid(newRoom.Grid);
        }
    }
}
```

## Запуск и тестирование

1. Убедитесь, что `CommandManager` и `TurnSystem` зарегистрированы в DI контейнере
2. Настройте `GridMovementHandler` на персонаже с правильными ссылками
3. Проверьте, что `DualLayerTile[,]` сетка комнаты передана в обработчик
4. Протестируйте перемещение с помощью WASD или стрелок

Система полностью готова к использованию с вашей комнатной системой на основе `DualLayerTile[,]`.