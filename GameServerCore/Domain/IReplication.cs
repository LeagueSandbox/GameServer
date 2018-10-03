namespace GameServerCore.Domain
{
    public interface IReplication
    {
        uint NetId { get; }
        /// <summary> Writing to this array will cause an exception </summary>
        IReplicate[,] Values { get; }
        bool Changed { get; }
        void MarkAsUnchanged();
        void Update();
    }
}
