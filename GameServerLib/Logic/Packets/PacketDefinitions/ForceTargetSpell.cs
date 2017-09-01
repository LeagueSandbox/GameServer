using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class ForceTargetSpell : BasePacket
    {
        public ForceTargetSpell(Unit u, byte slot, float time)
            : base(PacketCmd.PKT_S2C_ForceTargetSpell, u.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((float)time);
        }
    }
}