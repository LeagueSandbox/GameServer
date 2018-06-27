using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Announce : BasePacket
    {
        public Announce(Announces messageId, int mapId = 0)
            : base(PacketCmd.PKT_S2C_ANNOUNCE)
        {
            _buffer.Write((byte)messageId);
            _buffer.Write((long)0);

            if (mapId > 0)
            {
                _buffer.Write((int)mapId);
            }
        }
    }
}