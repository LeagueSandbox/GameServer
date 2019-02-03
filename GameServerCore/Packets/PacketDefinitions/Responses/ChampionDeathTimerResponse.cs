using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ChampionDeathTimerResponse : ICoreResponse
    {
        public IChampion Die { get; }
        public ChampionDeathTimerResponse(IChampion die)
        {
            Die = die;
        }
    }
}