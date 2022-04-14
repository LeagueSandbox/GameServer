using LeaguePackets.Game.Common;
namespace GameServerCore.Domain
{
    public interface IReplication
    {
        uint NetID { get; }
        /// <summary> Writing to this array will cause an exception </summary>
        IReplicate[,] Values { get; }
        bool Changed { get; }
        void MarkAsUnchanged();
        void Update();
        ReplicationData GetData(bool partial = true);
    }
}
