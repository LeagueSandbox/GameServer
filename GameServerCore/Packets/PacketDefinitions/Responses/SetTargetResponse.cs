using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SetTargetResponse : ICoreResponse
    {
        public IAttackableUnit Attacker { get; }
        public IAttackableUnit Target { get; }
        public SetTargetResponse(IAttackableUnit attacker, IAttackableUnit target)
        {
            Attacker = attacker;
            Target = target;
        }
    }
}