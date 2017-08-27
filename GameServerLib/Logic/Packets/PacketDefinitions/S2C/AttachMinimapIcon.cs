using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttachMinimapIcon : BasePacket
    {
        public AttachMinimapIcon(AttachMinimapIconArgs args)
            : base(PacketCmd.PKT_S2C_AttachMinimapIcon)
        {
            buffer.Write(args.UnitNetId);
            buffer.Write((byte)args.Unk1);
            buffer.Write(Encoding.Default.GetBytes(args.IconName)); // This is probably the icon name, but sometimes it's empty
            buffer.fill(0, 64 - args.IconName.Length);              // Example: "Quest"
            buffer.Write((byte)args.Unk2);
            buffer.Write(Encoding.Default.GetBytes(args.Unk3));
            buffer.fill(0, 64 - args.Unk3.Length); // Example: "Recall"
            buffer.Write(Encoding.Default.GetBytes(args.Unk4));
            buffer.fill(0, 64 - args.Unk4.Length); // Example "OdinRecall", "odinrecallimproved"
        }
    }
}