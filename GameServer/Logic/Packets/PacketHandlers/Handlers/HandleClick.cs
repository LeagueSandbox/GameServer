using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using Ninject;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleClick : IPacketHandler
    {
        private Logger _logger = Program.Kernel.Get<Logger>();

        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var click = new Click(data);
            _logger.LogCoreInfo("Object " + game.GetPeerInfo(peer).GetChampion().getNetId() + " clicked on " + click.targetNetId);

            return true;
        }
    }
}
