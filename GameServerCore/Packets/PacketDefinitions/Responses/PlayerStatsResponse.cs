using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class PlayerStatsResponse : ICoreResponse
    {
        public IChampion Champion { get; }
        public PlayerStatsResponse(IChampion champion)
        {
            Champion = champion;
        }
    }
};