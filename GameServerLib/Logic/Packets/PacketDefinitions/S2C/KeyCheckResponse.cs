using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class KeyCheckResponse : Packet
    {
        public KeyCheckResponse(Game game, long userId, int playerNo)
            : base(game, PacketCmd.PKT_KEY_CHECK)
        {
            Write((byte)0x2A);
            Write((byte)0);
            Write((byte)0xFF);
            Write((uint)playerNo);
            Write((ulong)userId);
            Write((uint)0);
            Write((long)0);
            Write((uint)0);
        }
    }
}