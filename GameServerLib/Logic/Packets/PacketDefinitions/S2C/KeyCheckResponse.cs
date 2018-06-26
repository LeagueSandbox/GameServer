using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class KeyCheckResponse : Packet
    {
        public KeyCheckResponse(long userId, int playerNo) 
            : base(PacketCmd.PKT_KEY_CHECK)
        {
            _buffer.Write((byte)0x2A);
            _buffer.Write((byte)0);
            _buffer.Write((byte)0xFF);
            _buffer.Write((uint)playerNo);
            _buffer.Write((ulong)userId);
            _buffer.Write((uint)0);
            _buffer.Write((long)0);
            _buffer.Write((uint)0);
        }
    }
}