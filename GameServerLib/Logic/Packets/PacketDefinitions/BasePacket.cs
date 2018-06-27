using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions
{
    public abstract class BasePacket : Packet
    {
        protected BasePacket(PacketCmd cmd = PacketCmd.PKT_KEY_CHECK, uint netId = 0) : base(cmd)
        {
            _buffer.Write(netId);
            if ((short)cmd > 0xFF) // Make an extended packet instead
            {
                var oldPosition = _buffer.BaseStream.Position;
                _buffer.BaseStream.Position = 0;
                _buffer.BaseStream.Write(new[] { (byte)PacketCmd.PKT_S2_C_EXTENDED }, 0, 1);
                _buffer.BaseStream.Position = oldPosition;
                _buffer.Write((short)cmd);
            }
        }
    }
}