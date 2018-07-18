using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class WorldSendGameNumber : BasePacket
    {
        public WorldSendGameNumber(Game game, long gameId, string name)
            : base(game, PacketCmd.PKT_S2C_WORLD_SEND_GAME_NUMBER)
        {
            Write(gameId);
            WriteConstLengthString(name, 128);
        }
    }
}