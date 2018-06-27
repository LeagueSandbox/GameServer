using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EditBuff : BasePacket
    {
        public EditBuff(AttackableUnit u, byte slot, byte stacks)
            : base(PacketCmd.PKT_S2_C_EDIT_BUFF, u.NetId)
        {
            _buffer.Write(slot);//slot
            _buffer.Write(stacks);//stacks
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x50);
            _buffer.Write((byte)0xC3);
            _buffer.Write((byte)0x46);
            _buffer.Write(0);
            _buffer.Write(u.NetId);

        }
    }
}