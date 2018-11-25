using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SpawnResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public SpawnResponse(IAttackableUnit u)
        {
            Unit = u;
        }
    }
};