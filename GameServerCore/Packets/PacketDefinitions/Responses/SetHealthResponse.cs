using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SetHealthResponse : ICoreResponse
    {
        public int UserId { get; }
        public IAttackableUnit Unit { get; }
        public SetHealthResponse(int userId, IAttackableUnit unit)
        {
            UserId = userId;
            Unit = unit;
        }
    }
}