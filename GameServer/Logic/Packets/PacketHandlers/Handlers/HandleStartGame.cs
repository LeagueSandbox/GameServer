using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleStartGame : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            if (game.IsStarted())
                return true;

            game.IncrementReadyPlayers();
            if (game.GetReadyPlayers() == game.GetPlayers().Count)
            {
                var start = new StatePacket(PacketCmdS2C.PKT_S2C_StartGame);
                game.GetPacketHandlerManager().broadcastPacket(start, Channel.CHL_S2C);

                foreach (var player in game.GetPlayers())
                {
                    if (player.Item2.GetPeer() == peer && !player.Item2.IsVersionMatch())
                    {
                        var dm = new DebugMessage("Your client version does not match the server. Check the server log for more information.");
                        game.GetPacketHandlerManager().sendPacket(peer, dm, Channel.CHL_S2C);
                    }
                }

                game.SetStarted(true);
            }

            if (game.IsStarted())
            {
                foreach (var p in game.GetPlayers())
                {
                    var map = game.GetMap();
                    map.AddObject(p.Item2.GetChampion());

                    // Send the initial game time sync packets, then let the map send another
                    float gameTime = map.getGameTime() / 1000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    game.GetPacketHandlerManager().sendPacket(p.Item2.GetPeer(), timer, Channel.CHL_S2C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    game.GetPacketHandlerManager().sendPacket(p.Item2.GetPeer(), timer2, Channel.CHL_S2C);
                }
            }

            return true;
        }
    }
}
