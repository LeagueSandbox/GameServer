using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using Ninject;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleClick : IPacketHandler
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var click = new Click(data);
            _logger.LogCoreInfo(string.Format(
                "Object {0} clicked on {1}",
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                click.targetNetId
            ));

            return true;
        }
    }
}
