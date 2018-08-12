using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Announce : BasePacket
    {
        public Announce(Announces messageId, int mapId = 0)
            : base(PacketCmd.PKT_S2C_Announce)
        {
            buffer.Write((byte)messageId);
            buffer.Write((long)0);

            if (mapId > 0)
            {
                buffer.Write(mapId);
            }
        }
    }
}