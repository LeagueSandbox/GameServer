using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ForceTargetSpell : BasePacket
    {
        public ForceTargetSpell(AttackableUnit u, byte slot, float time)
            : base(PacketCmd.PKT_S2_C_FORCE_TARGET_SPELL, u.NetId)
        {
            _buffer.Write((byte)slot);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((float)time);
        }
    }
}