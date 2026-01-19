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
                throw new ArgumentException("Количество комнат должна быть положительным", nameof(config.RoomsCount));

            _currentConfig = config;
            _currentGenerationOrigin = generationOrigin;

            ClearDungeon();
            GenerateHybridDungeon(config);

            Debug.Log($"Сгенерировано подземелья: {_generatedRooms.Count} комнат (цель: {config.RoomsCount})");
            return _generatedRooms.AsReadOnly();
        }

        public IReadOnlyList<RoomData> GenerateDungeon(int roomsCount, Transform generationOrigin)
        {
            var config = new DungeonGenerationConfig
            {
                RoomsCount = roomsCount,
                TreasureRoomChance = 0.05f,
                SpecialRoomChance = 0.03f,
                ShopRoomChance = 0.08f,  // Магазин чаще особых комнат
                BossRoomChance = 0.00f   // Босс только в конце
            };
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

        // ГИБРИДНЫЙ АЛГОРИТМ: с магазином
        private void GenerateHybridDungeon(DungeonGenerationConfig config)
        {
            int roomSize = config.RoomSize > 0 ? config.RoomSize : 11;

            // 1. Создаем стартовую комнату
            Vector3Int startPosition = Vector3Int.zero;
            var startRoom = CreateRoom(startPosition, RoomType.StartRoom, DoorDirections.None, roomSize);
            _generatedRooms.Add(startRoom);
            _roomGrid[startPosition] = startRoom;

            // 2. Список комнат для расширения
            var roomsToExpand = new List<RoomData> { startRoom };
            int roomsGenerated = 1;

            // 3. Основной цикл генерации
            while (roomsGenerated < config.RoomsCount && roomsToExpand.Count > 0)
            {
                RoomData currentRoom = roomsToExpand[roomsToExpand.Count - 1];

                var freeDirections = GetFreeDirections(currentRoom.GridPosition, roomSize);

                if (freeDirections.Count == 0)
                {
                    roomsToExpand.RemoveAt(roomsToExpand.Count - 1);
                    continue;
                }

                int roomsToCreate = UnityEngine.Random.Range(1, 3);
                roomsToCreate = Mathf.Min(roomsToCreate, freeDirections.Count);
                roomsToCreate = Mathf.Min(roomsToCreate, config.RoomsCount - roomsGenerated);

                if (roomsToCreate == 0)
                {
                    roomsToExpand.RemoveAt(roomsToExpand.Count - 1);
                    continue;
                }

                ShuffleList(freeDirections);
                var createdRooms = new List<RoomData>();
                var allNewDoorsForCurrentRoom = DoorDirections.None;

                for (int i = 0; i < roomsToCreate; i++)
                {
                    var direction = freeDirections[i];
                    Vector3Int newPos = currentRoom.GridPosition + GetDirectionVector(direction) * roomSize;

                    if (_roomGrid.ContainsKey(newPos))
                        continue;

                    // Определяем тип комнаты
                    RoomType roomType;

                    // КЛЮЧЕВОЕ ПРАВИЛО: Только последняя комната - босс
                    if (roomsGenerated == config.RoomsCount - 1)
                    {
                        // Последняя комната всегда босс
                        roomType = RoomType.BossRoom;
                        Debug.Log($"🔥 ПОСЛЕДНЯЯ КОМНАТА (#{roomsGenerated + 1}) - БОСС в позиции {newPos}");
                    }
                    else
                    {
                        // Все остальные комнаты определяются случайно
                        roomType = DetermineRandomRoomTypeWithShop(config, roomsGenerated);

                        // Отладочные сообщения для особых комнат
                        switch (roomType)
                        {
                            case RoomType.TreasureRoom:
                                Debug.Log($"💰 СОКРОВИЩНИЦА в позиции {newPos} (комната #{roomsGenerated + 1})");
                                break;
                            case RoomType.SpecialRoom:
                                Debug.Log($"✨ ОСОБАЯ КОМНАТА в позиции {newPos} (комната #{roomsGenerated + 1})");
                                break;
                            case RoomType.ShopRoom:
                                Debug.Log($"🏪 МАГАЗИН в позиции {newPos} (комната #{roomsGenerated + 1})");
                                break;
                        }
                    }

                    var oppositeDirection = _doorHelper.GetOppositeDirection(direction);
                    var newRoom = CreateRoom(newPos, roomType,
                        _doorHelper.AddDoor(DoorDirections.None, oppositeDirection), roomSize);

                    allNewDoorsForCurrentRoom = _doorHelper.AddDoor(allNewDoorsForCurrentRoom, direction);

                    _generatedRooms.Add(newRoom);
                    _roomGrid[newPos] = newRoom;
                    createdRooms.Add(newRoom);

                    roomsGenerated++;
                }

                if (allNewDoorsForCurrentRoom != DoorDirections.None)
                {
                    var updatedDoors = _doorHelper.AddDoor(currentRoom.Doors, allNewDoorsForCurrentRoom);
                    UpdateRoomDoors(currentRoom, updatedDoors);
                }

                if (createdRooms.Count > 0)
                {
                    RoomData nextRoom = createdRooms[UnityEngine.Random.Range(0, createdRooms.Count)];

                    roomsToExpand.RemoveAt(roomsToExpand.Count - 1);
                    roomsToExpand.Add(nextRoom);

                    foreach (var room in createdRooms)
                    {
                        if (room != nextRoom && !roomsToExpand.Contains(room))
                        {
                            roomsToExpand.Add(room);
                        }
                    }
                }
                else
                {
                    roomsToExpand.RemoveAt(roomsToExpand.Count - 1);
                }
            }

            CleanupStrayDoors(roomSize);
            DebugDungeonStructure();
        }

        // НОВЫЙ МЕТОД: Определение типа комнаты с магазином
        private RoomType DetermineRandomRoomTypeWithShop(DungeonGenerationConfig config, int roomIndex)
        {
            // Не допускаем босса в середине подземелья
            float randomValue = UnityEngine.Random.value;

            // Накопительные шансы (без босса)
            float treasureChance = config.TreasureRoomChance;
            float specialChance = config.SpecialRoomChance;
            float shopChance = config.ShopRoomChance;  // Шанс магазина

            // Сумма всех шансов (без босса)
            float totalChance = treasureChance + specialChance + shopChance;

            // Если сумма больше 1, нормализуем шансы
            if (totalChance > 1.0f)
            {
                treasureChance /= totalChance;
                specialChance /= totalChance;
                shopChance /= totalChance;
            }

            // Определяем тип комнаты по диапазонам
            if (randomValue < treasureChance)
            {
                return RoomType.TreasureRoom;
            }
            else if (randomValue < treasureChance + specialChance)
            {
                return RoomType.SpecialRoom;
            }
            else if (randomValue < treasureChance + specialChance + shopChance)
            {
                return RoomType.ShopRoom;
            }
            else
            {
                // Оставшаяся вероятность - обычная комната
                return RoomType.NormalRoom;
            }
        }

        private List<DoorDirections> GetFreeDirections(Vector3Int position, int roomSize)
        {
            var freeDirections = new List<DoorDirections>();

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
            var roomsCopy = new List<RoomData>(_generatedRooms);

            foreach (var room in roomsCopy)
            {
                var currentDoors = room.Doors;
                var validDoors = DoorDirections.None;

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.North))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.up * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.North);
                }

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.East))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.right * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.East);
                }

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.South))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.down * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.South);
                }

                if (_doorHelper.HasDoor(currentDoors, DoorDirections.West))
                {
                    Vector3Int neighborPos = room.GridPosition + Vector3Int.left * roomSize;
                    if (_roomGrid.ContainsKey(neighborPos))
                        validDoors = _doorHelper.AddDoor(validDoors, DoorDirections.West);
                }

                if (validDoors != currentDoors)
                {
                    UpdateRoomDoors(room, validDoors);
                }
            }
        }

        private void DebugDungeonStructure()
        {
            Debug.Log("=== СТРУКТУРА ПОДЗЕМЕЛЬЯ ===");
            int bossCount = 0;
            int treasureCount = 0;
            int specialCount = 0;
            int shopCount = 0;      // НОВЫЙ СЧЕТЧИК
            int normalCount = 0;
            int roomNumber = 0;

            foreach (var room in _generatedRooms)
            {
                roomNumber++;
                string doors = "";
                if (_doorHelper.HasDoor(room.Doors, DoorDirections.North)) doors += "↑";
                if (_doorHelper.HasDoor(room.Doors, DoorDirections.East)) doors += "→";
                if (_doorHelper.HasDoor(room.Doors, DoorDirections.South)) doors += "↓";
                if (_doorHelper.HasDoor(room.Doors, DoorDirections.West)) doors += "←";

                string roomInfo = $"{room.GridPosition}: {room.RoomType} [{doors}]";

                // Подсвечиваем особые комнаты
                if (room.RoomType == RoomType.BossRoom && roomNumber == _generatedRooms.Count)
                    Debug.Log($"🔥 {roomInfo} (ПОСЛЕДНЯЯ КОМНАТА)");
                else if (room.RoomType == RoomType.ShopRoom)
                    Debug.Log($"🏪 {roomInfo}");
                else if (room.RoomType == RoomType.TreasureRoom)
                    Debug.Log($"💰 {roomInfo}");
                else if (room.RoomType == RoomType.SpecialRoom)
                    Debug.Log($"✨ {roomInfo}");
                else
                    Debug.Log(roomInfo);

                switch (room.RoomType)
                {
                    case RoomType.BossRoom: bossCount++; break;
                    case RoomType.TreasureRoom: treasureCount++; break;
                    case RoomType.SpecialRoom: specialCount++; break;
                    case RoomType.ShopRoom: shopCount++; break;  // НОВЫЙ СЧЕТЧИК
                    case RoomType.NormalRoom: normalCount++; break;
                }
            }

            Debug.Log($"ИТОГО: Боссов: {bossCount}, Сокровищниц: {treasureCount}, " +
                     $"Особых: {specialCount}, Магазинов: {shopCount}, Обычных: {normalCount}");
            Debug.Log("==========================");
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