namespace RoyT.AStar
{
    /// <summary>
    /// Node in a heap
    /// </summary>
    internal sealed class MinHeapNode
    {        
        public MinHeapNode(Position position, float expectedCost)
        {
            this.Position     = position;
            this.ExpectedCost = expectedCost;            
        }

        public Position Position { get; }
        public float ExpectedCost { get; set; }                
        public MinHeapNode Next { get; set; }
    }
}
