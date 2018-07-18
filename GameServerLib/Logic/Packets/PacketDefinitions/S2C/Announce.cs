using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class Announce : BasePacket
    {
        public Announce(Game game, Announces messageId, int mapId = 0)
            : base(game, PacketCmd.PKT_S2C_ANNOUNCE)
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