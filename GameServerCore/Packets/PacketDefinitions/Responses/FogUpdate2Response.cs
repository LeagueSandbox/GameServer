using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class FogUpdate2Response : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public uint NewFogId { get; }
        public FogUpdate2Response(IAttackableUnit u, uint newFogId)
        {
            Unit = u;
            NewFogId = newFogId;
        }
    }
}