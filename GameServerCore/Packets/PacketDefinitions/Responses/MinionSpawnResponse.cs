using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class MinionSpawnResponse : ICoreResponse
    {
        public IMinion Minion { get; }
        public MinionSpawnResponse(IMinion minion)
        {
            Minion = minion;
        }
    }
}
