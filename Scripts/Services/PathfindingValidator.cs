using System;
using System.Collections.Generic;

namespace ChalkAndSteel.Services
{
    /// <summary>
    /// Реализация валидатора проходимости с использованием A* алгоритма
    /// </summary>
    public class PathfindingValidator : IPathfindingValidator
    {
        public bool IsPathAvailable(Tile[,] grid, int startX, int startY, int endX, int endY)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Проверяем, что старт и финиш находятся в пределах сетки и являются проходимыми
            if (startX < 0 || startX >= width || startY < 0 || startY >= height ||
                endX < 0 || endX >= width || endY < 0 || endY >= height)
                return false;

            if (!grid[startX, startY].IsPassable || !grid[endX, endY].IsPassable)
                return false;

            // Если старт и финиш совпадают, считаем, что путь есть
            if (startX == endX && startY == endY)
                return true;

            // Реализация A* алгоритма с использованием SortedSet вместо PriorityQueue
            var openSet = new SortedSet<NodeWithPriority>();
            var closedSet = new HashSet<(int x, int y)>();

            var gScore = new Dictionary<(int x, int y), int>();
            var fScore = new Dictionary<(int x, int y), int>();

            var startNode = new NodeWithPriority((startX, startY), 0);
            openSet.Add(startNode);
            gScore[(startX, startY)] = 0;
            fScore[(startX, startY)] = Heuristic(startX, startY, endX, endY);

            int[] dx = { -1, 1, 0, 0 }; // лево, право, верх, низ
            int[] dy = { 0, 0, -1, 1 };

            while (openSet.Count > 0)
            {
                var current = openSet.Min;
                openSet.Remove(current);

                if (current.Position.x == endX && current.Position.y == endY)
                {
                    return true; // Нашли путь
                }

                closedSet.Add(current.Position);

                // Проверяем соседние клетки
                for (int i = 0; i < 4; i++)
                {
                    int newX = current.Position.x + dx[i];
                    int newY = current.Position.y + dy[i];

                    // Проверяем границы
                    if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                        continue;

                    // Если клетка недоступна или уже в закрытом множестве
                    if (!grid[newX, newY].IsPassable || closedSet.Contains((newX, newY)))
                        continue;

                    int tentativeGScore = gScore[current.Position] + 1;

                    if (!gScore.ContainsKey((newX, newY)) || tentativeGScore < gScore[(newX, newY)])
                    {
                        // Обновляем путь
                        gScore[(newX, newY)] = tentativeGScore;
                        int f = tentativeGScore + Heuristic(newX, newY, endX, endY);
                        fScore[(newX, newY)] = f;

                        var newNode = new NodeWithPriority((newX, newY), f);
                        openSet.Add(newNode);
                    }
                }
            }

            return false; // Путь не найден
        }

        private int Heuristic(int x1, int y1, int x2, int y2)
        {
            // Манхэттенское расстояние
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        // Вспомогательный класс для сортировки узлов по приоритету
        private class NodeWithPriority : IComparable<NodeWithPriority>
        {
            public (int x, int y) Position { get; }
            public int Priority { get; }

            public NodeWithPriority((int x, int y) position, int priority)
            {
                Position = position;
                Priority = priority;
            }

            public int CompareTo(NodeWithPriority other)
            {
                int result = Priority.CompareTo(other.Priority);
                if (result == 0)
                    result = Position.x.CompareTo(other.Position.x);
                if (result == 0)
                    result = Position.y.CompareTo(other.Position.y);
                return result;
            }

            public override bool Equals(object obj)
            {
                if (obj is NodeWithPriority other)
                    return Position.x == other.Position.x && Position.y == other.Position.y;
                return false;
            }

            public override int GetHashCode()
            {
                return Position.x.GetHashCode() ^ Position.y.GetHashCode();
            }
        }
    }
}