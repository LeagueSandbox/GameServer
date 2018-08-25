using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class Announce : BasePacket
    {
        public Announce(Announces messageId, int mapId = 0)
            : base(PacketCmd.PKT_S2C_ANNOUNCE)
        {
            Write((byte)messageId);
            Write((long)0);

            if (mapId > 0)
            {
                Write((int)mapId);
            }
        }
    }
}