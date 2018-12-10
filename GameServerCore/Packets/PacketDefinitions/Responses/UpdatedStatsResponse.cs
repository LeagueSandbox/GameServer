using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class UpdatedStatsResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public bool Partial { get; }
        public UpdatedStatsResponse(IAttackableUnit u, bool partial = true)
        {
            Unit = u;
            Partial = partial;
        }
    }
}