using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class KeyCheckResponse : Packet
    {
        public KeyCheckResponse(ulong playerId, uint clientId)
            : base(PacketCmd.PKT_KEY_CHECK)
        {
            Write((byte)0x2A);
            Write((byte)0);
            Write((byte)0xFF);
            Write(clientId);
            Write(playerId);
            Write((uint)0);
            Write((long)0);
            Write((uint)0);
        }
    }
}