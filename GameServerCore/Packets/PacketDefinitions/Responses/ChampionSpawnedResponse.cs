using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ChampionSpawnedResponse : ICoreResponse
    {
        public IChampion Champ { get; }
        public TeamId Team { get; }
        public ChampionSpawnedResponse(IChampion c, TeamId team)
        {
            Champ = c;
            Team = team;
        }
    }
}