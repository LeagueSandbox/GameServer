using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class EditBuff : BasePacket
    {
        public EditBuff(IAttackableUnit u, byte slot, byte stacks)
            : base(PacketCmd.PKT_S2C_EDIT_BUFF, u.NetId)
        {
            Write(slot);//slot
            Write(stacks);//stacks
            Write((byte)0x00);
            Write((byte)0x50);
            Write((byte)0xC3);
            Write((byte)0x46);
            Write(0);
            WriteNetId(u);
        }
    }
}