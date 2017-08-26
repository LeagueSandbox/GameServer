using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LoadScreenPlayerName : Packet
    {
        public LoadScreenPlayerName(Pair<uint, ClientInfo> player)
            : base(PacketCmd.PKT_S2C_LoadName)
        {
            buffer.Write((long)player.Item2.UserId);
            buffer.Write((int)0);
            buffer.Write((int)player.Item2.Name.Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Item2.Name))
                buffer.Write(b);
            buffer.Write((byte)0);
        }

        /*
         * long userId;
         * int unk1; // most likly not skinId ?
         * int length;
         * byte* playerName;
         */
    }
}