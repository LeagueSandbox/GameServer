using GameServerCore.NetInfo;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class HeroSpawnResponse : ICoreResponse
    {
        public int UserId { get; }
        public ClientInfo Client { get; }
        public int PlayerId { get; }
        public HeroSpawnResponse(int userId, ClientInfo client, int playerId)
        {
            UserId = userId;
            Client = client;
            PlayerId = playerId;
        }
    }
};