﻿using GameServerCore;
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
            var peerInfo = _playerManager.GetPeerInfo((ulong)userId);

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
                    foreach (var p in _playerManager.GetPlayers())
                    {
                        if (!p.Item2.IsStartedClient) continue; //user still didn't connect, not get informed about it
                        if (player.Item2.PlayerId == p.Item2.PlayerId) continue; //Don't self-inform twice
                        _game.PacketNotifier.NotifyHeroSpawn((int)player.Item2.PlayerId, p.Item2);
                        _game.PacketNotifier.NotifyAvatarInfo((int)player.Item2.PlayerId, p.Item2);
                    }

                    if (player.Item2.PlayerId == (ulong)userId && !player.Item2.IsMatchingVersion)
                    {
                        var msg = "Your client version does not match the server. " +
                                  "Check the server log for more information.";
                         _game.PacketNotifier.NotifyDebugMessage(userId, msg);
                    }

                    _game.PacketNotifier.NotifyEnterLocalVisibilityClient(player.Item2.Champion);
                    // TODO: send this in one place only
                    _game.PacketNotifier.NotifyUpdatedStats(player.Item2.Champion, false);
                    _game.PacketNotifier.NotifyBlueTip((int) player.Item2.PlayerId, "Welcome to League Sandbox!",
                        "This is a WIP product.", "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                    _game.PacketNotifier.NotifyBlueTip((int) player.Item2.PlayerId, "Server Build Date",
                        ServerContext.BuildDateString, "", 0, player.Item2.Champion.NetId,
                        _game.NetworkIdManager.GetNewNetId());
                    _game.PacketNotifier.NotifyBlueTip((int)player.Item2.PlayerId, "Your Champion",
                        "You play "+ player.Item2.Champion.Model, "", 0, player.Item2.Champion.NetId,
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
                             _game.PacketNotifier.NotifyHeroSpawn2(userId, player.Item2.Champion);

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
                     _game.PacketNotifier.NotifyGameTimer(userId, gameTime);
                     _game.PacketNotifier.NotifyGameTimerUpdate(userId, gameTime);

                    return true;
                }

                foreach (var p in _playerManager.GetPlayers())
                {
                    _game.ObjectManager.AddObject(p.Item2.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = _game.GameTime;
                     _game.PacketNotifier.NotifyGameTimer((int) p.Item2.PlayerId, gameTime);
                     _game.PacketNotifier.NotifyGameTimerUpdate((int) p.Item2.PlayerId, gameTime);
                }
            }

            return true;
        }
    }
}
