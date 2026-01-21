using System;

namespace Core.Generation
{
    /// <summary>
    /// Типы комнат в подземелье
    /// </summary>
    public enum RoomType
    {
        Empty,      // Пустая/Коридор: Минимум препятствий
        Tactical,   // Тактическая: Множество укрытий, ловушек, перемещаемых объектов
        Hunt,       // Охотничья: Несколько врагов, расставленных тактически
        Puzzle,     // Пазл-комната: Требует взаимодействия с окружением для открытия выхода
        KeyEvent,   // Ключевое событие: Триггер смены стадии или нарративная виньетка
        Exit        // Финальная комната: Выход с этажа или финальная битва/выбор
    }
}

namespace ChalkAndSteel.Services
{
    using Core.Generation;
    
    /// <summary>
    /// Типы тайлов в сетке комнаты
    /// </summary>
    public enum TileType
    {
        Floor,              // Пол
        Wall,               // Стена
        Pillar,             // Колонна
        Trap,               // Ловушка
        InteractiveObject,  // Интерактивный объект
        Entrance,           // Вход в комнату
        Exit                // Выход из комнаты
    }

    /// <summary>
    /// Состояние тайла
    /// </summary>
    public class Tile
    {
        public TileType Type { get; set; }
        public bool IsPassable { get; set; }
        public bool IsInteractive { get; set; }
        public bool IsDestructible { get; set; }

        public Tile(TileType type, bool isPassable, bool isInteractive, bool isDestructible = false)
        {
            Type = type;
            IsPassable = isPassable;
            IsInteractive = isInteractive;
            IsDestructible = isDestructible;
        }
    }
}