namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class StaticObjectSpawnResponse : ICoreResponse
    {
        public int UserId { get; }
        public uint NetId { get; }
        public StaticObjectSpawnResponse(int userId, uint netId)
        {
            UserId = userId;
            NetId = netId;
        }
    }
}