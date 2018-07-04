using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LoadScreenPlayerName : Packet
    {
        public LoadScreenPlayerName(Pair<uint, ClientInfo> player)
            : base(PacketCmd.PKT_S2C_LOAD_NAME)
        {
            Write(player.Item2.UserId);
            Write(0);
            Write(player.Item2.Name.Length + 1);
			Write(player.Item2.Name);
            Write((byte)0);
        }

        /*
         * long userId;
         * int unk1; // most likly not skinId ?
         * int length;
         * byte* playerName;
         */
    }
}