using System;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Интерфейс для стратегии генерации содержимого комнаты
    /// </summary>
    public interface IRoomGenerationStrategy
    {
        void Generate(Tile[,] grid, int stage);
    }

    /// <summary>
    /// Интерфейс для проверки проходимости комнаты
    /// </summary>
    public interface IPathfindingValidator
    {
        bool IsPathAvailable(Tile[,] grid, int startX, int startY, int endX, int endY);
    }

    /// <summary>
    /// Интерфейс для генерации макрокарты подземелья
    /// </summary>
    public interface IMacroMapGenerator
    {
        DungeonMap Generate(int stage, int seed = 0);
    }
}