using System.Collections.Generic;
using System.Linq;

namespace ChalkAndSteel.Services
{
    public class DungeonMap
    {
        public List<Room> Rooms { get; }
        public int StartRoomId { get; }
        public int CurrentRoomId { get; set; }

        public DungeonMap(List<Room> rooms, int startRoomId, int currentRoomId)
        {
            Rooms = rooms;
            StartRoomId = startRoomId;
            CurrentRoomId = currentRoomId;
        }

        public Room GetRoom(int roomId)
        {
            return Rooms.FirstOrDefault(r => r.Id == roomId);
        }
    }
}