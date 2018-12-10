using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class HeroSpawn2Response : ICoreResponse
    {
        public int UserId { get; }
        public IChampion Champion { get; }
        public HeroSpawn2Response(int userId, IChampion champion)
        {
            UserId = userId;
            Champion = champion;
        }
    }
}