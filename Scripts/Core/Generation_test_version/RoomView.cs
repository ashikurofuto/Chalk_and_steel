using UnityEngine;
using UnityEngine.Tilemaps;

namespace Core.Generation
{
    /// <summary>
    /// Визуальное представление комнаты для 2D проекта.
    /// Содержит ссылки на Tilemap пола и стен, а также на 2D компоненты.
    /// </summary>
    public class RoomView : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap _floorTilemap;
        [SerializeField] private Tilemap _wallsTilemap;
        
        [Header("2D Components")]
        [SerializeField] private Collider2D _roomCollider2D;
        [SerializeField] private SpriteRenderer _roomSpriteRenderer;
        
        /// <summary>
        /// Tilemap пола комнаты.
        /// </summary>
        public Tilemap FloorTilemap => _floorTilemap;

        /// <summary>
        /// Tilemap стен комнаты (может быть null).
        /// </summary>
        public Tilemap WallsTilemap => _wallsTilemap;

        /// <summary>
        /// Коллайдер2D комнаты для физических взаимодействий в 2D.
        /// </summary>
        public Collider2D RoomCollider2D => _roomCollider2D;

        /// <summary>
        /// Спрайт-рендерер комнаты для визуализации в 2D.
        /// </summary>
        public SpriteRenderer RoomSpriteRenderer => _roomSpriteRenderer;

        /// <summary>
        /// Уникальный идентификатор комнаты.
        /// </summary>
        public int RoomId { get; private set; }

        /// <summary>
        /// Тип комнаты (стартовая, обычная, боссовая, сокровищница и т.д.).
        /// </summary>
        public RoomType RoomType { get; private set; }

        /// <summary>
        /// Модель комнаты, рассчитанная на основе Tilemap.
        /// </summary>
        public RoomModel RoomModel { get; private set; }

        /// <summary>
        /// Инициализация комнаты с заданными параметрами.
        /// Создаёт RoomModel на основе Tilemap.
        /// </summary>
        /// <param name="roomId">Уникальный идентификатор.</param>
        /// <param name="roomType">Тип комнаты.</param>
        public void Initialize(int roomId, RoomType roomType)
        {
            RoomId = roomId;
            RoomType = roomType;
            
            // Создаём модель комнаты, анализирующую Tilemap
            RoomModel = new RoomModel(roomId, roomType, _floorTilemap, _wallsTilemap);
            
            UpdateVisuals();
            UpdateColliderFromModel();
        }

        /// <summary>
        /// Обновление визуальных параметров комнаты в зависимости от её типа.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_roomSpriteRenderer != null)
            {
                // Здесь можно менять цвет спрайта в зависимости от типа комнаты
                // Например, _roomSpriteRenderer.color = GetColorForRoomType(RoomType);
            }
        }

        /// <summary>
        /// Настроить коллайдер по размерам модели комнаты (если коллайдер отсутствует).
        /// </summary>
        private void UpdateColliderFromModel()
        {
            if (_roomCollider2D != null || RoomModel == null) return;
            
            // Добавляем BoxCollider2D, соответствующий размерам комнаты
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = RoomModel.Size;
            collider.offset = RoomModel.Center - (Vector2)transform.position;
            _roomCollider2D = collider;
        }

        /// <summary>
        /// Активировать/деактивировать видимость комнаты.
        /// </summary>
        /// <param name="isActive">Флаг активности.</param>
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        /// <summary>
        /// Установить позицию комнаты в мировых координатах.
        /// Также обновляет позицию Tilemap.
        /// </summary>
        /// <param name="position">Новая позиция.</param>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            
            // Если нужно переместить Tilemap относительно родителя, можно настроить локальную позицию
            // В данном случае Tilemap уже является дочерним, поэтому трансформируется автоматически
        }

        /// <summary>
        /// Установить вращение комнаты (в 2D обычно не используется, но оставим).
        /// </summary>
        /// <param name="rotation">Новое вращение.</param>
        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        /// <summary>
        /// Получить границы комнаты (Bounds) для проверки пересечений.
        /// Использует модель комнаты или коллайдер.
        /// </summary>
        /// <returns>Границы комнаты.</returns>
        public Bounds GetBounds()
        {
            if (_roomCollider2D != null)
                return _roomCollider2D.bounds;
            if (RoomModel != null)
                return RoomModel.Bounds;
            return new Bounds(transform.position, Vector3.zero);
        }

        /// <summary>
        /// Получить размер комнаты из модели.
        /// </summary>
        /// <returns>Размер комнаты (ширина, высота).</returns>
        public Vector2 GetRoomSize()
        {
            return RoomModel?.Size ?? Vector2.zero;
        }

        /// <summary>
        /// Получить центр комнаты из модели.
        /// </summary>
        /// <returns>Центр комнаты в мировых координатах.</returns>
        public Vector2 GetRoomCenter()
        {
            return RoomModel?.Center ?? (Vector2)transform.position;
        }

        /// <summary>
        /// Добавить соседа с указанной стороны.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <param name="neighborRoomId">ID соседней комнаты.</param>
        /// <returns>True, если сосед успешно добавлен.</returns>
        public bool AddNeighbor(RoomSide side, int neighborRoomId)
        {
            return RoomModel?.AddNeighbor(side, neighborRoomId) ?? false;
        }

        /// <summary>
        /// Получить ID соседа с указанной стороны.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>ID соседа или -1, если соседа нет.</returns>
        public int GetNeighborId(RoomSide side)
        {
            return RoomModel?.GetNeighborId(side) ?? -1;
        }

        /// <summary>
        /// Проверить, есть ли сосед с указанной стороны.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>True, если есть сосед.</returns>
        public bool HasNeighbor(RoomSide side)
        {
            return RoomModel?.HasNeighbor(side) ?? false;
        }

        /// <summary>
        /// Получить позицию входа/выхода на указанной стороне комнаты.
        /// </summary>
        /// <param name="side">Сторона.</param>
        /// <returns>Мировая позиция центра стороны.</returns>
        public Vector2 GetSideCenter(RoomSide side)
        {
            return RoomModel?.GetSideCenter(side) ?? (Vector2)transform.position;
        }

        /// <summary>
        /// Получить строковое представление для отладки.
        /// </summary>
        public override string ToString()
        {
            return $"RoomView[Id={RoomId}, Type={RoomType}, Model={RoomModel}]";
        }
    }
}