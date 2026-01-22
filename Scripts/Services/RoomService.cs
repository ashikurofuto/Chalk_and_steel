using Architecture.GlobalModules;
using System;
using System.Linq;
using UnityEngine;

namespace ChalkAndSteel.Services
{
    public class RoomService : IRoomService
    {
        public event Action<RoomTransitionEvent> OnRoomTransitioned;
        public event Action<CurrentRoomCompletedEvent> OnCurrentRoomCompleted;

        private DungeonMap _dungeonMap;
        private RoomLinkedList _roomList;
        private RoomListNode _currentRoomNode;
        private readonly IEventBus _eventBus;
        private readonly IDungeonGenerationService _dungeonGenService;
        private readonly IRoomGenerationService _roomGenService;
        private readonly IPlayerService _playerService;
        private readonly RoomStateStorage _roomStateStorage;

        public bool IsDungeonInitialized => _dungeonMap != null;

        public RoomService(
            IEventBus eventBus,
            IDungeonGenerationService dungeonGenService,
            IRoomGenerationService roomGenService,
            IPlayerService playerService)
        {
            _eventBus = eventBus;
            _dungeonGenService = dungeonGenService;
            _roomGenService = roomGenService;
            _playerService = playerService;
            _roomStateStorage = new RoomStateStorage();
            _roomList = new RoomLinkedList();
        }

        public void InitializeDungeon(int stageOfTempering)
        {
            // Очищаем предыдущее подземелье, если оно было
            _roomList.Clear();
            
            _dungeonMap = _dungeonGenService.GenerateDungeon(stageOfTempering);

            // Заполняем двусвязный список комнатами из макрокарты
            foreach (var room in _dungeonMap.Rooms)
            {
                _roomList.Add(room);
            }

            // Находим стартовую комнату
            var startRoomNode = _roomList.Find(_dungeonMap.StartRoomId);
            if (startRoomNode != null)
            {
                _currentRoomNode = startRoomNode;
                
                // Генерируем содержимое стартовой комнаты, если оно еще не сгенерировано
                if (!_currentRoomNode.Room.IsGenerated)
                {
                    _roomGenService.GenerateRoomContent(_currentRoomNode.Room, stageOfTempering);
                }
            }

            // Выводим в лог список всех комнат в подземелье
            LogDungeonRooms();

            // Публикуем событие перехода в стартовую комнату
            if (_eventBus != null)
                _eventBus.Publish(new RoomTransitionEvent(-1, _currentRoomNode?.Room.Id ?? -1, false));
        }
        
       

        private void LogDungeonRooms()
        {
            if (_dungeonMap?.Rooms == null) return;

            Debug.Log($"Сгенерировано подземелье с {_dungeonMap.Rooms.Count} комнатами:");
            foreach (var room in _dungeonMap.Rooms)
            {
                Debug.Log($"  Комната {room.Id}: Тип={room.Type}, Соединения=[{string.Join(",", room.Connections)}], Сгенерирована={room.IsGenerated}");
            }
        }

       

        // Метод уже определен ранее, удаляем дубликат

        public bool TryEnterRoom(int targetRoomId, int connectionIndex = 0)
        {
            if (!IsDungeonInitialized || _currentRoomNode == null) return false;

            var currentRoom = _currentRoomNode.Room;
            if (currentRoom == null || !currentRoom.Connections.Contains(targetRoomId)) return false; // Проверка доступности

            // Сохраняем состояние текущей комнаты перед переходом
            if (currentRoom != null)
            {
                _roomStateStorage.SaveRoomState(currentRoom);
            }

            // Находим целевую комнату в списке
            var targetRoomNode = _roomList.Find(targetRoomId);
            if (targetRoomNode == null) return false;
            
            var targetRoom = targetRoomNode.Room;
            
            // Проверяем, есть ли сохраненное состояние для целевой комнаты
            var savedRoomState = _roomStateStorage.GetSavedRoomState(targetRoomId);
            if (savedRoomState != null)
            {
                // Восстанавливаем комнату из сохраненного состояния
                targetRoom = savedRoomState;
            }
            else if (!targetRoom.IsGenerated)
            {
                // Генерируем комнату, если она еще не была посещена и нет сохраненного состояния
                var currentStage = (int)_playerService.GetCurrentStage();
                _roomGenService.GenerateRoomContent(targetRoom, currentStage);
            }

            var wasCompleted = currentRoom.IsCompleted;
            var fromRoomId = currentRoom.Id;

            _currentRoomNode = targetRoomNode;

            // Публикуем событие перехода
            var transitionEvent = new RoomTransitionEvent(fromRoomId, _currentRoomNode.Room.Id, wasCompleted);
            if (_eventBus != null)
                _eventBus.Publish(transitionEvent);

            // Логируем переход между комнатами
            Debug.Log($"Переход из комнаты {fromRoomId} (тип: {currentRoom.Type}) в комнату {targetRoomId} (тип: {targetRoom.Type})");

            // Выводим в лог список всех комнат в графе подземелья
            LogDungeonGraph();

            return true;
        }
        
        private void LogDungeonGraph()
        {
            if (_dungeonMap?.Rooms == null) return;

            Debug.Log("Текущий граф подземелья:");
            foreach (var room in _dungeonMap.Rooms)
            {
                Debug.Log($"  Комната {room.Id}: Тип={room.Type}, Соединения=[{string.Join(",", room.Connections)}], Сгенерирована={room.IsGenerated}, Завершена={room.IsCompleted}");
            }
        }

        public void CompleteCurrentRoom()
        {
            if (!IsDungeonInitialized || _currentRoomNode == null) return;

            var currentRoom = _currentRoomNode.Room;
            if (!currentRoom.IsCompleted)
            {
                // Помечаем как завершенную
                var roomIndex = _dungeonMap.Rooms.FindIndex(r => r.Id == currentRoom.Id);
                if (roomIndex != -1)
                {
                    // Создаем обновленную комнату с использованием метода With
                    var updatedRoom = _dungeonMap.Rooms[roomIndex].With(IsCompleted: true);
                    _dungeonMap.Rooms[roomIndex] = updatedRoom;
                    
                    // Обновляем комнату в хранилище состояний, если она там есть
                    if (_roomStateStorage.HasSavedState(currentRoom.Id))
                    {
                        _roomStateStorage.SaveRoomState(updatedRoom);
                    }
                }

                var completedEvent = new CurrentRoomCompletedEvent(currentRoom.Id);
                if (_eventBus != null)
                    _eventBus.Publish(completedEvent);
            }
        }

        public Room GetCurrentRoom()
        {
            if (!IsDungeonInitialized || _currentRoomNode == null) return null;
            return _currentRoomNode.Room;
        }
    }
}