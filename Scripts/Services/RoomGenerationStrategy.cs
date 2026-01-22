using System;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Стратегия генерации пустой комнаты
    /// </summary>
    public class EmptyRoomGenerationStrategy : IRoomGenerationStrategy
    {
        private readonly Random _random;

        public EmptyRoomGenerationStrategy(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(DualLayerTile[,] grid, int stage)
        {
            // Минимальное содержимое - возможно, 0-1 враг или препятствие
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Оставляем пространство от краев для потенциальных дверей
            for (int x = 2; x < width - 2; x++)
            {
                for (int y = 2; y < height - 2; y++)
                {
                    // С небольшой вероятностью добавляем препятствие как наложение
                    if (_random.NextDouble() < 0.1) // 10% шанс добавить препятствие
                    {
                        if (grid[x, y].Base.Type == TileType.Floor)
                        {
                            grid[x, y] = new DualLayerTile(
                                grid[x, y].Base,
                                new OverlayTile(TileType.Pillar, false, stage <= (int)PlayerStage.KID)
                            );
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Стратегия генерации тактической комнаты
    /// </summary>
    public class TacticalRoomGenerationStrategy : IRoomGenerationStrategy
    {
        private readonly Random _random;

        public TacticalRoomGenerationStrategy(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(DualLayerTile[,] grid, int stage)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Добавляем укрытия и ловушки
            int numWalls = 3 + _random.Next(2); // 3-4 внутренних укрытий
            for (int i = 0; i < numWalls; i++)
            {
                int x, y;
                do
                {
                    x = 2 + _random.Next(width - 4); // Избегаем краев
                    y = 2 + _random.Next(height - 4);
                } while (grid[x, y].Base.Type != TileType.Floor);

                grid[x, y] = new DualLayerTile(
                    grid[x, y].Base,
                    new OverlayTile(TileType.Wall, false, stage <= (int)PlayerStage.KID)
                );
            }

            int numTraps = _random.Next(1, 3); // 1-2 ловушки
            for (int i = 0; i < numTraps; i++)
            {
                int x, y;
                do
                {
                    x = 2 + _random.Next(width - 4);
                    y = 2 + _random.Next(height - 4);
                } while (grid[x, y].Base.Type != TileType.Floor);

                grid[x, y] = new DualLayerTile(
                    grid[x, y].Base,
                    new OverlayTile(TileType.Trap, true, false)
                );
            }
        }
    }

    /// <summary>
    /// Стратегия генерации охотничьей комнаты
    /// </summary>
    public class HuntRoomGenerationStrategy : IRoomGenerationStrategy
    {
        private readonly Random _random;

        public HuntRoomGenerationStrategy(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(DualLayerTile[,] grid, int stage)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Добавляем укрытия для тактического расположения врагов
            int numPillars = 2 + _random.Next(3); // 2-4 колонны
            for (int i = 0; i < numPillars; i++)
            {
                int x, y;
                do
                {
                    x = 2 + _random.Next(width - 4); // Избегаем краев более строго
                    y = 2 + _random.Next(height - 4);
                } while (grid[x, y].Base.Type != TileType.Floor);

                grid[x, y] = new DualLayerTile(
                    grid[x, y].Base,
                    new OverlayTile(TileType.Pillar, false, stage <= (int)PlayerStage.KID)
                );
            }
        }
    }

    /// <summary>
    /// Стратегия генерации паззл-комнаты
    /// </summary>
    public class PuzzleRoomGenerationStrategy : IRoomGenerationStrategy
    {
        private readonly Random _random;

        public PuzzleRoomGenerationStrategy(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(DualLayerTile[,] grid, int stage)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Добавляем интерактивные объекты
            int numObjects = 2 + _random.Next(3); // 2-4 объекта
            for (int i = 0; i < numObjects; i++)
            {
                int x, y;
                do
                {
                    x = 2 + _random.Next(width - 4);
                    y = 2 + _random.Next(height - 4);
                } while (grid[x, y].Base.Type != TileType.Floor);

                grid[x, y] = new DualLayerTile(
                    grid[x, y].Base,
                    new OverlayTile(TileType.InteractiveObject, true, false)
                );
            }

            // С вероятностью добавляем одного врага
            if (_random.NextDouble() < 0.3) // 30% шанс добавить врага
            {
                int x, y;
                do
                {
                    x = 2 + _random.Next(width - 4);
                    y = 2 + _random.Next(height - 4);
                } while (grid[x, y].Base.Type != TileType.Floor);

                // Используем Pillar как символическое обозначение врага на карте
                grid[x, y] = new DualLayerTile(
                    grid[x, y].Base,
                    new OverlayTile(TileType.Pillar, false, stage <= (int)PlayerStage.KID)
                );
            }
        }
    }

    /// <summary>
    /// Стратегия генерации ключевого события
    /// </summary>
    public class KeyEventRoomGenerationStrategy : IRoomGenerationStrategy
    {
        private readonly Random _random;

        public KeyEventRoomGenerationStrategy(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(DualLayerTile[,] grid, int stage)
        {
            // Комнаты с ключевым событием обычно имеют минимальное количество препятствий
            // для акцентирования внимания на событии
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Только несколько декоративных элементов
            if (_random.NextDouble() < 0.2) // 20% шанс добавить элемент
            {
                int x = width / 2;
                int y = height / 2;

                grid[x, y] = new DualLayerTile(
                    grid[x, y].Base,
                    new OverlayTile(TileType.InteractiveObject, true, false)
                );
            }
        }
    }

    /// <summary>
    /// Стратегия генерации комнаты выхода
    /// </summary>
    public class ExitRoomGenerationStrategy : IRoomGenerationStrategy
    {
        private readonly Random _random;

        public ExitRoomGenerationStrategy(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(DualLayerTile[,] grid, int stage)
        {
            // Финальная комната может иметь особое оформление в зависимости от стадии
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Добавляем несколько элементов, символизирующих завершение
            for (int x = 3; x < width - 3; x++)
            {
                for (int y = 3; y < height - 3; y++)
                {
                    if (_random.NextDouble() < 0.15) // 15% шанс добавить элемент
                    {
                        if (grid[x, y].Base.Type == TileType.Floor)
                        {
                            var elementType = _random.NextDouble() < 0.5 ? TileType.Pillar : TileType.InteractiveObject;
                            grid[x, y] = new DualLayerTile(
                                grid[x, y].Base,
                                new OverlayTile(elementType, true, stage <= (int)PlayerStage.KID)
                            );
                        }
                    }
                }
            }
        }
    }
}