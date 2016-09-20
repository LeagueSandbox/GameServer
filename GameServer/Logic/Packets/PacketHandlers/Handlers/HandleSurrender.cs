using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSurrender : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _pm = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var c = _pm.GetPeerInfo(peer).Champion;
            Surrender surrender = new Surrender(c, 0x03, 1, 0, 5, c.Team, 10.0f);
            _game.PacketHandlerManager.broadcastPacketTeam(TeamId.TEAM_BLUE, surrender, Channel.CHL_S2C);
            return true;
        }
    }
}
