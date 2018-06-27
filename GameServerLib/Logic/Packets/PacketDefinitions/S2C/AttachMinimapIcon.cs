using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttachMinimapIcon : BasePacket
    {
        public AttachMinimapIcon(AttackableUnit unit, byte unk1, string iconName, byte unk2, string unk3, string unk4)
            : base(PacketCmd.PKT_S2_C_ATTACH_MINIMAP_ICON)
        {
            _buffer.Write(unit.NetId);
            _buffer.Write(unk1);
            _buffer.Write(Encoding.Default.GetBytes(iconName)); // This is probably the icon name, but sometimes it's empty
            _buffer.Fill(0, 64 - iconName.Length);              // Example: "Quest"
            _buffer.Write(unk2);
            _buffer.Write(Encoding.Default.GetBytes(unk3));
            _buffer.Fill(0, 64 - unk3.Length); // Example: "Recall"
            _buffer.Write(Encoding.Default.GetBytes(unk4));
            _buffer.Fill(0, 64 - unk4.Length); // Example "OdinRecall", "odinrecallimproved"
        }
    }
}