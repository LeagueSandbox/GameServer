using GameServerCore;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LoadScreenPlayerChampion : Packet
    {
        public LoadScreenPlayerChampion(Pair<uint, ClientInfo> p)
            : base(PacketCmd.PKT_S2C_LOAD_HERO)
        {
            var player = p.Item2;
            Write(player.UserId);
            Write(player.SkinNo);
            Write(player.Champion.Model.Length + 1);
			Write(player.Champion.Model);
            Write((byte)0);
        }

        /*
         * long userId;
         * int skinId;
         * int length;
         * byte* championName;
         */
    }
}