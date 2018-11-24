namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SetHealth2Response : ICoreResponse
    {
        public int UserId { get; }
        public uint NetId { get; }
        public SetHealth2Response(int userId, uint netId)
        {
            UserId = userId;
            NetId = netId;
        }
    }
}