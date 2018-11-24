using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class TeleportResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public float X { get; }
        public float Y { get; }
        public TeleportResponse(IAttackableUnit u, float x, float y)
        {
            Unit = u;
            X = x;
            Y = y;
        }
    }
};