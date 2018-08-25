using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetSpellActiveState : BasePacket
    {
        public SetSpellActiveState(IAttackableUnit u, byte slot, byte state)
            : base(PacketCmd.PKT_S2C_SET_SPELL_ACTIVE_STATE, u.NetId)
        {
            Write(slot);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write(state);
        }
    }
}