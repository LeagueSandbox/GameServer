namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SpawnEndResponse : ICoreResponse
    {
        public int UserId { get; }
        public SpawnEndResponse(int userId)
        {
            UserId = userId;
        }
    }
}