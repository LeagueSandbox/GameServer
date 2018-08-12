using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public abstract class BasePacket : Packet
    {
        protected BasePacket(PacketCmd cmd = PacketCmd.PKT_KeyCheck, uint netId = 0) : base(cmd)
        {
            buffer.Write((uint)netId);
            if ((short)cmd > 0xFF) // Make an extended packet instead
            {
                var oldPosition = buffer.BaseStream.Position;
                buffer.BaseStream.Position = 0;
                buffer.BaseStream.Write(new byte[] { (byte)PacketCmd.PKT_S2C_Extended }, 0, 1);
                buffer.BaseStream.Position = oldPosition;
                buffer.Write((short)cmd);
            }
        }
    }
}