using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class WorldSendGameNumber : BasePacket
    {
        public WorldSendGameNumber(long gameId, string name)
            : base(PacketCmd.PKT_S2C_WORLD_SENDGame_NUMBER)
        {
            Write(gameId);
            WriteConstLengthString(name, 128);
        }
    }
}