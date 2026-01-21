using System;
using Core.Generation;

namespace ChalkAndSteel.Services
{
    public class Room
    {
        public int Id { get; }
        public RoomType Type { get; }
        public int[] Connections { get; }
        public bool IsGenerated { get; set; }
        public bool IsCompleted { get; set; }
        public Tile[,] Grid { get; set; }

        public Room(int id, RoomType type, int[] connections, bool isGenerated = false, bool isCompleted = false, Tile[,] grid = null)
        {
            Id = id;
            Type = type;
            Connections = connections;
            IsGenerated = isGenerated;
            IsCompleted = isCompleted;
            Grid = grid;
        }

        // Реализация метода с тем же функционалом, что и record
        public Room With(bool? IsGenerated = null, bool? IsCompleted = null, Tile[,] Grid = null, RoomType? Type = null)
        {
            return new Room(
                Id,
                Type ?? this.Type,
                Connections,
                IsGenerated ?? this.IsGenerated,
                IsCompleted ?? this.IsCompleted,
                Grid ?? this.Grid
            );
        }
    }
}