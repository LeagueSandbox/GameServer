using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using Ninject;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleUseObject : IPacketHandler
    {
        private Logger _logger = Program.Kernel.Get<Logger>();
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var parsedData = new UseObject(data);
            _logger.LogCoreInfo("Object " + game.GetPeerInfo(peer).GetChampion().getNetId() + " is trying to use (right clicked) " + parsedData.targetNetId);

            return true;
        }
    }
}
