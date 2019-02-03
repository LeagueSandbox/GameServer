using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class NpcDieResponse : ICoreResponse
    {
        public IAttackableUnit Die { get; }
        public IAttackableUnit Killer { get; }
        public NpcDieResponse(IAttackableUnit die, IAttackableUnit killer)
        {
            Die = die;
            Killer = killer;
        }
    }
}