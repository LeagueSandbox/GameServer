using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class AddUnitFOW : BasePacket
    {
        public AddUnitFOW(Unit u) : base(PacketCmd.PKT_S2C_AddUnitFOW)
        {
            buffer.Write((int)u.NetId);
        }
    }
}