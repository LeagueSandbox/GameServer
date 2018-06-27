using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LoadScreenPlayerChampion : Packet
    {
        public LoadScreenPlayerChampion(Pair<uint, ClientInfo> p)
            : base(PacketCmd.PKT_S2_C_LOAD_HERO)
        {
            var player = p.Item2;
            _buffer.Write(player.UserId);
            _buffer.Write(player.SkinNo);
            _buffer.Write(player.Champion.Model.Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Champion.Model))
                _buffer.Write(b);
            _buffer.Write((byte)0);
        }

        /*
         * long userId;
         * int skinId;
         * int length;
         * byte* championName;
         */
    }
}