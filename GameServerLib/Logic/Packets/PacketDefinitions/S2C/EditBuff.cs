using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EditBuff : BasePacket
    {
        public EditBuff(EditBuffArgs args)
            : base(PacketCmd.PKT_S2C_EditBuff, args.Unit.ObjectNetId)
        {
            buffer.Write(args.Slot);//slot
            buffer.Write(args.Stacks);//stacks
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x50);
            buffer.Write((byte)0xC3);
            buffer.Write((byte)0x46);
            buffer.Write(0);
            buffer.Write(args.Unit.ObjectNetId);

        }
    }
}