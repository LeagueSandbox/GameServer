using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class WorldSendGameNumber : BasePacket
    {
        public WorldSendGameNumber(long gameId, string name)
            : base(PacketCmd.PKT_S2_C_WORLD_SEND_GAME_NUMBER)
        {
            var data = Encoding.Default.GetBytes(name);
            _buffer.Write(gameId);
            foreach (var d in data)
                _buffer.Write(d);
            _buffer.Fill(0, 128 - data.Length);
        }
    }
}