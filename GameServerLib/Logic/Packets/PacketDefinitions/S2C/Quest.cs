using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Quest : BasePacket
    {
        public Quest(string title, string description, byte type, byte command, uint netid, byte questEvent = 0)
            : base(PacketCmd.PKT_S2C_Quest)
        {
            buffer.Write(Encoding.Default.GetBytes(title));
            buffer.fill(0, 256 - title.Length);
            buffer.Write(Encoding.Default.GetBytes(description));
            buffer.fill(0, 256 - description.Length);
            buffer.Write((byte)type); // 0 : Primary quest, 1 : Secondary quest
            buffer.Write((byte)command); // 0 : Activate quest, 1 : Complete quest, 2 : Remove quest
            buffer.Write((byte)questEvent); // 0 : Roll over, 1 : Roll out, 2 : Mouse down, 3 : Mouse up
            buffer.Write((int)netid);
        }
    }
}