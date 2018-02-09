using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FreezeUnitAnimation : BasePacket
    {
        public FreezeUnitAnimation(AttackableUnit u, bool freeze)
            : base(PacketCmd.PKT_S2C_FreezeUnitAnimation, u.NetId)
        {
            byte flag = 0xDE;
            if (freeze)
                flag = 0xDD;
            buffer.Write(flag);
        }
    }
}