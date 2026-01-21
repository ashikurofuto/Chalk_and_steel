using System;
using System.Collections.Generic;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Узел двусвязного списка комнат
    /// </summary>
    public class RoomListNode
    {
        public Room Room { get; set; }
        public RoomListNode Previous { get; set; }
        public RoomListNode Next { get; set; }

        public RoomListNode(Room room)
        {
            Room = room;
            Previous = null;
            Next = null;
        }
    }

    /// <summary>
    /// Двусвязный список комнат для удобного переключения между ними
    /// </summary>
    public class RoomLinkedList
    {
        public RoomListNode Head { get; private set; }
        public RoomListNode Tail { get; private set; }
        public int Count { get; private set; }

        public RoomLinkedList()
        {
            Head = null;
            Tail = null;
            Count = 0;
        }

        /// <summary>
        /// Добавить комнату в конец списка
        /// </summary>
        public void Add(Room room)
        {
            var newNode = new RoomListNode(room);
            
            if (Head == null)
            {
                Head = newNode;
                Tail = newNode;
            }
            else
            {
                Tail.Next = newNode;
                newNode.Previous = Tail;
                Tail = newNode;
            }
            
            Count++;
        }

        /// <summary>
        /// Найти комнату по ID
        /// </summary>
        public RoomListNode Find(int roomId)
        {
            var current = Head;
            while (current != null)
            {
                if (current.Room.Id == roomId)
                    return current;
                current = current.Next;
            }
            return null;
        }

        /// <summary>
        /// Получить комнату по индексу
        /// </summary>
        public RoomListNode GetAt(int index)
        {
            if (index < 0 || index >= Count)
                return null;

            var current = Head;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }
            return current;
        }

        /// <summary>
        /// Очистить список
        /// </summary>
        public void Clear()
        {
            Head = null;
            Tail = null;
            Count = 0;
        }
    }
}