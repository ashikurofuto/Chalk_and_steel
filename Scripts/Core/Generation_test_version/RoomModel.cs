using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Generation
{
    /// <summary>
    /// Модель комнаты, анализирующая Tilemap для определения размеров, центра и соседей.
    /// Работает в 2D пространстве.
    /// </summary>
    public class RoomModel
    {
        /// <summary>
        /// Уникальный идентификатор комнаты.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Тип комнаты.
        /// </summary>
        public RoomType RoomType { get; private set; }

        /// <summary>
        /// Tilemap пола комнаты.
        /// </summary>
        public Tilemap FloorTilemap { get; private set; }

        /// <summary>
        /// Tilemap стен комнаты.
        /// </summary>
        public Tilemap WallsTilemap { get; private set; }

        /// <summary>
        /// Размер комнаты в мировых единицах (ширина, высота).
        /// </summary>
        public Vector2 Size { get; private set; }

        /// <summary>
        /// Центр комнаты в мировых координатах.
        /// </summary>
        public Vector2 Center { get; private set; }

        /// <summary>
        /// Границы комнаты в мировых координатах.
        /// </summary>
        public Bounds Bounds { get; private set; }

        /// <summary>
        /// Список соседних комнат по сторонам (максимум 2).
        /// Ключ - сторона, значение - ID соседней комнаты.
        /// </summary>
        public Dictionary<RoomSide, int> Neighbors { get; private set; }

        /// <summary>
        /// Создать модель комнаты на основе Tilemap.
        /// </summary>
        /// <param name="id">ID комнаты.</param>
        /// <param name="roomType">Тип комнаты.</param>
        /// <param name="floorTilemap">Tilemap пола (обязательно).</param>
        /// <param name="wallsTilemap">Tilemap стен (может быть null).</param>
        public RoomModel(int id, RoomType roomType, Tilemap floorTilemap, Tilemap wallsTilemap = null)
        {
            Id = id;
            RoomType = roomType;
            FloorTilemap = floorTilemap;
            WallsTilemap = wallsTilemap;
            Neighbors = new Dictionary<RoomSide, int>();

            CalculateRoomDimensions();
        }

        /// <summary>
        /// Вычислить размеры комнаты на основе заполненных тайлов в Tilemap пола.
        /// </summary>
        private void CalculateRoomDimensions()
        {
            if (FloorTilemap == null)
            {
                Debug.LogError($"RoomModel {Id}: FloorTilemap is null, cannot calculate dimensions.");
                Size = Vector2.zero;
                Center = Vector2.zero;
                Bounds = new Bounds(Vector3.zero, Vector3.zero);
                return;
            }

            // Получить все заполненные клетки
            var bounds = FloorTilemap.cellBounds;
            if (bounds.size.x == 0 || bounds.size.y == 0)
            {
                // Если нет тайлов, используем размер по умолчанию
                Size = Vector2.one;
                Center = FloorTilemap.transform.position;
                Bounds = new Bounds(Center, Vector3.one);
                return;
            }

            // Мировые позиции углов
            Vector3 minWorld = FloorTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
            Vector3 maxWorld = FloorTilemap.CellToWorld(new Vector3Int(bounds.xMax - 1, bounds.yMax - 1, 0));

            // Учитываем размер тайла (предполагаем квадратные тайлы)
            Vector3 tileSize = FloorTilemap.cellSize;
            float width = (maxWorld.x - minWorld.x) + tileSize.x;
            float height = (maxWorld.y - minWorld.y) + tileSize.y;

            Size = new Vector2(width, height);
            Center = new Vector2((minWorld.x + maxWorld.x) / 2f, (minWorld.y + maxWorld.y) / 2f);
            Bounds = new Bounds(Center, new Vector3(width, height, 0f));
        }

        /// <summary>
        /// Добавить соседа с указанной стороны.
        /// Проверяет, что соседей не больше 2.
        /// </summary>
        /// <param name="side">Сторона, с которой соседствует комната.</param>
        /// <param name="neighborId">ID соседней комнаты.</param>
        /// <returns>True, если сосед успешно добавлен, false если достигнут лимит.</returns>
        public bool AddNeighbor(RoomSide side, int neighborId)
        {
            if (Neighbors.Count >= 2)
            {
                Debug.LogWarning($"Room {Id}: Cannot add more than 2 neighbors.");
                return false;
            }

            if (Neighbors.ContainsKey(side))
            {
                Debug.LogWarning($"Room {Id}: Already has neighbor on side {side}.");
                return false;
            }

            Neighbors[side] = neighborId;
            return true;
        }

        /// <summary>
        /// Удалить соседа с указанной стороны.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>True, если сосед удалён.</returns>
        public bool RemoveNeighbor(RoomSide side)
        {
            return Neighbors.Remove(side);
        }

        /// <summary>
        /// Получить ID соседа с указанной стороны.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>ID соседа или -1, если соседа нет.</returns>
        public int GetNeighborId(RoomSide side)
        {
            return Neighbors.TryGetValue(side, out int id) ? id : -1;
        }

        /// <summary>
        /// Проверить, есть ли сосед с указанной стороны.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>True, если есть сосед.</returns>
        public bool HasNeighbor(RoomSide side)
        {
            return Neighbors.ContainsKey(side);
        }

        /// <summary>
        /// Получить позицию входа/выхода на указанной стороне комнаты (центр стороны).
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>Мировая позиция центра стороны.</returns>
        public Vector2 GetSideCenter(RoomSide side)
        {
            float halfWidth = Size.x / 2f;
            float halfHeight = Size.y / 2f;

            return side switch
            {
                RoomSide.North => Center + new Vector2(0, halfHeight),
                RoomSide.South => Center + new Vector2(0, -halfHeight),
                RoomSide.East => Center + new Vector2(halfWidth, 0),
                RoomSide.West => Center + new Vector2(-halfWidth, 0),
                _ => Center
            };
        }

        /// <summary>
        /// Получить строковое представление для отладки.
        /// </summary>
        public override string ToString()
        {
            return $"RoomModel[Id={Id}, Type={RoomType}, Size={Size}, Center={Center}, Neighbors={Neighbors.Count}]";
        }
    }

    /// <summary>
    /// Стороны комнаты для указания соседей.
    /// </summary>
    public enum RoomSide
    {
        North,  // верх (положительный Y)
        South,  // низ (отрицательный Y)
        East,   // право (положительный X)
        West    // лево (отрицательный X)
    }
}

