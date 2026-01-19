using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChalkAndSteel.Services
{
    public sealed class DungeonGenerationService : IDungeonGenerationService
    {
        private GameObject _roomPrefab;
        private readonly DoorHelper _doorHelper;
        private readonly List<RoomData> _generatedRooms = new();
        private readonly Dictionary<Vector3Int, RoomData> _roomGrid = new();
        private DungeonGenerationConfig _currentConfig;
        private Transform _currentGenerationOrigin;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        public DungeonGenerationService()
        {
            _doorHelper = new DoorHelper();
        }

        public void Initialize(GameObject roomPrefab)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Сервис генерации подземелья уже инициализирован");
                return;
            }

            _roomPrefab = roomPrefab;
            _isInitialized = true;
            Debug.Log("Сервис генерации подземелья инициализирован");
        }

        public IReadOnlyList<RoomData> GenerateDungeon(DungeonGenerationConfig config, Transform generationOrigin)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Сервис генерации подземелья не инициализирован.");

            if (config.RoomsCount <= 0)
                throw new ArgumentException("Количество комнат должно быть положительным", nameof(config.RoomsCount));

            _currentConfig = config;
            _currentGenerationOrigin = generationOrigin;

            ClearDungeon();
            GenerateDungeonWithOpenDoors(config);

            Debug.Log($"Сгенерировано подземелье: {_generatedRooms.Count} комнат (цель: {config.RoomsCount})");
            return _generatedRooms.AsReadOnly();
        }

        public IReadOnlyList<RoomData> GenerateDungeon(int roomsCount, Transform generationOrigin)
        {
            var config = new DungeonGenerationConfig { RoomsCount = roomsCount };
            return GenerateDungeon(config, generationOrigin);
        }

        public void ClearDungeon()
        {
            foreach (var roomData in _generatedRooms)
            {
                if (roomData.RoomObject != null)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                        GameObject.Destroy(roomData.RoomObject);
                    else
                        GameObject.DestroyImmediate(roomData.RoomObject);
#else
                    GameObject.Destroy(roomData.RoomObject);
#endif
                }
            }

            _generatedRooms.Clear();
            _roomGrid.Clear();
            Debug.Log("Подземелье очищено");
        }

        public IReadOnlyList<RoomData> GetCurrentDungeon() => _generatedRooms.AsReadOnly();

        public IReadOnlyList<RoomData> RegenerateDungeon()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Сервис генерации подземелья не инициализирован.");

            if (_currentConfig == null || _currentGenerationOrigin == null)
                throw new InvalidOperationException("Нет сохраненной конфигурации для пересоздания");

            return GenerateDungeon(_currentConfig, _currentGenerationOrigin);
        }

        public RoomData GetRoomAtPosition(Vector3Int gridPosition)
        {
            return _roomGrid.TryGetValue(gridPosition, out var room) ? room : null;
        }

        // ИСПРАВЛЕННЫЙ АЛГОРИТМ: Все двери открываются сразу при создании комнат
        private void GenerateDungeonWithOpenDoors(DungeonGenerationConfig config)
        {
            int roomSize = config.RoomSize > 0 ? config.RoomSize : 11;

            // 1. Создаем стартовую комнату
            Vector3Int startPosition = Vector3Int.zero;
            var startRoom = CreateRoom(startPosition, RoomType.StartRoom, DoorDirections.None, roomSize);
            _generatedRooms.Add(startRoom);
            _roomGrid[startPosition] = startRoom;

            // 2. Список активных комнат (комнаты, которые могут создавать соседей)
            var activeRooms = new List<RoomData> { startRoom };
            int roomsGenerated = 1;

            // 3. Основной цикл генерации
            while (roomsGenerated < config.RoomsCount && activeRooms.Count > 0)
            {
                // Выбираем последнюю активную комнату
                RoomData currentRoom = activeRooms[activeRooms.Count - 1];

                // Получаем свободные направления для этой комнаты
                var freeDirections = GetFreeDirections(currentRoom.GridPosition, roomSize);

                if (freeDirections.Count == 0)
                {
                    // Если у текущей комнаты нет свободных направлений, удаляем ее из активных
                    activeRooms.RemoveAt(activeRooms.Count - 1);
                    continue;
                }

                // Решаем: создаем 1 или 2 соседние комнаты
                int roomsToCreate = UnityEngine.Random.Range(1, 3); // 1 или 2
                roomsToCreate = Mathf.Min(roomsToCreate, freeDirections.Count);
                roomsToCreate = Mathf.Min(roomsToCreate, config.RoomsCount - roomsGenerated);

                if (roomsToCreate == 0)
                {
                    activeRooms.RemoveAt(activeRooms.Count - 1);
                    continue;
                }

                // Выбираем случайные свободные направления
                ShuffleList(freeDirections);
                var createdRooms = new List<RoomData>();

                for (int i = 0; i < roomsToCreate; i++)
                {
                    var direction = freeDirections[i];
                    Vector3Int newPos = currentRoom.GridPosition + GetDirectionVector(direction) * roomSize;

                    // Проверяем, не занята ли позиция
                    if (_roomGrid.ContainsKey(newPos))
                        continue;

                    // Определяем тип комнаты
                    RoomType roomType = DetermineRoomType(config, roomsGenerated);

                    // КЛЮЧЕВОЙ МОМЕНТ: сразу открываем дверь для входа в новую комнату
                    var oppositeDirection = _doorHelper.GetOppositeDirection(direction);

                    // Создаем комнату с уже открытой входной дверью
                    var newRoom = CreateRoom(newPos, roomType, _doorHelper.AddDoor(DoorDirections.None, oppositeDirection), roomSize);

                    // ОТКРЫВАЕМ ДВЕРЬ В РОДИТЕЛЬСКОЙ КОМНАТЕ ТОЖЕ
                    var parentNewDoors = _doorHelper.AddDoor(currentRoom.Doors, direction);
                    UpdateRoomDoors(currentRoom, parentNewDoors);

                    // Добавляем комнату
                    _generatedRooms.Add(newRoom);
                    _roomGrid[newPos] = newRoom;
                    createdRooms.Add(newRoom);

                    roomsGenerated++;

                    Debug.Log($"Создана комната {roomType} в {newPos} в направлении {direction}");
                    Debug.Log($"  Входная дверь: {oppositeDirection} в новой комнате");
                    Debug.Log($"  Выходная дверь: {direction} в родительской комнате");
                }

                // Выбираем одну из созданных комнат для продолжения генерации
                if (createdRooms.Count > 0)
                {
                    RoomData nextRoom = createdRooms[UnityEngine.Random.Range(0, createdRooms.Count)];

                    // Удаляем текущую комнату из активных
                    activeRooms.RemoveAt(activeRooms.Count - 1);

                    // Добавляем все созданные комнаты в активные (они могут создавать свои ветки)
                    foreach (var room in createdRooms)
                    {
                        if (!activeRooms.Contains(room))
                        {
                            activeRooms.Add(room);
                        }
                    }
                }
                else
                {
                    // Если не удалось создать ни одной комнаты, удаляем текущую из активных
                    activeRooms.RemoveAt(activeRooms.Count - 1);
                }
            }

            // 4. Финальная проверка: закрываем все висячие двери
            CleanupStrayDoors(roomSize);

            Debug.Log($"Генерация завершена. Создано {roomsGenerated} комнат.");
        }

        // Вспомогательные методы
        private List<DoorDirections> GetFreeDirections(Vector3Int position, int roomSize)
        {
            var freeDirections = new List<DoorDirections>();

            // Проверяем все 4 направления
            if (!_roomGrid.ContainsKey(position + Vector3Int.up * roomSize))
                freeDirections.Add(DoorDirections.North);

            if (!_roomGrid.ContainsKey(position + Vector3Int.right * roomSize))
                freeDirections.Add(DoorDirections.East);

            if (!_roomGrid.ContainsKey(position + Vector3Int.down * roomSize))
                freeDirections.Add(DoorDirections.South);

            if (!_roomGrid.ContainsKey(position + Vector3Int.left * roomSize))
                freeDirections.Add(DoorDirections.West);

            return freeDirections;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        private Vector3Int GetDirectionVector(DoorDirections direction)
        {
            return direction switch
            {
                DoorDirections.North => Vector3Int.up,
                DoorDirections.East => Vector3Int.right,
                DoorDirections.South => Vector3Int.down,
                DoorDirections.West => Vector3Int.left,
                _ => Vector3Int.zero
            };
        }

        private void CleanupStrayDoors(int roomSize)
        {
            // Создаем копию списка комнат для безопасной итерации
            var roomsCopy = new List<RoomData>(_generatedRooms);

            foreach (var room in roomsCopy)
            {
                var currentDoors = room.Doors;
                var validDoors = DoorDirections.None;

                // Проверяем каждую дверь
                if (_doorHelper.HasDoor(currentDoors, DoorDirections.North))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.up * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.North);
                    else
                        Debug.Log($"Закрываем висячую дверь на север в комнате {room.GridPosition}");
                }

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.East))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.right * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.East);
                    else
                        Debug.Log($"Закрываем висячую дверь на восток в комнате {room.GridPosition}");
                }

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.South))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.down * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.South);
                    else
                        Debug.Log($"Закрываем висячую дверь на юг в комнате {room.GridPosition}");
                }

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.West))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.left * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.West);
                    else
                        Debug.Log($"Закрываем висячую дверь на запад в комнате {room.GridPosition}");
                }

                // Обновляем если есть изменения
                if (validDoors != currentDoors)
                {
                    UpdateRoomDoors(room, validDoors);
                }
            }
        }

        private RoomType DetermineRoomType(DungeonGenerationConfig config, int roomIndex)
        {
            if (roomIndex == 0) return RoomType.StartRoom;
            if (roomIndex == config.RoomsCount - 1) return RoomType.BossRoom;
            if (roomIndex == config.RoomsCount - 2) return RoomType.TreasureRoom;

            float randomValue = UnityEngine.Random.value;

            if (randomValue < config.BossRoomChance) return RoomType.BossRoom;
            if (randomValue < config.BossRoomChance + config.TreasureRoomChance) return RoomType.TreasureRoom;
            if (randomValue < config.BossRoomChance + config.TreasureRoomChance + config.SpecialRoomChance) return RoomType.SpecialRoom;

            return RoomType.NormalRoom;
        }

        private RoomData CreateRoom(Vector3Int gridPosition, RoomType roomType, DoorDirections doors, int roomSize)
        {
            Vector3 worldPosition = _currentGenerationOrigin.position + new Vector3(
                gridPosition.x,
                gridPosition.y,
                0);

            GameObject roomInstance = GameObject.Instantiate(
                _roomPrefab,
                worldPosition,
                Quaternion.identity,
                _currentGenerationOrigin);

            string roomName = $"{roomType}_{gridPosition.x}_{gridPosition.y}";
            roomInstance.name = roomName;

            var roomInfo = roomInstance.GetComponent<RoomInfo>();
            if (roomInfo != null)
            {
                roomInfo.RoomType = roomType;
                roomInfo.GridPosition = gridPosition;
                roomInfo.SetDoors(doors);
            }
            else
            {
                Debug.LogError($"RoomInfo компонент не найден на префабе комнаты: {_roomPrefab.name}");
            }

            return new RoomData(roomInstance, gridPosition, roomType, doors);
        }

        private void UpdateRoomDoors(RoomData room, DoorDirections newDoors)
        {
            var roomInfo = room.RoomObject.GetComponent<RoomInfo>();
            if (roomInfo != null)
            {
                roomInfo.SetDoors(newDoors);
            }

            // Обновляем данные комнаты в списке
            var index = _generatedRooms.IndexOf(room);
            if (index >= 0)
            {
                _generatedRooms[index] = new RoomData(
                    room.RoomObject,
                    room.GridPosition,
                    room.RoomType,
                    newDoors
                );
            }
        }
    }
}