using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions
{
    public abstract class BasePacket : Packet
    {
        protected BasePacket(PacketCmd cmd, uint netId = 0) : base(cmd)
        {
            Write(netId);
            if ((short)cmd > 0xFF) // Make an extended packet instead
            {
                _bytes[0] = (byte)PacketCmd.PKT_S2C_EXTENDED;
                Write((short)cmd);
            }
        }
    }
}