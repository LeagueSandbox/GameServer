using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleExit : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var peerinfo = _playerManager.GetPeerInfo(peer);
            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SummonerDisconnected, peerinfo.Champion);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}