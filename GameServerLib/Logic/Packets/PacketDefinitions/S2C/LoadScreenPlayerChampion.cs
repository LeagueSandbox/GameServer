using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LoadScreenPlayerChampion : Packet
    {
        public LoadScreenPlayerChampion(Pair<uint, ClientInfo> p)
            : base(PacketCmd.PKT_S2C_LoadHero)
        {
            var player = p.Item2;
            buffer.Write((long)player.UserId);
            buffer.Write((int)player.SkinNo);
            buffer.Write((int)player.Champion.Model.Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Champion.Model))
                buffer.Write(b);
            buffer.Write((byte)0);
        }

        /*
         * long userId;
         * int skinId;
         * int length;
         * byte* championName;
         */
    }
}