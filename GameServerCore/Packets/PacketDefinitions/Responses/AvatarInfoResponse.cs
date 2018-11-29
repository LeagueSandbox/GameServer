using GameServerCore.NetInfo;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class AvatarInfoResponse : ICoreResponse
    {
        public int UserId { get; }
        public ClientInfo Client { get; }
        public AvatarInfoResponse(int userId, ClientInfo client)
        {
            UserId = userId;
            Client = client;
        }
    }
}