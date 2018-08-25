using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class FreezeUnitAnimation : BasePacket
    {
        public FreezeUnitAnimation(IAttackableUnit u, bool freeze)
            : base(PacketCmd.PKT_S2C_FREEZE_UNIT_ANIMATION, u.NetId)
        {
            byte flag = 0xDE;
            if (freeze)
                flag = 0xDD;
            Write(flag);
        }
    }
}