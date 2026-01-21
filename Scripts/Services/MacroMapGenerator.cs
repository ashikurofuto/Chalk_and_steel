using Core.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Улучшенный генератор макрокарты, учитывающий стадию закалки
    /// </summary>
    public class MacroMapGenerator : IDungeonGenerationService
    {
        private System.Random _random = new();

        public DungeonMap GenerateDungeon(int stage, int seed = 0)
        {
            if (seed != 0)
            {
                _random = new System.Random(seed);
            }

            // Используем RoomGraphGenerator для создания графа комнат с правильными связями
            var graphGenerator = new RoomGraphGenerator(_random);
            const int totalRooms = 9; // 3x3 сетка комнат
            var rooms = graphGenerator.GenerateRoomGraph(totalRooms, stage);

            var startRoomId = 0; // Первая комната
            var currentRoomId = startRoomId;
            
            // Выводим в лог список всех комнат в подземелье
            LogGeneratedRooms(rooms);

            return new DungeonMap(rooms, startRoomId, currentRoomId);
        }
        
        private void LogGeneratedRooms(List<Room> rooms)
        {
            Debug.Log($"Сгенерирован граф подземелья с {rooms.Count} комнатами:");
            foreach (var room in rooms)
            {
                Debug.Log($"  Комната {room.Id}: Тип={room.Type}, Соединения=[{string.Join(",", room.Connections)}], Сгенерирована={room.IsGenerated}");
            }
        }
    }
}