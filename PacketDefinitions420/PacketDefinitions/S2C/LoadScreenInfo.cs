using System.Collections.Generic;
using GameServerCore;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LoadScreenInfo : Packet
    {
        public LoadScreenInfo(List<Pair<uint, ClientInfo>> players)
            : base(PacketCmd.PKT_S2C_LOAD_SCREEN_INFO)
        {
            //Zero this complete buffer
            Write((uint)6); // blueMax
            Write((uint)6); // redMax

            var currentBlue = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.Team == TeamId.TEAM_BLUE)
                {
                    Write((ulong)player.UserId);
                    currentBlue++;
                }
            }

            for (var i = 0; i < 6 - currentBlue; ++i)
                Write((ulong)0);

            Fill(0, 144);

            var currentPurple = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.Team == TeamId.TEAM_PURPLE)
                {
                    Write((ulong)player.UserId);
                    currentPurple++;
                }
            }

            for (var i = 0; i < 6 - currentPurple; ++i)
            {
                Write((ulong)0);
            }

            Fill(0, 144);
            Write(currentBlue);
            Write(currentPurple);
        }
    }
}