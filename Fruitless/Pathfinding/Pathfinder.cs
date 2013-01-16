using Fruitless.Collections;
using System;
using System.Collections.Generic;

namespace Fruitless.Pathfinding {
    public class Pathfinder {
        public static Path<TCell> GetLinePath<TCell>(TCell[,] grid, TCell from, TCell to) where TCell : IGridCell {
            return GetLinePathOptimized(grid, from, to);
        }

        static Path<TCell> GetLinePathSimple<TCell>(TCell[,] grid, TCell from, TCell to) where TCell : IGridCell {
            Path<TCell> path = new Path<TCell>(from);

            int x0 = from.Column;
            int x1 = to.Column;

            int y0 = from.Row;
            int y1 = to.Row;

            if (x0 != x1 && y0 != y1) {
                int dx = x1 - x0;
                int dy = y1 - y0;

                int y = y0;

                int eps = 0;

                for (int x = x0; x <= x1; x++) {
                    path = path.AddStep(grid[x, y]);

                    eps += dy;

                    if ((eps << 1) >= dx) {
                        y++;
                        eps -= dx;
                    }
                }
            }

            return path;
        }

        static Path<TCell> GetLinePathOptimized<TCell>(TCell[,] grid, TCell from, TCell to) where TCell : IGridCell {
            Path<TCell> path = new Path<TCell>(from);

            int x0 = from.Column;
            int x1 = to.Column;

            int y0 = from.Row;
            int y1 = to.Row;

            if (x0 != x1 && y0 != y1) {
                bool isSteep =
                    Math.Abs(y1 - y0) >
                    Math.Abs(x1 - x0);

                if (isSteep) {
                    int tmp = x0;

                    x0 = y0;
                    y0 = tmp;

                    tmp = x1;

                    x1 = y1;
                    y1 = tmp;
                }

                int dx = Math.Abs(x1 - x0);
                int dy = Math.Abs(y1 - y0);

                int error = 0;
                int derror = dy;

                int xstep = 0;
                int ystep = 0;

                int x = x0;
                int y = y0;

                ystep = y0 < y1 ? 1 : -1;
                xstep = x0 < x1 ? 1 : -1;

                int tmpX = 0;
                int tmpY = 0;

                int columns = grid.GetLength(0);
                int rows = grid.GetLength(1);

                while (x != x1) {
                    x += xstep;
                    error += derror;

                    if ((2 * error) > dx) {
                        y += ystep;
                        error -= dx;
                    }

                    if (isSteep) {
                        tmpX = y;
                        tmpY = x;
                    } else {
                        tmpX = x;
                        tmpY = y;
                    }

                    if (tmpX >= 0 & tmpX < columns & tmpY >= 0 & tmpY < rows) {
                        path = path.AddStep(grid[tmpX, tmpY]);
                    } else {
                        break;
                    }
                }
            }

            return path;
        }

        public static Path<TCell> GetShortestPath<TCell>(TCell[,] grid, TCell from, TCell to) where TCell : IGridCell, IHasNeighbors<TCell> {
            return GetShortestPath(grid, from, to, GetShortestPathDistance, GetShortestPathEstimate);
        }

        public static Path<TCell> GetShortestPath<TCell>(TCell[,] grid, TCell from, TCell to, Func<TCell, TCell, double> distance, Func<TCell, TCell, double> estimate) where TCell : IGridCell, IHasNeighbors<TCell> {
            var closed = new HashSet<TCell>();
            var queue = new PriorityQueue<double, WeightedPath<TCell>>();

            queue.Enqueue(0, new WeightedPath<TCell>(from));

            while (!queue.IsEmpty) {
                var path = queue.Dequeue();

                if (closed.Contains(path.LastStep)) {
                    continue;
                }

                if (path.LastStep.Equals(to)) {
                    return path;
                }

                closed.Add(path.LastStep);

                foreach (TCell n in path.LastStep.GetNeighbours(grid)) {
                    double d = distance(path.LastStep, n);

                    WeightedPath<TCell> newPath = (WeightedPath<TCell>)path.AddStep(n, d);

                    queue.Enqueue(newPath.TotalCost + estimate(n, to), newPath);
                }
            }

            return null;
        }

        static double GetShortestPathDistance<TCell>(TCell a, TCell b) where TCell : IGridCell {
            double result = 1.0;

            if (b.Row != a.Row &&
                b.Column != a.Column) {
                result += 1.25;
            }

            return result;
        }

        static double GetShortestPathEstimate<TCell>(TCell a, TCell b) where TCell : IGridCell {
            return
                Math.Abs(a.Column - b.Column) +
                Math.Abs(a.Row - b.Row);
        }
    }
}
