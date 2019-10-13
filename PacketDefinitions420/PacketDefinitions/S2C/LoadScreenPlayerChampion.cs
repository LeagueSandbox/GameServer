using GameServerCore.NetInfo;
using System;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LoadScreenPlayerChampion : Packet
    {
        public LoadScreenPlayerChampion(Tuple<uint, ClientInfo> p)
            : base(PacketCmd.PKT_S2C_LOAD_HERO)
        {
            var player = p.Item2;
            Write(player.PlayerId);
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