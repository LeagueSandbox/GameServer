using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using IntWarsSharp.Logic.Packets;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleStartGame : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            game.IncrementReadyPlayers();
            if (game.getReadyPlayers() == game.getPlayers().Count)
            {
                var start = new StatePacket(PacketCmdS2C.PKT_S2C_StartGame);
                PacketHandlerManager.getInstace().broadcastPacket(start, Channel.CHL_S2C);

                foreach (var player in game.getPlayers())
                {
                    if (player.Item2.getPeer() == peer && !player.Item2.isVersionMatch())
                    {
                        var dm = new SpawnParticle.DebugMessage("Your client version does not match the server. Check the server log for more information.");
                        PacketHandlerManager.getInstace().sendPacket(peer, dm, Channel.CHL_S2C);
                    }
                }

                game.setStarted(true);
            }

            if (game.isStarted())
            {
                foreach (var p in game.getPlayers())
                {
                    var map = game.getMap();
                    map.addObject(p.Item2.getChampion());

                    // Send the initial game time sync packets, then let the map send another
                    float gameTime = map.getGameTime() / 1000000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    PacketHandlerManager.getInstace().sendPacket(p.Item2.getPeer(), timer, Channel.CHL_S2C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    PacketHandlerManager.getInstace().sendPacket(p.Item2.getPeer(), timer2, Channel.CHL_S2C);
                }
            }

            return true;
        }
    }
}
