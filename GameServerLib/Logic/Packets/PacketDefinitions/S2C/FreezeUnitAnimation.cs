using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FreezeUnitAnimation : BasePacket
    {
        public FreezeUnitAnimation(Game game, AttackableUnit u, bool freeze)
            : base(game, PacketCmd.PKT_S2C_FREEZE_UNIT_ANIMATION, u.NetId)
        {
            byte flag = 0xDE;
            if (freeze)
                flag = 0xDD;
            Write(flag);
        }
    }
}