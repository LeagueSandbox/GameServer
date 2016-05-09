using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSurrender : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            Surrender surrender = new Surrender();
            game.PacketHandlerManager.broadcastPacketTeam(TeamId.TEAM_BLUE, surrender, Channel.CHL_S2C);
            return true;
        }
    }
}
