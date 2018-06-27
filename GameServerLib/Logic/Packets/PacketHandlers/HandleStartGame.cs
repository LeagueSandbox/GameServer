using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleStartGame : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_START_GAME;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleStartGame(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
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
                var start = new StatePacket(PacketCmd.PKT_S2C_START_GAME);
                _game.PacketHandlerManager.BroadcastPacket(start, Channel.CHL_S2_C);

                foreach (var player in _playerManager.GetPlayers())
                {
                    if (player.Item2.Peer == peer && !player.Item2.IsMatchingVersion)
                    {
                        var msg = "Your client version does not match the server. " +
                                  "Check the server log for more information.";
                        var dm = new DebugMessage(msg);
                        _game.PacketHandlerManager.SendPacket(peer, dm, Channel.CHL_S2_C);
                    }
                    _game.PacketNotifier.NotifySetHealth(player.Item2.Champion);
                    // TODO: send this in one place only
                    _game.PacketNotifier.NotifyUpdatedStats(player.Item2.Champion, false);
                }

                _game.Start();
            }

            if (_game.IsRunning)
            {
                var map = _game.Map;
                if (peerInfo.IsDisconnected)
                {
                    foreach (var player in _playerManager.GetPlayers())
                    {
                        if (player.Item2.Team == peerInfo.Team)
                        {
                            var heroSpawnPacket = new HeroSpawn2(player.Item2.Champion);
                            _game.PacketHandlerManager.SendPacket(peer, heroSpawnPacket, Channel.CHL_S2_C);

                            /* This is probably not the best way
                             * of updating a champion's level, but it works */
                            var levelUpPacket = new LevelUp(player.Item2.Champion);
                            _game.PacketHandlerManager.SendPacket(peer, levelUpPacket, Channel.CHL_S2_C);
                            if (_game.IsPaused)
                            {
                                var pausePacket = new PauseGame((int)_game.PauseTimeLeft, true);
                                _game.PacketHandlerManager.SendPacket(peer, pausePacket, Channel.CHL_S2_C);
                            }
                        }
                    }
                    peerInfo.IsDisconnected = false;
                    _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_RECONNECTED, peerInfo.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime / 1000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    _game.PacketHandlerManager.SendPacket(peer, timer, Channel.CHL_S2_C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    _game.PacketHandlerManager.SendPacket(peer, timer2, Channel.CHL_S2_C);

                    return true;
                }

                foreach (var p in _playerManager.GetPlayers())
                {
                    _game.ObjectManager.AddObject(p.Item2.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime / 1000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    _game.PacketHandlerManager.SendPacket(p.Item2.Peer, timer, Channel.CHL_S2_C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    _game.PacketHandlerManager.SendPacket(p.Item2.Peer, timer2, Channel.CHL_S2_C);
                }
            }

            return true;
        }
    }
}
