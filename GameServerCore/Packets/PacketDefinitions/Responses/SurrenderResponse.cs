using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SurrenderResponse : ICoreResponse
    {
        public IChampion Starter { get; }
        public byte Flag { get; }
        public byte YesVotes { get; }
        public byte NoVotes { get; }
        public byte MaxVotes { get; }
        public TeamId Team { get; }
        public float TimeOut { get; }
        public SurrenderResponse(IChampion starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
        {
            Starter = starter;
            Flag = flag;
            YesVotes = yesVotes;
            NoVotes = noVotes;
            MaxVotes = maxVotes;
            Team = team;
            TimeOut = timeOut;
        }
    }
}