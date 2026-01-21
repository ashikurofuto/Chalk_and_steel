using System;
using System.Collections.Generic;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Хранилище состояний комнат для возможности сохранения и восстановления
    /// </summary>
    public class RoomStateStorage
    {
        private readonly Dictionary<int, Room> _roomStates = new();

        public void SaveRoomState(Room room)
        {
            if (room == null) return;
            
            // Сохраняем копию комнаты с текущим состоянием
            _roomStates[room.Id] = room;
        }

        public Room GetSavedRoomState(int roomId)
        {
            return _roomStates.TryGetValue(roomId, out var room) ? room : null;
        }

        public bool HasSavedState(int roomId)
        {
            return _roomStates.ContainsKey(roomId);
        }

        public void ClearSavedState(int roomId)
        {
            _roomStates.Remove(roomId);
        }

        public void ClearAllStates()
        {
            _roomStates.Clear();
        }
    }
}