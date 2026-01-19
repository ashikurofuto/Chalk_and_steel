using System.Collections.Generic;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Событие завершения генерации подземелья.
    /// Содержит ссылки на созданные комнаты для дальнейшей обработки.
    /// </summary>
    public record DungeonGeneratedEvent
    {
        public IReadOnlyList<RoomData> GeneratedRooms { get; }

        public DungeonGeneratedEvent(IReadOnlyList<RoomData> generatedRooms)
        {
            GeneratedRooms = generatedRooms;
        }
    }
}
