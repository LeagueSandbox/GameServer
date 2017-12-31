using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase<EmptyClientPacket>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_Exit;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleExit(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacketInternal(Peer peer, EmptyClientPacket data)
        {
            var peerinfo = _playerManager.GetPeerInfo(peer);
            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SummonerDisconnected, peerinfo.Champion);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}