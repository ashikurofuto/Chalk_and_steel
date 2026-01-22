using System;
using System.Collections.Generic;
using Core.Generation;
using UnityEngine;

namespace ChalkAndSteel.Services
{
    public class RoomGenerationService : IRoomGenerationService
    {
        private readonly System.Random _random = new();
        private readonly Dictionary<RoomType, IRoomGenerationStrategy> _generationStrategies;
        private readonly IPathfindingValidator _pathfindingValidator;

        public RoomGenerationService(IPathfindingValidator pathfindingValidator = null)
        {
            _pathfindingValidator = pathfindingValidator ?? new PathfindingValidator();
            
            // Инициализируем стратегии генерации для каждого типа комнаты
            _generationStrategies = new Dictionary<RoomType, IRoomGenerationStrategy>
            {
                { RoomType.Empty, new EmptyRoomGenerationStrategy(_random) },
                { RoomType.Tactical, new TacticalRoomGenerationStrategy(_random) },
                { RoomType.Hunt, new HuntRoomGenerationStrategy(_random) },
                { RoomType.Puzzle, new PuzzleRoomGenerationStrategy(_random) },
                { RoomType.KeyEvent, new KeyEventRoomGenerationStrategy(_random) },
                { RoomType.Exit, new ExitRoomGenerationStrategy(_random) }
            };
        }

        public void GenerateRoomContent(Room room, int stage)
        {
            if (room.IsGenerated) return; // Не генерируем дважды

            // Создаем сетку 11x11 для комнаты (внешние стены 1 тайл + игровое поле 9 тайлов + внешние стены 1 тайл)
            var grid = new DualLayerTile[11, 11];

            // 1. Заполнить базовым полом (внутреннюю 9x9 область)
            for (int x = 0; x < 11; x++)
            {
                for (int y = 0; y < 11; y++)
                {
                    // Для краев создаем стены, для внутренней части - пол
                    if (x == 0 || x == 10 || y == 0 || y == 10)
                    {
                        // Крайние тайлы - стены
                        grid[x, y] = new DualLayerTile(
                            new BaseTile(TileType.Wall, false, false, stage <= (int)PlayerStage.KID)
                        );
                    }
                    else
                    {
                        // Внутренние тайлы - пол
                        grid[x, y] = new DualLayerTile(
                            new BaseTile(TileType.Floor, true, false)
                        );
                    }
                }
            }

            // 2. Получить стратегию генерации для типа комнаты и применить её к внутренней области
            if (_generationStrategies.TryGetValue(room.Type, out var strategy))
            {
                strategy.Generate(grid, stage);
            }

            // 3. Разместить вход и выход (на внешних стенах, но не в углах)
            PlaceDoors(grid, room.Connections);

            // 4. Проверить проходимость комнаты
            if (!ValidateRoomPathfinding(grid))
            {
                // Если путь недоступен, можно попробовать перегенерировать или использовать дефолтный паттерн
                ApplyFallbackPattern(grid);
            }

            // 5. Присвоить сгенерированную сетку комнате
            room.Grid = grid;
            room.IsGenerated = true;
            
            // Логируем информацию о сгенерированной комнате
            Debug.Log($"Сгенерирована комната {room.Id} (Тип: {room.Type}). Соединения: [{string.Join(",", room.Connections)}]");
        }

        private bool ValidateRoomPathfinding(DualLayerTile[,] grid)
        {
            // Найдем точки входа и выхода
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            int entranceX = -1, entranceY = -1, exitX = -1, exitY = -1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y].Base.Type == TileType.Entrance)
                    {
                        entranceX = x;
                        entranceY = y;
                    }
                    else if (grid[x, y].Base.Type == TileType.Exit ||
                             (grid[x, y].HasOverlay && grid[x, y].Overlay.Type == TileType.Exit))
                    {
                        exitX = x;
                        exitY = y;
                    }
                }
            }

            // Если обе точки найдены, проверяем путь между ними
            if (entranceX != -1 && entranceY != -1 && exitX != -1 && exitY != -1)
            {
                return _pathfindingValidator.IsPathAvailable(grid, entranceX, entranceY, exitX, exitY);
            }

            // Если одна из точек не найдена, считаем комнату валидной (это может быть специальная комната)
            return true;
        }

        private void ApplyFallbackPattern(DualLayerTile[,] grid)
        {
            // Применяем дефолтный паттерн, чтобы гарантировать проходимость
            // Очищаем центральные области, но оставляем проходы к входу и выходу
            
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            // Находим позиции входа и выхода
            int entranceX = -1, entranceY = -1, exitX = -1, exitY = -1;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y].Base.Type == TileType.Entrance)
                    {
                        entranceX = x;
                        entranceY = y;
                    }
                    else if (grid[x, y].Base.Type == TileType.Exit ||
                             (grid[x, y].HasOverlay && grid[x, y].Overlay.Type == TileType.Exit))
                    {
                        exitX = x;
                        exitY = y;
                    }
                }
            }
            
            // Очищаем центральную область, оставляя проходы к входу и выходу
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    // Не очищаем позиции входа и выхода
                    if (!((x == entranceX && y == entranceY) || (x == exitX && y == exitY)))
                    {
                        // Очищаем только накладываемый слой, оставляя базовый слой нетронутым
                        if (grid[x, y].Base.Type == TileType.Floor)
                        {
                            grid[x, y] = new DualLayerTile(grid[x, y].Base, null); // Убираем наложение
                        }
                        else
                        {
                            // Если базовый тайл не пол, создаем новый с полом
                            grid[x, y] = new DualLayerTile(new BaseTile(TileType.Floor, true, false, false), null);
                        }
                    }
                }
            }
            
            // Убеждаемся, что вход и выход остаются доступными
            // Они уже должны быть установлены в правильные позиции
        }

        private void PlaceDoors(DualLayerTile[,] grid, int[] connections)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            // Очищаем предыдущие двери
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y].Base.Type == TileType.Entrance || grid[x, y].Base.Type == TileType.Exit)
                    {
                        grid[x, y] = new DualLayerTile(new BaseTile(TileType.Floor, true, false, false), grid[x, y].Overlay);
                    }
                    else if (grid[x, y].HasOverlay &&
                             (grid[x, y].Overlay.Type == TileType.Entrance || grid[x, y].Overlay.Type == TileType.Exit))
                    {
                        grid[x, y] = new DualLayerTile(grid[x, y].Base, null);
                    }
                }
            }
            
            // Определяем позиции дверей посередине каждой стороны
            var doorPositions = new Dictionary<string, (int x, int y)>
            {
                {"top", (width / 2, 0)},        // верхняя стена (по середине)
                {"bottom", (width / 2, height - 1)}, // нижняя стена (по середине)
                {"left", (0, height / 2)},     // левая стена (по середине)
                {"right", (width - 1, height / 2)}   // правая стена (по середине)
            };
            
            // Размещаем двери в зависимости от количества соединений
            if (connections.Length == 0)
            {
                // Если нет соединений, это тупиковая комната - размещаем только вход
                grid[doorPositions["top"].x, doorPositions["top"].y] = new DualLayerTile(
                    new BaseTile(TileType.Entrance, true, false),
                    grid[doorPositions["top"].x, doorPositions["top"].y].Overlay
                );
            }
            else if (connections.Length == 1)
            {
                // Если одна связь - вход и одна дверь для связи
                grid[doorPositions["top"].x, doorPositions["top"].y] = new DualLayerTile(
                    new BaseTile(TileType.Entrance, true, false),
                    grid[doorPositions["top"].x, doorPositions["top"].y].Overlay
                );
                grid[doorPositions["bottom"].x, doorPositions["bottom"].y] = new DualLayerTile(
                    new BaseTile(TileType.Exit, true, false),
                    grid[doorPositions["bottom"].x, doorPositions["bottom"].y].Overlay
                );
            }
            else if (connections.Length == 2)
            {
                // Если две связи - вход и выходы на противоположных сторонах
                grid[doorPositions["top"].x, doorPositions["top"].y] = new DualLayerTile(
                    new BaseTile(TileType.Entrance, true, false),
                    grid[doorPositions["top"].x, doorPositions["top"].y].Overlay
                );
                grid[doorPositions["bottom"].x, doorPositions["bottom"].y] = new DualLayerTile(
                    new BaseTile(TileType.Exit, true, false),
                    grid[doorPositions["bottom"].x, doorPositions["bottom"].y].Overlay
                );
            }
            else
            {
                // Если больше двух связей - размещаем вход на одной стороне и остальные на других сторонах
                // Вход всегда сверху (по соглашению)
                grid[doorPositions["top"].x, doorPositions["top"].y] = new DualLayerTile(
                    new BaseTile(TileType.Entrance, true, false),
                    grid[doorPositions["top"].x, doorPositions["top"].y].Overlay
                );
                
                // Определяем, какие стороны использовать для выходов
                var sides = new List<string> { "bottom", "left", "right" };
                
                // Размещаем выходы на доступных сторонах
                for (int i = 1; i < connections.Length && i - 1 < sides.Count; i++)
                {
                    var side = sides[i - 1];
                    grid[doorPositions[side].x, doorPositions[side].y] = new DualLayerTile(
                        new BaseTile(TileType.Exit, true, false),
                        grid[doorPositions[side].x, doorPositions[side].y].Overlay
                    );
                }
            }
        }
    }
}