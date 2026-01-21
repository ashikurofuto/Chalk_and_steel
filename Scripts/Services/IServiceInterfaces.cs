using System;

namespace ChalkAndSteel.Services
{
    public interface IDungeonGenerationService
    {
        DungeonMap GenerateDungeon(int stage, int seed = 0);
    }

    public interface IRoomGenerationService
    {
        void GenerateRoomContent(Room room, int stage);
    }

    public interface IRoomService
    {
        event Action<RoomTransitionEvent> OnRoomTransitioned;
        event Action<CurrentRoomCompletedEvent> OnCurrentRoomCompleted;

        bool IsDungeonInitialized { get; }
        
        void InitializeDungeon(int stageOfTempering);
        bool TryEnterRoom(int targetRoomId, int connectionIndex = 0);
        void CompleteCurrentRoom();
        Room GetCurrentRoom();
    }

    // Удаляем дубликат IMacroMapGenerator
}