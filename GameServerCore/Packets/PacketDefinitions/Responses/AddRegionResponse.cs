using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class AddRegionResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public uint NewFogId { get; }
        public AddRegionResponse(IAttackableUnit u, uint newFogId)
        {
            Unit = u;
            NewFogId = newFogId;
        }
    }
}