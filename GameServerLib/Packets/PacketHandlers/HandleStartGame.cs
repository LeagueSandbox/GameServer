using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

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

            if (!peerInfo.IsDisconnected)
            {
                _game.IncrementReadyPlayers();
            }

            /* if (_game.IsRunning)
                return true; */
            
            // Only one packet enter here
            if (_game.PlayersReady == _playerManager.GetPlayers().Count)
            {
                _game.PacketNotifier.NotifyGameStart();

                foreach (var player in _playerManager.GetPlayers())
                {
                    // Get notified about the spawn of other connected players - IMPORTANT: should only occur one-time
                    foreach (var p in _playerManager.GetPlayers(true))
                    {
                        if (!p.Item2.IsStartedClient) continue; //user still didn't connect, not get informed about it
                        if (player.Item2.PlayerId == p.Item2.PlayerId) continue; //Don't self-inform twice
                        _game.PacketNotifier.NotifyS2C_CreateHero(p.Item2, (int)player.Item2.PlayerId);
                        _game.PacketNotifier.NotifyAvatarInfo(p.Item2, (int)player.Item2.PlayerId);
                    }

                    if (player.Item2.PlayerId == userId && !player.Item2.IsMatchingVersion)
                    {
                        var msg = "Your client version does not match the server. " +
                                  "Check the server log for more information.";
                         _game.PacketNotifier.NotifyS2C_SystemMessage(userId, msg);
                    }

                    _game.PacketNotifier.NotifySpawn(player.Item2.Champion, userId, false);
                    
                    while(player.Item2.Champion.Stats.Level < _game.Map.MapScript.MapScriptMetadata.InitialLevel)
                    {
                        player.Item2.Champion.LevelUp(true);
                    }

                    // TODO: send this in one place only
                    _game.PacketNotifier.NotifyS2C_HandleTipUpdatep((int) player.Item2.PlayerId, "Welcome to League Sandbox!",
                        "This is a WIP project.", "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                    _game.PacketNotifier.NotifyS2C_HandleTipUpdatep((int) player.Item2.PlayerId, "Server Build Date",
                        ServerContext.BuildDateString, "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                    _game.PacketNotifier.NotifyS2C_HandleTipUpdatep((int)player.Item2.PlayerId, "Your Champion:",
                        player.Item2.Champion.Model, "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                }
                _game.Start();
            }

            if (_game.IsRunning)
            {
                if (peerInfo.IsDisconnected)
                {
                    foreach (var player in _playerManager.GetPlayers(true))
                    {
                        if (player.Item2.Team == peerInfo.Team)
                        {
                            _game.PacketNotifier.NotifySpawn(player.Item2.Champion, userId, false);

                            /* This is probably not the best way
                             * of updating a champion's level, but it works */
                            _game.PacketNotifier.NotifyNPC_LevelUp(player.Item2.Champion);
                            if (_game.IsPaused)
                            {
                                 _game.PacketNotifier.NotifyPausePacket(peerInfo, (int)_game.PauseTimeLeft, true);
                            }
                        }
                    }
                    peerInfo.IsDisconnected = false;
                    _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_RECONNECTED, peerInfo.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                     _game.PacketNotifier.NotifySynchSimTimeS2C(userId, gameTime);
                     _game.PacketNotifier.NotifySyncMissionStartTimeS2C(userId, gameTime);

                    return true;
                }

                foreach (var p in _playerManager.GetPlayers(true))
                {
                    _game.ObjectManager.AddObject(p.Item2.Champion);

                    if (p.Item2.Champion.IsBot)
                    {
                        continue;
                    }

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                     _game.PacketNotifier.NotifySynchSimTimeS2C((int) p.Item2.PlayerId, gameTime);
                     _game.PacketNotifier.NotifySyncMissionStartTimeS2C((int) p.Item2.PlayerId, gameTime);
                }
            }

            return true;
        }
    }
}
