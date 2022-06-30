using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeaguePackets.Game.Events;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleStartGame : PacketHandlerBase<StartGameRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleStartGame(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, StartGameRequest req)
        {
            var peerInfo = _playerManager.GetPeerInfo(userId);

            if (_game.IsRunning)
            {
                if (_game.IsPaused)
                {
                    _game.PacketNotifier.NotifyPausePacket(peerInfo, (int)_game.PauseTimeLeft, true);
                }
                _game.PacketNotifier.NotifyGameStart(userId);

                if (peerInfo.IsDisconnected)
                {
                    peerInfo.IsDisconnected = false;

                    var announcement = new OnReconnect { OtherNetID = peerInfo.Champion.NetId };
                    _game.PacketNotifier.NotifyS2C_OnEventWorld(announcement, peerInfo.Champion);
                }

                SyncTime(userId);

                return true;
            }
            else
            {
                if (!peerInfo.IsDisconnected)
                {
                    _game.IncrementReadyPlayers();
                }

                // Only one packet enter here
                if (_game.PlayersReady == _playerManager.GetPlayers().Count)
                {
                    _game.PacketNotifier.NotifyGameStart();

                    foreach (var player in _playerManager.GetPlayers())
                    {
                        if (player.ClientId == userId && !player.IsMatchingVersion)
                        {
                            var msg = "Your client version does not match the server. " +
                                    "Check the server log for more information.";
                            _game.PacketNotifier.NotifyS2C_SystemMessage(userId, msg);
                        }

                        while (player.Champion.Stats.Level < _game.Map.MapScript.MapScriptMetadata.InitialLevel)
                        {
                            player.Champion.LevelUp(true);
                        }

                        // TODO: send this in one place only
                        _game.PacketNotifier.NotifyS2C_HandleTipUpdate(player.ClientId, "Welcome to League Sandbox!",
                            "This is a WIP project.", "", 0, player.Champion.NetId,
                            _game.NetworkIdManager.GetNewNetId());
                        _game.PacketNotifier.NotifyS2C_HandleTipUpdate(player.ClientId, "Server Build Date",
                            ServerContext.BuildDateString, "", 0, player.Champion.NetId,
                            _game.NetworkIdManager.GetNewNetId());
                        _game.PacketNotifier.NotifyS2C_HandleTipUpdate(player.ClientId, "Your Champion:",
                            player.Champion.Model, "", 0, player.Champion.NetId,
                            _game.NetworkIdManager.GetNewNetId());

                        SyncTime(player.ClientId);
                    }
                    _game.Start();
                }
            }
            return true;
        }

        void SyncTime(int userId)
        {
            // Send the initial game time sync packets, then let the map send another
            var gameTime = _game.GameTime;
            _game.PacketNotifier.NotifySynchSimTimeS2C(userId, gameTime);
            _game.PacketNotifier.NotifySyncMissionStartTimeS2C(userId, gameTime);
        }
    }
}