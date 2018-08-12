using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LoadScreenInfo : Packet
    {
        public LoadScreenInfo(List<Pair<uint, ClientInfo>> players) 
            : base(PacketCmd.PKT_S2C_LoadScreenInfo)
        {
            //Zero this complete buffer
            buffer.Write((uint)6); // blueMax
            buffer.Write((uint)6); // redMax

            int currentBlue = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.Team == TeamId.TEAM_BLUE)
                {
                    buffer.Write((ulong)player.UserId);
                    currentBlue++;
                }
            }

            for (var i = 0; i < 6 - currentBlue; ++i)
                buffer.Write((ulong)0);

            buffer.fill(0, 144);

            int currentPurple = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.Team == TeamId.TEAM_PURPLE)
                {
                    buffer.Write((ulong)player.UserId);
                    currentPurple++;
                }
            }

            for (int i = 0; i < 6 - currentPurple; ++i)
            {
                buffer.Write((ulong)0);
            }

            buffer.fill(0, 144);
            buffer.Write(currentBlue);
            buffer.Write(currentPurple);
        }
    }
}