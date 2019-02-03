using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class OnAttackResponse : ICoreResponse
    {
        public IAttackableUnit Attacker { get; }
        public IAttackableUnit Attacked { get; }
        public AttackType AttackType { get; }
        public OnAttackResponse(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType)
        {
            Attacker = attacker;
            Attacked = attacked;
            AttackType = attackType;
        }
    }
}