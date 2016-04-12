using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSurrender : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            GameEnd surrender = new GameEnd(TeamId.TEAM_PURPLE);
            foreach (var p in game.getPlayers())
            {
                PacketHandlerManager.getInstace().sendPacket(peer, surrender, Channel.CHL_S2C);
            }
            return true;
        }
    }
}
