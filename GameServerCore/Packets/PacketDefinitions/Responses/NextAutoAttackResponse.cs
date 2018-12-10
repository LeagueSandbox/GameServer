using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class NextAutoAttackResponse : ICoreResponse
    {
        public IAttackableUnit Attacker { get; }
        public IAttackableUnit Target { get; }
        public uint FutureProjNetId { get; }
        public bool IsCritical { get; }
        public bool NextAttackFlag { get; }
        public NextAutoAttackResponse(IAttackableUnit attacker, IAttackableUnit target, uint futureProjNetId, bool isCritical, bool nextAttackFlag)
        {
            Attacker = attacker;
            Target = target;
            FutureProjNetId = futureProjNetId;
            IsCritical = isCritical;
            NextAttackFlag = nextAttackFlag;
        }
    }
};