using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Quest : BasePacket
    {
        public Quest(string title, string description, byte type, byte command, uint netid, byte questEvent = 0)
            : base(PacketCmd.PKT_S2_C_QUEST)
        {
            _buffer.Write(Encoding.Default.GetBytes(title));
            _buffer.Fill(0, 256 - title.Length);
            _buffer.Write(Encoding.Default.GetBytes(description));
            _buffer.Fill(0, 256 - description.Length);
            _buffer.Write(type); // 0 : Primary quest, 1 : Secondary quest
            _buffer.Write(command); // 0 : Activate quest, 1 : Complete quest, 2 : Remove quest
            _buffer.Write(questEvent); // 0 : Roll over, 1 : Roll out, 2 : Mouse down, 3 : Mouse up
            _buffer.Write((int)netid);
        }
    }
}