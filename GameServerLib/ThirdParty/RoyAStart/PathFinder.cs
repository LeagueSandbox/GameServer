using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoyT.AStar
{
    /// <summary>
    /// Computes a path in a grid according to the A* algorithm
    /// </summary>
    internal static partial class PathFinder
    {
        public static List<Position> FindPath(Grid grid, Position start, Position end, Offset[] movementPattern)
        {            
            ClearStepList();

            if (start == end)
            {
                return new List<Position> {start};
            }
           
            var head = new MinHeapNode(start, ManhattanDistance(start, end));
            var open = new MinHeap();            
            open.Push(head);

            var costSoFar = new float[grid.DimX * grid.DimY];
            var cameFrom = new Position[grid.DimX * grid.DimY];                       

            while (open.HasNext())
            {
                // Get the best candidate
                var current = open.Pop().Position;
                MessageCurrent(current, PartiallyReconstructPath(grid, start, current, cameFrom));

                if (current == end)
                {
                    return ReconstructPath(grid, start, end, cameFrom);
                }

                Step(grid, open, cameFrom, costSoFar, movementPattern, current, end);

                MessageClose(current);
            }

            return null;
        }

        public static List<Position> FindPath(Grid grid, Position start, Position end, Offset[] movementPattern, int iterationLimit)
        {
            ClearStepList();

            if (start == end)
            {
                return new List<Position> { start };
            }

            var head = new MinHeapNode(start, ManhattanDistance(start, end));
            var open = new MinHeap();
            open.Push(head);

            var costSoFar = new float[grid.DimX * grid.DimY];
            var cameFrom = new Position[grid.DimX * grid.DimY];
            
            while (open.HasNext() && iterationLimit > 0)
            {
                // Get the best candidate
                var current = open.Pop().Position;
                MessageCurrent(current, PartiallyReconstructPath(grid, start, current, cameFrom));

                if (current == end)
                {
                    return ReconstructPath(grid, start, end, cameFrom);
                }

                Step(grid, open, cameFrom, costSoFar, movementPattern, current, end);

                MessageClose(current);

                --iterationLimit;
            }

            return null;
        }

        private static void Step(
            Grid grid,
            MinHeap open,
            Position[] cameFrom,
            float[] costSoFar,
            Offset[] movementPattern,
            Position current,
            Position end)
        {
            // Get the cost associated with getting to the current position
            var initialCost = costSoFar[grid.GetIndexUnchecked(current.X, current.Y)];

            // Get all directions we can move to according to the movement pattern and the dimensions of the grid
            foreach (var option in GetMovementOptions(current, grid.DimX, grid.DimY, movementPattern))
            {
                var position = current + option;
                var cellCost = grid.GetCellCostUnchecked(position);

                // Ignore this option if the cell is blocked
                if (float.IsInfinity(cellCost))
                    continue;

                var index = grid.GetIndexUnchecked(position.X, position.Y);

                // Compute how much it would cost to get to the new position via this path
                var newCost = initialCost + cellCost * option.Cost;

                // Compare it with the best cost we have so far, 0 means we don't have any path that gets here yet
                var oldCost = costSoFar[index];
                if (!(oldCost <= 0) && !(newCost < oldCost))
                    continue;

                // Update the best path and the cost if this path is cheaper
                costSoFar[index] = newCost;
                cameFrom[index] = current;

                // Use the heuristic to compute how much it will probably cost 
                // to get from here to the end, and store the node in the open list
                var expectedCost = newCost + ManhattanDistance(position, end);
                open.Push(new MinHeapNode(position, expectedCost));

                MessageOpen(position);
            }
        }

        private static List<Position> ReconstructPath(Grid grid, Position start, Position end, Position[] cameFrom)
        {
            var path = new List<Position> { end };
            var current = end;
            do
            {
                var previous = cameFrom[grid.GetIndexUnchecked(current.X, current.Y)];               
                current = previous;
                path.Add(current);
            } while (current != start);

            return path;
        }        

        private static IEnumerable<Offset> GetMovementOptions(
            Position position,
            int dimX,
            int dimY,
            IEnumerable<Offset> movementPattern)
        {
            return movementPattern.Where(
                m =>
                {
                    var target = position + m;
                    return target.X >= 0 && target.X < dimX && target.Y >= 0 && target.Y < dimY;
                });            
        }        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ManhattanDistance(Position p0, Position p1)
        {
            var dx = Math.Abs(p0.X - p1.X);
            var dy = Math.Abs(p0.Y - p1.Y);
            return dx + dy;
        }
    }
}
