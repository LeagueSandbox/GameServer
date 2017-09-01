using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ShowHPAndName : BasePacket
    {
        public ShowHPAndName(Unit unit, bool show) 
            : base(PacketCmd.PKT_S2C_ShowHPAndName, unit.NetId)
        {
            buffer.Write(show);
            buffer.Write((byte)0x00);
        }
    }
}