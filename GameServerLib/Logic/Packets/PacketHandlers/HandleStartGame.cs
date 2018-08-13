using ENet;
using GameServerCore.Logic.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleStartGame : PacketHandlerBase
    {
        private readonly IPacketNotifier _packetNotifier;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_START_GAME;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleStartGame(Game game)
        {
            _packetNotifier = game.PacketNotifier;
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
                _packetNotifier.NotifyGameStart();

                foreach (var player in _playerManager.GetPlayers())
                {
                    if (player.Item2.Peer == peer && !player.Item2.IsMatchingVersion)
                    {
                        var msg = "Your client version does not match the server. " +
                                  "Check the server log for more information.";
                        _packetNotifier.NotifyDebugMessage(peer, msg);
                    }

                    _packetNotifier.NotifySetHealth(player.Item2.Champion);
                    // TODO: send this in one place only
                    _packetNotifier.NotifyUpdatedStats(player.Item2.Champion, false);
                    _packetNotifier.NotifyBlueTip(player.Item2.Peer, "Server Build Date", ServerContext.BuildDateString,
                        "", 0, player.Item2.Champion.NetId, _game.NetworkIdManager.GetNewNetId());
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
                            _packetNotifier.NotifyHeroSpawn2(peer, player.Item2.Champion);

                            /* This is probably not the best way
                             * of updating a champion's level, but it works */
                            _packetNotifier.NotifyLevelUp(player.Item2.Champion);
                            if (_game.IsPaused)
                            {
                                _packetNotifier.NotifyPauseGame((int)_game.PauseTimeLeft, true);
                            }
                        }
                    }
                    peerInfo.IsDisconnected = false;
                    _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_RECONNECTED, peerInfo.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                    _packetNotifier.NotifyGameTimer(peer, gameTime);
                    _packetNotifier.NotifyGameTimerUpdate(peer, gameTime);

                    return true;
                }

                foreach (var p in _playerManager.GetPlayers())
                {
                    _game.ObjectManager.AddObject((GameObject)p.Item2.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                    _packetNotifier.NotifyGameTimer(p.Item2.Peer, gameTime);
                    _packetNotifier.NotifyGameTimerUpdate(p.Item2.Peer, gameTime);
                }
            }

            return true;
        }
    }
}
