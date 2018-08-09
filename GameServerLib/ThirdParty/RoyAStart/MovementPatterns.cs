namespace RoyT.AStar
{
    /// <summary>
    /// Predefined ranges of motions for your agent.
    /// </summary>
    public static class MovementPatterns
    {        
        /// <summary>
        /// Both diagonal and lateral movement (west, north-west, north, north-east, south-east, south, south-west).
        /// The path planning algorithm's heuristic is best suited for this movement pattern.
        /// </summary>
        public static readonly Offset[] Full = {
            new Offset(-1, -1), new Offset(0, -1), new Offset(1, -1),
            new Offset(-1, 0),                     new Offset(1, 0),
            new Offset(-1, 1),  new Offset(0, 1),  new Offset(1, 1)
        };

        /// <summary>
        /// Lateral movement only (west, north, easth, south)
        /// </summary>
        public static readonly Offset[] LateralOnly = {
                                new Offset(0, -1),
            new Offset(-1, 0),                     new Offset(1, 0),
                                new Offset(0, 1)
        };

        /// <summary>
        /// Diagonal movement only (north-west, north-east, south-east, south-west)
        /// </summary>
        public static readonly Offset[] DiagonalOnly = {
            new Offset(-1, -1),                    new Offset(1, -1),
            
            new Offset(-1, 1),                     new Offset(1, 1)
        };
    }
}
