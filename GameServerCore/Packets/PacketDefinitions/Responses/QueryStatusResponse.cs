namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class QueryStatusResponse : ICoreResponse
    {
        public int UserId { get; }
        public QueryStatusResponse(int userId)
        {
            UserId = userId;
        }
    }
};