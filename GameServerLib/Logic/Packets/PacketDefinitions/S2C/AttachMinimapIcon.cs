using System.IO;
using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttachMinimapIcon : BasePacket
    {
        public AttachMinimapIcon(Game game, AttackableUnit unit, byte unk1, string iconName, byte unk2, string unk3, string unk4)
            : base(game, PacketCmd.PKT_S2C_ATTACH_MINIMAP_ICON)
        {
            WriteNetId(unit);
            Write((byte)unk1);
            WriteConstLengthString(iconName, 64); // This is probably the icon name, but sometimes it's empty; Example: "Quest"
            Write((byte)unk2);
            WriteConstLengthString(unk3, 64); // Example: "Recall"
            WriteConstLengthString(unk4, 64); // Example "OdinRecall", "odinrecallimproved"
        }
    }
}