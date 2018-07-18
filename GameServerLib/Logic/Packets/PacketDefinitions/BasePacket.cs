using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions
{
    public abstract class BasePacket : Packet
    {
        protected BasePacket(Game game, PacketCmd cmd, uint netId = 0) : base(game, cmd)
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