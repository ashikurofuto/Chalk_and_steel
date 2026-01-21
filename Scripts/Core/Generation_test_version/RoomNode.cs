using System.Collections.Generic;
using UnityEngine;

namespace Core.Generation
{
    /// <summary>
    /// Узел бинарного дерева для представления комнаты в структуре генерации подземелья.
    /// Каждый узел содержит данные о комнате и ссылки на левого и правого потомков.
    /// </summary>
    public class RoomNode
    {
        /// <summary>
        /// Данные комнаты, связанные с этим узлом.
        /// </summary>
        public RoomData RoomData { get; private set; }

        /// <summary>
        /// Левый дочерний узел (может быть null).
        /// </summary>
        public RoomNode Left { get; set; }

        /// <summary>
        /// Правый дочерний узел (может быть null).
        /// </summary>
        public RoomNode Right { get; set; }

        /// <summary>
        /// Родительский узел (может быть null для корня).
        /// </summary>
        public RoomNode Parent { get; set; }

        /// <summary>
        /// Глубина узла в дереве (0 для корня).
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Позиция комнаты в мировых координатах.
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// Уникальный идентификатор узла.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Создать новый узел комнаты.
        /// </summary>
        /// <param name="id">Уникальный идентификатор.</param>
        /// <param name="roomData">Данные комнаты.</param>
        /// <param name="depth">Глубина узла.</param>
        public RoomNode(int id, RoomData roomData, int depth = 0)
        {
            Id = id;
            RoomData = roomData;
            Depth = depth;
            Left = null;
            Right = null;
            Parent = null;
            WorldPosition = Vector3.zero;
        }

        /// <summary>
        /// Проверка, является ли узел листом (нет потомков).
        /// </summary>
        public bool IsLeaf => Left == null && Right == null;

        /// <summary>
        /// Проверка, является ли узел полным (имеет обоих потомков).
        /// </summary>
        public bool IsFull => Left != null && Right != null;

        /// <summary>
        /// Добавить левого потомка.
        /// </summary>
        /// <param name="child">Дочерний узел.</param>
        public void AddLeftChild(RoomNode child)
        {
            Left = child;
            if (child != null)
            {
                child.Parent = this;
                child.Depth = Depth + 1;
            }
        }

        /// <summary>
        /// Добавить правого потомка.
        /// </summary>
        /// <param name="child">Дочерний узел.</param>
        public void AddRightChild(RoomNode child)
        {
            Right = child;
            if (child != null)
            {
                child.Parent = this;
                child.Depth = Depth + 1;
            }
        }

        /// <summary>
        /// Получить список всех потомков данного узла (рекурсивно).
        /// </summary>
        /// <returns>Список всех потомков.</returns>
        public List<RoomNode> GetAllChildren()
        {
            var children = new List<RoomNode>();
            if (Left != null)
            {
                children.Add(Left);
                children.AddRange(Left.GetAllChildren());
            }
            if (Right != null)
            {
                children.Add(Right);
                children.AddRange(Right.GetAllChildren());
            }
            return children;
        }

        /// <summary>
        /// Получить количество потомков (рекурсивно).
        /// </summary>
        /// <returns>Количество потомков.</returns>
        public int GetChildrenCount()
        {
            int count = 0;
            if (Left != null)
                count += 1 + Left.GetChildrenCount();
            if (Right != null)
                count += 1 + Right.GetChildrenCount();
            return count;
        }

        /// <summary>
        /// Найти узел по идентификатору в поддереве.
        /// </summary>
        /// <param name="id">Идентификатор для поиска.</param>
        /// <returns>Найденный узел или null.</returns>
        public RoomNode FindNodeById(int id)
        {
            if (Id == id)
                return this;

            RoomNode found = null;
            if (Left != null)
                found = Left.FindNodeById(id);
            if (found == null && Right != null)
                found = Right.FindNodeById(id);
            return found;
        }

        /// <summary>
        /// Вычислить bounding box, содержащий все комнаты в поддереве.
        /// </summary>
        /// <returns>Bounding box в мировых координатах.</returns>
        public Bounds CalculateSubtreeBounds()
        {
            var bounds = new Bounds(WorldPosition, RoomData.Size);
            var children = GetAllChildren();
            foreach (var child in children)
            {
                bounds.Encapsulate(new Bounds(child.WorldPosition, child.RoomData.Size));
            }
            return bounds;
        }

        /// <summary>
        /// Получить строковое представление узла для отладки.
        /// </summary>
        /// <returns>Строка с информацией об узле.</returns>
        public override string ToString()
        {
            return $"RoomNode[Id={Id}, Type={RoomData.RoomType}, Pos={WorldPosition}, Depth={Depth}, Leaf={IsLeaf}]";
        }
    }

    /// <summary>
    /// Данные комнаты, используемые в узле дерева.
    /// </summary>
    public class RoomData
    {
        public int Id { get; set; }
        public RoomType RoomType { get; set; }
        public Vector3 Size { get; set; }
        public int Difficulty { get; set; }
        public List<int> ConnectedRoomIds { get; set; }

        public RoomData(int id, RoomType roomType, Vector3 size)
        {
            Id = id;
            RoomType = roomType;
            Size = size;
            Difficulty = 1;
            ConnectedRoomIds = new List<int>();
        }
    }
}




