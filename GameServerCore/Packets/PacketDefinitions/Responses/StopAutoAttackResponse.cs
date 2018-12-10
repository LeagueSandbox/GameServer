using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class StopAutoAttackResponse : ICoreResponse
    {
        public IAttackableUnit Attacker { get; }
        public StopAutoAttackResponse(IAttackableUnit attacker)
        {
            Attacker = attacker;
        }
    }
}