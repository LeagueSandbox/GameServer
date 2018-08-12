using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EditBuff : BasePacket
    {
        public EditBuff(AttackableUnit u, byte slot, byte stacks)
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