﻿using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleMap : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLIENT_READY;
        public override Channel PacketChannel => Channel.CHL_LOADING_SCREEN;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            // Builds team info e.g. first UserId set on Blue has PlayerId 0
            // increment by 1 for each added player
            var screenInfo = new LoadScreenInfo(PlayerManager.GetPlayers());
            var pInfo = Game.PacketHandlerManager.SendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);

            // Distributes each players info by UserId
            var bOk = false;
            foreach (var player in PlayerManager.GetPlayers())
            {
                // Giving the UserId in loading screen a name
                var loadName = new LoadScreenPlayerName(player);
                // Giving the UserId in loading screen a champion
                var loadChampion = new LoadScreenPlayerChampion(player);
                var pName = Game.PacketHandlerManager.SendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
                var pHero = Game.PacketHandlerManager.SendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);

                bOk = pName && pHero;

                if (!bOk)
                {
                    break;
                }
            }

            return pInfo && bOk;
        }
    }
}
