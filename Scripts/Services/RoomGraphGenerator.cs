using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Generation;

namespace ChalkAndSteel.Services
{
    public class RoomGraphGenerator
    {
        private readonly System.Random _random;

        public RoomGraphGenerator(System.Random random = null)
        {
            _random = random ?? new System.Random();
        }

        /// <summary>
        /// Генерирует граф комнат с правильными связями
        /// </summary>
        /// <param name="totalRooms">Общее количество комнат</param>
        /// <param name="stage">Стадия игрока</param>
        /// <returns>Список комнат с правильными двусторонними связями</returns>
        public List<Room> GenerateRoomGraph(int totalRooms, int stage)
        {
            if (totalRooms < 2)
            {
                Debug.LogError("Для генерации подземелья требуется как минимум 2 комнаты");
                return new List<Room>();
            }

            var rooms = new List<Room>();
            
            // Создаем комнаты без связей
            for (int i = 0; i < totalRooms; i++)
            {
                var roomType = DetermineRoomType(i, totalRooms, stage);
                rooms.Add(new Room(i, roomType, new int[0], false, false));
            }

            // Создаем основной путь от стартовой комнаты к финальной
            CreateMainPath(rooms);

            // Добавляем дополнительные связи в зависимости от стадии
            AddAdditionalConnections(rooms, stage);

            // Проверяем, что все комнаты достижимы
            if (!IsGraphConnected(rooms))
            {
                // Если граф несвязный, добавляем связи для соединения компонентов
                ConnectComponents(rooms);
            }

            // Убеждаемся, что все связи двусторонние
            EnsureBidirectionalConnections(rooms);

            // Обновляем комнаты с учетом двусторонних связей
            var updatedRooms = new List<Room>();
            for (int i = 0; i < rooms.Count; i++)
            {
                var connections = GetRoomConnections(rooms, i);
                var room = rooms[i];
                updatedRooms.Add(new Room(room.Id, room.Type, connections.ToArray(), room.IsGenerated, room.IsCompleted, room.Grid));
            }

            // Выводим в лог список всех комнат в графе
            LogGeneratedRooms(updatedRooms);

            return updatedRooms;
        }

        private void LogGeneratedRooms(List<Room> rooms)
        {
            Debug.Log($"Сгенерирован граф подземелья с {rooms.Count} комнатами:");
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                Debug.Log($"  Комната {room.Id}: Тип={room.Type}, Соединения=[{string.Join(",", room.Connections)}], Сгенерирована={room.IsGenerated}");
            }
        }

        private void CreateMainPath(List<Room> rooms)
        {
            // Создаем линейный путь от 0 до последней комнаты
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                // Добавляем двустороннюю связь между комнатами
                AddBidirectionalConnection(rooms, i, i + 1);
            }
        }

        private void AddAdditionalConnections(List<Room> rooms, int stage)
        {
            // Варьируем количество дополнительных связей в зависимости от стадии
            int additionalConnectionsCount = 0;
            
            switch (stage)
            {
                case 1: // KID - минимальное количество дополнительных связей (чтобы избежать циклов)
                    additionalConnectionsCount = _random.Next(0, 2); // 0-1 дополнительная связь
                    break;
                case 2: // TEEN - среднее количество дополнительных связей
                    additionalConnectionsCount = _random.Next(1, Math.Max(2, rooms.Count / 3)); // 1-N связей
                    break;
                case 3: // ADULT - больше дополнительных связей для сложности
                    additionalConnectionsCount = _random.Next(Math.Max(1, rooms.Count / 4), Math.Max(2, rooms.Count / 2)); // N-M связей
                    break;
                default:
                    additionalConnectionsCount = _random.Next(0, Math.Max(1, rooms.Count / 3));
                    break;
            }

            // Добавляем дополнительные связи
            for (int i = 0; i < additionalConnectionsCount; i++)
            {
                var possibleConnections = GetPossibleAdditionalConnections(rooms);
                if (possibleConnections.Count > 0)
                {
                    var connection = possibleConnections[_random.Next(possibleConnections.Count)];
                    AddBidirectionalConnection(rooms, connection.from, connection.to);
                }
            }
        }

        private void EnsureBidirectionalConnections(List<Room> rooms)
        {
            // Убедиться, что все соединения между комнатами двусторонние
            // Пройтись по всем комнатам и добавить обратные связи, если они отсутствуют
            
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                var connections = room.Connections.ToList();
                
                foreach (int connectedRoomId in connections)
                {
                    if (connectedRoomId >= 0 && connectedRoomId < rooms.Count && connectedRoomId != i)
                    {
                        var connectedRoom = rooms[connectedRoomId];
                        var connectedRoomConnections = connectedRoom.Connections.ToList();
                        
                        // Если у связанной комнаты нет обратной связи, добавляем её
                        if (!connectedRoomConnections.Contains(i))
                        {
                            connectedRoomConnections.Add(i);
                            rooms[connectedRoomId] = new Room(connectedRoom.Id, connectedRoom.Type, connectedRoomConnections.ToArray(), connectedRoom.IsGenerated, connectedRoom.IsCompleted, connectedRoom.Grid);
                        }
                    }
                }
            }
        }

        private List<(int from, int to)> GetPossibleAdditionalConnections(List<Room> rooms)
        {
            var possibleConnections = new List<(int from, int to)>();

            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    // Проверяем, есть ли уже связь между комнатами
                    if (!IsConnected(rooms, i, j))
                    {
                        // Не создаем связь между соседними комнатами в основном пути, чтобы не создавать маленькие циклы
                        if (Math.Abs(i - j) > 1)
                        {
                            possibleConnections.Add((i, j));
                        }
                    }
                }
            }

            return possibleConnections;
        }

        private void AddBidirectionalConnection(List<Room> rooms, int roomA, int roomB)
        {
            // Убеждаемся, что комнаты существуют
            if (roomA < 0 || roomA >= rooms.Count || roomB < 0 || roomB >= rooms.Count)
                return;

            // Добавляем связь от комнаты A к B
            var connectionsA = rooms[roomA].Connections.ToList();
            if (!connectionsA.Contains(roomB))
                connectionsA.Add(roomB);
            
            // Добавляем связь от комнаты B к A
            var connectionsB = rooms[roomB].Connections.ToList();
            if (!connectionsB.Contains(roomA))
                connectionsB.Add(roomA);

            // Обновляем комнаты
            rooms[roomA] = new Room(rooms[roomA].Id, rooms[roomA].Type, connectionsA.ToArray(), rooms[roomA].IsGenerated, rooms[roomA].IsCompleted, rooms[roomA].Grid);
            rooms[roomB] = new Room(rooms[roomB].Id, rooms[roomB].Type, connectionsB.ToArray(), rooms[roomB].IsGenerated, rooms[roomB].IsCompleted, rooms[roomB].Grid);
        }

        private bool IsConnected(List<Room> rooms, int roomA, int roomB)
        {
            return rooms[roomA].Connections.Contains(roomB) || rooms[roomB].Connections.Contains(roomA);
        }

        private bool IsGraphConnected(List<Room> rooms)
        {
            if (rooms.Count == 0) return true;

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            
            // Начинаем с первой комнаты
            queue.Enqueue(0);
            visited.Add(0);

            while (queue.Count > 0)
            {
                int currentRoomId = queue.Dequeue();

                // Проверяем все соединения текущей комнаты
                foreach (int connectedRoomId in rooms[currentRoomId].Connections)
                {
                    if (!visited.Contains(connectedRoomId))
                    {
                        visited.Add(connectedRoomId);
                        queue.Enqueue(connectedRoomId);
                    }
                }
            }

            // Проверяем, посетили ли мы все комнаты
            return visited.Count == rooms.Count;
        }

        private void ConnectComponents(List<Room> rooms)
        {
            var visited = new HashSet<int>();
            var components = new List<List<int>>();

            // Находим все компоненты связности
            for (int i = 0; i < rooms.Count; i++)
            {
                if (!visited.Contains(i))
                {
                    var component = new List<int>();
                    var queue = new Queue<int>();
                    
                    queue.Enqueue(i);
                    visited.Add(i);
                    component.Add(i);

                    while (queue.Count > 0)
                    {
                        int currentRoomId = queue.Dequeue();

                        foreach (int connectedRoomId in rooms[currentRoomId].Connections)
                        {
                            if (!visited.Contains(connectedRoomId))
                            {
                                visited.Add(connectedRoomId);
                                queue.Enqueue(connectedRoomId);
                                component.Add(connectedRoomId);
                            }
                        }
                    }

                    components.Add(component);
                }
            }

            // Соединяем компоненты
            for (int i = 1; i < components.Count; i++)
            {
                // Соединяем текущую компоненту с предыдущей
                int roomIdFromPrevComponent = components[i - 1][0];
                int roomIdFromCurrentComponent = components[i][0];
                
                AddBidirectionalConnection(rooms, roomIdFromPrevComponent, roomIdFromCurrentComponent);
            }
        }

        // Метод был удален согласно требованиям - больше не нужен алгоритм прохождения комнат

        private List<int> GetRoomConnections(List<Room> rooms, int roomId)
        {
            var connections = new List<int>();
            
            for (int i = 0; i < rooms.Count; i++)
            {
                if (i != roomId && rooms[i].Connections.Contains(roomId))
                {
                    connections.Add(i);
                }
            }
            
            return connections;
        }

        private RoomType DetermineRoomType(int id, int totalRooms, int stage)
        {
            // Специальные комнаты
            if (id == 0) return RoomType.Empty; // Стартовая комната
            if (id == totalRooms - 1) 
            {
                // Финальная комната - босс
                return RoomType.Exit;
            }

            var roll = _random.NextDouble();
            
            // Варьируем вероятности в зависимости от стадии
            switch (stage)
            {
                case 1: // KID - более линейный лабиринт с тупиками-ловушками
                    if (roll < 0.3) return RoomType.Empty;
                    if (roll < 0.6) return RoomType.Tactical;
                    if (roll < 0.8) return RoomType.Hunt;
                    return RoomType.Puzzle;
                    
                case 2: // TEEN - более развитые развилки
                    // Увеличиваем вероятность ключевых событий в центральной области
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
    }
}