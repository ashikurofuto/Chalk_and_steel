using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Generation;

namespace ChalkAndSteel.Services
{
    public class InspectorDungeonGenerator : MonoBehaviour, IDungeonGenerationService
    {
        [Header("Настройки генерации")]
        [SerializeField] private DungeonGeneratorSettings settings;
        
        private System.Random _random = new();

        public DungeonMap GenerateDungeon(int stage, int seed = 0)
        {
            if (settings == null)
            {
                Debug.LogWarning("DungeonGeneratorSettings не назначен. Используются значения по умолчанию.");
                // Используем значения по умолчанию
                return GenerateDefaultDungeon(stage, seed);
            }

            if (seed != 0)
            {
                _random = new System.Random(seed);
            }

            var rooms = new List<Room>();
            
            // Используем настройки для определения размера сетки
            int gridSize = settings.GridSize;
            int totalRooms = Mathf.Min(settings.NumberOfRooms, gridSize * gridSize);

            // Создаем комнаты в зависимости от стадии
            for (int i = 0; i < totalRooms; i++)
            {
                int x = i % gridSize;
                int y = i / gridSize;
                
                int id = i;
                var roomType = DetermineRoomType(id, totalRooms, stage, x, y);
                
                // Определяем возможные соединения в зависимости от стадии
                var possibleConnections = DeterminePossibleConnections(id, gridSize, stage, x, y);
                
                rooms.Add(new Room(id, roomType, possibleConnections.ToArray(), false, false));
            }

            var startRoomId = 0; // Первая комната
            
            // Настройка последней комнаты в зависимости от стадии
            if (rooms.Count > 0)
            {
                var exitRoom = rooms[totalRooms - 1];
                rooms.RemoveAt(totalRooms - 1);
                
                // Для стадии ADULT добавляем более сложную финальную комнату
                var finalExitRoomType = stage == 3 ? RoomType.KeyEvent : RoomType.Exit;
                rooms.Add(exitRoom.With(Type: finalExitRoomType));
            }

            // Обновим сетку комнат с учетом Exit
            var finalRooms = rooms.OrderBy(r => r.Id).ToList();

            // Выводим в лог список всех комнат в графе
            LogDungeonRooms(finalRooms);

            return new DungeonMap(finalRooms, startRoomId, startRoomId);
        }
        
        private void LogDungeonRooms(List<Room> rooms)
        {
            Debug.Log($"Сгенерирован граф подземелья с {rooms.Count} комнатами:");
            foreach (var room in rooms)
            {
                Debug.Log($"  Комната {room.Id}: Тип={room.Type}, Соединения=[{string.Join(",", room.Connections)}], Сгенерирована={room.IsGenerated}");
            }
        }

        private DungeonMap GenerateDefaultDungeon(int stage, int seed)
        {
            if (seed != 0)
            {
                _random = new System.Random(seed);
            }

            var rooms = new List<Room>();
            const int gridSize = 3; // 3x3 сетка комнат для примера
            int totalRooms = gridSize * gridSize;

            // Создаем комнаты в зависимости от стадии
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    int id = y * gridSize + x;
                    var roomType = DetermineRoomType(id, totalRooms, stage, x, y);
                    
                    // Определяем возможные соединения в зависимости от стадии
                    var possibleConnections = DeterminePossibleConnections(id, gridSize, stage, x, y);
                    
                    rooms.Add(new Room(id, roomType, possibleConnections.ToArray(), false, false));
                }
            }

            var startRoomId = 0; // Первая комната
            
            // Настройка последней комнаты в зависимости от стадии
            var exitRoom = rooms[totalRooms - 1];
            rooms.RemoveAt(totalRooms - 1);
            
            // Для стадии ADULT добавляем более сложную финальную комнату
            var finalExitRoomType = stage == 3 ? RoomType.KeyEvent : RoomType.Exit;
            rooms.Add(exitRoom.With(Type: finalExitRoomType));

            // Обновим сетку комнат с учетом Exit
            var finalRooms = rooms.OrderBy(r => r.Id).ToList();

            return new DungeonMap(finalRooms, startRoomId, startRoomId);
        }

        private RoomType DetermineRoomType(int id, int totalRooms, int stage, int x, int y)
        {
            // Специальные комнаты
            if (id == 0) return RoomType.Empty; // Стартовая комната
            if (id == totalRooms - 1)
            {
                // Для стадии ADULT делаем более сложную финальную комнату
                return stage == 3 ? RoomType.KeyEvent : RoomType.Exit;
            }

            var roll = _random.NextDouble();
            
            // Варьируем вероятности в зависимости от стадии и позиции
            switch (stage)
            {
                case 1: // KID - более линейный лабиринт с тупиками-ловушками
                    if (roll < 0.3) return RoomType.Empty;
                    if (roll < 0.6) return RoomType.Tactical;
                    if (roll < 0.8) return RoomType.Hunt;
                    return RoomType.Puzzle;
                    
                case 2: // TEEN - более развитые развилки
                    // Увеличиваем вероятность ключевых событий в центральной области
                    if (x == 1 && y == 1 && roll < 0.1) return RoomType.KeyEvent;
                    if (roll < 0.15) return RoomType.Empty;
                    if (roll < 0.5) return RoomType.Tactical;
                    if (roll < 0.75) return RoomType.Hunt;
                    if (roll < 0.9) return RoomType.Puzzle;
                    return RoomType.KeyEvent;
                    
                case 3: // ADULT - наиболее сложные комнаты с обязательным боем
                    // Повышаем вероятность боевых комнат
                    if (roll < 0.1) return RoomType.Empty;
                    if (roll < 0.4) return RoomType.Tactical;
                    if (roll < 0.75) return RoomType.Hunt; // Увеличенная вероятность Hunt комнат
                    if (roll < 0.9) return RoomType.Puzzle;
                    return RoomType.KeyEvent;
                    
                default:
                    if (roll < 0.2) return RoomType.Empty;
                    if (roll < 0.5) return RoomType.Tactical;
                    if (roll < 0.8) return RoomType.Hunt;
                    return RoomType.Puzzle;
            }
        }

        private List<int> DeterminePossibleConnections(int id, int gridSize, int stage, int x, int y)
        {
            var possibleConnections = new List<int>();

            // Соединения с соседями
            if (x > 0) possibleConnections.Add((y * gridSize) + (x - 1)); // Слева
            if (x < gridSize - 1) possibleConnections.Add((y * gridSize) + (x + 1)); // Справа
            if (y > 0) possibleConnections.Add(((y - 1) * gridSize) + x); // Сверху
            if (y < gridSize - 1) possibleConnections.Add(((y + 1) * gridSize) + x); // Снизу

            // Варьируем соединения в зависимости от стадии
            switch (stage)
            {
                case 1: // KID - более линейная структура
                    // Удаляем случайные соединения, чтобы сделать путь более линейным
                    if (possibleConnections.Count > 1 && _random.NextDouble() > 0.4) // 60% шанс удалить соединение
                    {
                        possibleConnections.RemoveAt(_random.Next(possibleConnections.Count));
                    }
                    break;
                    
                case 2: // TEEN - стандартная структура
                    // Добавляем немного случайности (не для всех комнат)
                    if (_random.NextDouble() > 0.3f && possibleConnections.Count > 1)
                    {
                        possibleConnections.RemoveAt(_random.Next(possibleConnections.Count));
                    }
                    break;
                    
                case 3: // ADULT - более сложная структура с петлями
                    // В некоторых случаях добавляем дополнительные соединения для создания петель
                    if (possibleConnections.Count > 2 && _random.NextDouble() > 0.7) // 30% шанс оставить больше соединений
                    {
                        // Оставляем случайное количество соединений, но не менее 2
                        while (possibleConnections.Count > 2 && _random.NextDouble() > 0.5)
                        {
                            possibleConnections.RemoveAt(_random.Next(possibleConnections.Count));
                        }
                    }
                    break;
            }

            return possibleConnections;
        }
    }
}