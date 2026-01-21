using System;

namespace ChalkAndSteel.Services
{
    public class RoomTransitionEvent
    {
        public int FromRoomId { get; }
        public int ToRoomId { get; }
        public bool WasPreviousRoomCompleted { get; }

        public RoomTransitionEvent(int fromRoomId, int toRoomId, bool wasPreviousRoomCompleted)
        {
            FromRoomId = fromRoomId;
            ToRoomId = toRoomId;
            WasPreviousRoomCompleted = wasPreviousRoomCompleted;
        }
    }

    public class CurrentRoomCompletedEvent
    {
        public int RoomId { get; }

        public CurrentRoomCompletedEvent(int roomId)
        {
            RoomId = roomId;
        }
    }
}