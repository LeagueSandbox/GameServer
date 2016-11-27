using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

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
