using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class KeyCheckResponse : Packet
    {
        public KeyCheckResponse(long userId, int playerNo) 
            : base(PacketCmd.PKT_KeyCheck)
        {
            buffer.Write((byte)0x2A);
            buffer.Write((byte)0);
            buffer.Write((byte)0xFF);
            buffer.Write((uint)playerNo);
            buffer.Write((ulong)userId);
            buffer.Write((uint)0);
            buffer.Write((long)0);
            buffer.Write((uint)0);
        }
    }
}