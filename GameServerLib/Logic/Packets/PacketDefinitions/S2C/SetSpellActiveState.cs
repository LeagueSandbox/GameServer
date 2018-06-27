using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetSpellActiveState : BasePacket
    {
        public SetSpellActiveState(AttackableUnit u, byte slot, byte state)
            : base(PacketCmd.PKT_S2_C_SET_SPELL_ACTIVE_STATE, u.NetId)
        {
            _buffer.Write(slot);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write(state);
        }
    }
}