using ENet;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleStartGame : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_START_GAME;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleStartGame(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var peerInfo = _playerManager.GetPeerInfo(peer);

            if (!peerInfo.IsDisconnected)
            {
                _game.IncrementReadyPlayers();
            }

            /* if (_game.IsRunning)
                return true; */

            if (_game.PlayersReady == _playerManager.GetPlayers().Count)
            {
                 _game.PacketNotifier.NotifyGameStart();

                foreach (var player in _playerManager.GetPlayers())
                {
                    if (player.Item2.Peer == peer && !player.Item2.IsMatchingVersion)
                    {
                        var msg = "Your client version does not match the server. " +
                                  "Check the server log for more information.";
                         _game.PacketNotifier.NotifyDebugMessage(peer, msg);
                    }

                    _game.PacketNotifier.NotifySetHealth(player.Item2.Champion);
                    // TODO: send this in one place only
                    _game.PacketNotifier.NotifyUpdatedStats(player.Item2.Champion, false);
                    _game.PacketNotifier.NotifyBlueTip(player.Item2.Peer, "Welcome to League Sandbox!",
                        "This is a WIP product.", "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                    _game.PacketNotifier.NotifyBlueTip(player.Item2.Peer, "Server Build Date",
                        ServerContext.BuildDateString, "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                }

                _game.Start();
            }

            if (_game.IsRunning)
            {
                if (peerInfo.IsDisconnected)
                {
                    foreach (var player in _playerManager.GetPlayers())
                    {
                        if (player.Item2.Team == peerInfo.Team)
                        {
                             _game.PacketNotifier.NotifyHeroSpawn2(peer, player.Item2.Champion);

                            /* This is probably not the best way
                             * of updating a champion's level, but it works */
                             _game.PacketNotifier.NotifyLevelUp(player.Item2.Champion);
                            if (_game.IsPaused)
                            {
                                 _game.PacketNotifier.NotifyPauseGame((int)_game.PauseTimeLeft, true);
                            }
                        }
                    }
                    peerInfo.IsDisconnected = false;
                    _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_RECONNECTED, peerInfo.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                     _game.PacketNotifier.NotifyGameTimer(peer, gameTime);
                     _game.PacketNotifier.NotifyGameTimerUpdate(peer, gameTime);

                    return true;
                }

                foreach (var p in _playerManager.GetPlayers())
                {
                    _game.ObjectManager.AddObject((GameObject)p.Item2.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                     _game.PacketNotifier.NotifyGameTimer(p.Item2.Peer, gameTime);
                     _game.PacketNotifier.NotifyGameTimerUpdate(p.Item2.Peer, gameTime);
                }
            }

            return true;
        }
    }
}
