using UnityEngine;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Событие запроса генерации подземелья.
    /// Публикуется для запуска процесса создания подземелья.
    /// </summary>
    public record DungeonGenerationRequestedEvent
    {
        public int RoomsCount { get; }
        public Transform GenerationOrigin { get; }

        public DungeonGenerationRequestedEvent(int roomsCount, Transform generationOrigin)
        {
            RoomsCount = roomsCount;
            GenerationOrigin = generationOrigin;
        }
    }
}

