using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EditBuff : BasePacket
    {
        public EditBuff(Unit u, byte slot, byte stacks) 
            : base(PacketCmd.PKT_S2C_EditBuff, u.NetId)
        {
            buffer.Write(slot);//slot
            buffer.Write(stacks);//stacks
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x50);
            buffer.Write((byte)0xC3);
            buffer.Write((byte)0x46);
            buffer.Write(0);
            buffer.Write(u.NetId);

        }
    }
}