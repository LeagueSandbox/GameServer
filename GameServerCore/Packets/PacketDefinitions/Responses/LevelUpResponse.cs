using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LevelUpResponse : ICoreResponse
    {
        public IChampion Champ { get; }
        public LevelUpResponse(IChampion c)
        {
            Champ = c;
        }
    }
};