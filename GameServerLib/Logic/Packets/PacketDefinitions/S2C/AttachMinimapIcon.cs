using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttachMinimapIcon : BasePacket
    {
        public AttachMinimapIcon(AttackableUnit unit, byte unk1, string iconName, byte unk2, string unk3, string unk4)
            : base(PacketCmd.PKT_S2C_AttachMinimapIcon)
        {
            buffer.Write(unit.NetId);
            buffer.Write((byte)unk1);
            buffer.Write(Encoding.Default.GetBytes(iconName)); // This is probably the icon name, but sometimes it's empty
            buffer.fill(0, 64 - iconName.Length);              // Example: "Quest"
            buffer.Write((byte)unk2);
            buffer.Write(Encoding.Default.GetBytes(unk3));
            buffer.fill(0, 64 - unk3.Length); // Example: "Recall"
            buffer.Write(Encoding.Default.GetBytes(unk4));
            buffer.fill(0, 64 - unk4.Length); // Example "OdinRecall", "odinrecallimproved"
        }
    }
}