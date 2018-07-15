using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ShowHpAndName : BasePacket
    {
        public ShowHpAndName(AttackableUnit unit, bool show)
            : base(PacketCmd.PKT_S2C_SHOW_HP_AND_NAME, unit.NetId)
        {
            Write(show);
            Write((byte)0x00);
        }
    }
}