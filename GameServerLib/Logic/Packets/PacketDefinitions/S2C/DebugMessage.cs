using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DebugMessage : BasePacket
    {
        public DebugMessage(string message)
            : base(PacketCmd.PKT_S2_C_DEBUG_MESSAGE)
        {
            _buffer.Write((int)0);
            foreach (var b in Encoding.Default.GetBytes(message))
                _buffer.Write((byte)b);
            _buffer.Fill(0, 512 - message.Length);
        }
    }
}