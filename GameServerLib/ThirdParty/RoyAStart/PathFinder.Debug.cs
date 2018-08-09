using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Viewer")]

namespace RoyT.AStar
{
    
    internal static partial class PathFinder
    {
        internal static List<Step> StepList { get; } = new List<Step>(0);

        [Conditional("DEBUG")]
        private static void MessageCurrent(Position position, IReadOnlyList<Position> path)
        {                        
            StepList.Add(new Step(StepType.Current, position, path));
        }            

        [Conditional("DEBUG")]
        private static void MessageOpen(Position position)
            => StepList.Add(new Step(StepType.Open, position, new List<Position>(0)));

        [Conditional("DEBUG")]
        private static void MessageClose(Position position)
            => StepList.Add(new Step(StepType.Close, position, new List<Position>(0)));

        [Conditional("DEBUG")]
        private static void ClearStepList()
            => StepList.Clear();
        
        private static List<Position> PartiallyReconstructPath(Grid grid, Position start, Position end, Position[] cameFrom)
        {
            var path = new List<Position> { end };

#if DEBUG          
            var current = end;
            do
            {
                var previous = cameFrom[grid.GetIndexUnchecked(current.X, current.Y)];

                // If the path is invalid, probably becase we've not closed
                // a node yet, return an empty list
                if (current == previous)
                    return new List<Position>();

                current = previous;
                path.Add(current);
            } while (current != start);

#endif
            return path;
        }
    }

    internal class Step
    {
        public Step(StepType type, Position position, IReadOnlyList<Position> path)
        {
            this.Type = type;
            this.Position = position;
            this.Path = path;
        }

        public StepType Type { get; }
        public Position Position { get; }
        public IReadOnlyList<Position> Path { get; }
    }

    internal enum StepType
    {
        Current,
        Open,
        Close
    }
}
