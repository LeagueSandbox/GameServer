using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class BeginAutoAttackResponse : ICoreResponse
    {
        public IAttackableUnit Attacker { get; }
        public IAttackableUnit Victim { get; }
        public uint FutureProjNetId { get; }
        public bool IsCritical { get; }
        public BeginAutoAttackResponse(IAttackableUnit attacker, IAttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            Attacker = attacker;
            Victim = victim;
            FutureProjNetId = futureProjNetId;
            IsCritical = isCritical;
        }
    }
}