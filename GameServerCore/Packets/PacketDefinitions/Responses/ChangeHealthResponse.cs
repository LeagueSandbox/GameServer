using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class ChangeHealthResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public ChangeHealthResponse(IAttackableUnit unit)
        {
            Unit = unit;
        }
    }
}
