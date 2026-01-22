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
            var grid = new Tile[11, 11];

            // 1. Заполнить базовым полом (внутреннюю 9x9 область)
            for (int x = 1; x < 10; x++)
            {
                for (int y = 1; y < 10; y++)
                {
                    grid[x, y] = new Tile(TileType.Floor, true, false);
                }
            }

            // 2. Разместить стены по краям (внешняя рамка 11x11)
            for (int i = 0; i < 11; i++)
            {
                grid[i, 0] = new Tile(TileType.Wall, false, false, stage <= (int)PlayerStage.KID); // Разрушаемые на стадии Ребёнка
                grid[i, 10] = new Tile(TileType.Wall, false, false, stage <= (int)PlayerStage.KID);
                grid[0, i] = new Tile(TileType.Wall, false, false, stage <= (int)PlayerStage.KID);
                grid[10, i] = new Tile(TileType.Wall, false, false, stage <= (int)PlayerStage.KID);
            }

            // 3. Получить стратегию генерации для типа комнаты и применить её к внутренней области
            if (_generationStrategies.TryGetValue(room.Type, out var strategy))
            {
                strategy.Generate(grid, stage);
            }

            // 4. Разместить вход и выход (на внешних стенах, но не в углах)
            PlaceDoors(grid, room.Connections);

            // 5. Проверить проходимость комнаты
            if (!ValidateRoomPathfinding(grid))
            {
                // Если путь недоступен, можно попробовать перегенерировать или использовать дефолтный паттерн
                ApplyFallbackPattern(grid);
            }

            // 6. Присвоить сгенерированную сетку комнате
            room.Grid = grid;
            room.IsGenerated = true;
            
            // Логируем информацию о сгенерированной комнате
            Debug.Log($"Сгенерирована комната {room.Id} (Тип: {room.Type}). Соединения: [{string.Join(",", room.Connections)}]");
        }

        private bool ValidateRoomPathfinding(Tile[,] grid)
        {
            // Найдем точки входа и выхода
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            int entranceX = -1, entranceY = -1, exitX = -1, exitY = -1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y].Type == TileType.Entrance)
                    {
                        entranceX = x;
                        entranceY = y;
                    }
                    else if (grid[x, y].Type == TileType.Exit)
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

        private void ApplyFallbackPattern(Tile[,] grid)
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
                    if (grid[x, y].Type == TileType.Entrance)
                    {
                        entranceX = x;
                        entranceY = y;
                    }
                    else if (grid[x, y].Type == TileType.Exit)
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
                        grid[x, y] = new Tile(TileType.Floor, true, false);
                    }
                }
            }
            
            // Убеждаемся, что вход и выход остаются доступными
            // Они уже должны быть установлены в правильные позиции
        }

        private void PlaceDoors(Tile[,] grid, int[] connections)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            
            // Очищаем предыдущие двери
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y].Type == TileType.Entrance || grid[x, y].Type == TileType.Exit)
                    {
                        grid[x, y] = new Tile(TileType.Floor, true, false);
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
                grid[doorPositions["top"].x, doorPositions["top"].y] = new Tile(TileType.Entrance, true, false);
            }
            else if (connections.Length == 1)
            {
                // Если одна связь - вход и одна дверь для связи
                grid[doorPositions["top"].x, doorPositions["top"].y] = new Tile(TileType.Entrance, true, false);
                grid[doorPositions["bottom"].x, doorPositions["bottom"].y] = new Tile(TileType.Exit, true, false);
            }
            else if (connections.Length == 2)
            {
                // Если две связи - вход и выходы на противоположных сторонах
                grid[doorPositions["top"].x, doorPositions["top"].y] = new Tile(TileType.Entrance, true, false);
                grid[doorPositions["bottom"].x, doorPositions["bottom"].y] = new Tile(TileType.Exit, true, false);
            }
            else
            {
                // Если больше двух связей - размещаем вход на одной стороне и остальные на других сторонах
                // Вход всегда сверху (по соглашению)
                grid[doorPositions["top"].x, doorPositions["top"].y] = new Tile(TileType.Entrance, true, false);
                
                // Определяем, какие стороны использовать для выходов
                var sides = new List<string> { "bottom", "left", "right" };
                
                // Размещаем выходы на доступных сторонах
                for (int i = 1; i < connections.Length && i - 1 < sides.Count; i++)
                {
                    var side = sides[i - 1];
                    grid[doorPositions[side].x, doorPositions[side].y] = new Tile(TileType.Exit, true, false);
                }
            }
        }
    }
}