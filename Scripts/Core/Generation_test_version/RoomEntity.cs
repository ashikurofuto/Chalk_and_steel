using UnityEngine;

namespace Core.Generation
{
    /// <summary>
    /// Сущность комнаты, объединяющая данные, узел и визуальное представление
    /// </summary>
    public class RoomEntity
    {
        public int Id { get; private set; }
        public RoomType RoomType { get; private set; }
        public Vector3 WorldPosition { get; set; }
        public Vector3 Size { get; set; }
        
        public RoomNode Node { get; private set; }
        public RoomView View { get; private set; }

        public RoomEntity(int id, RoomType roomType, Vector3 size)
        {
            Id = id;
            RoomType = roomType;
            Size = size;
            WorldPosition = Vector3.zero;
        }

        public void Initialize(RoomView view, RoomNode node)
        {
            View = view;
            Node = node;
        }

        public void SetPosition(Vector3 position)
        {
            WorldPosition = position;
            
            // Обновляем позицию во всех компонентах
            if (View != null)
                View.SetPosition(position);
                
            if (Node != null)
                Node.WorldPosition = position;
        }

        public bool IsConnectedTo(RoomEntity other)
        {
            // Проверяем, есть ли соединение между комнатами
            if (Node?.Left?.Id == other.Id || Node?.Right?.Id == other.Id)
                return true;
                
            if (other.Node?.Left?.Id == Id || other.Node?.Right?.Id == Id)
                return true;
                
            return false;
        }

        public override string ToString()
        {
            return $"RoomEntity[Id={Id}, Type={RoomType}, Pos={WorldPosition}, Size={Size}]";
        }
    }
}