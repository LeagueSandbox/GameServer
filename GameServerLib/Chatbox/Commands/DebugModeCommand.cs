using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class DebugParticlesCommand : ChatCommandBase
    {
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;
        private readonly Game _game;
        private IChampion _userChampion;
        private static readonly Dictionary<uint, Particle> _circleParticles = new Dictionary<uint, Particle>();
        private static readonly Dictionary<uint, List<Particle>> _arrowParticlesList = new Dictionary<uint, List<Particle>>();
        private int _debugMode = 0;

        private static readonly object _particlesLock = new object();

        public override string Command => "debugmode";
        public override string Syntax => $"{Command} self/champions";

        public DebugParticlesCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
            _game = game;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            _userChampion = _playerManager.GetPeerInfo((ulong)userId).Champion;
            if (_debugMode != 0)
            {
                _debugMode = 0;

                if (_debugMode == 1)
                {
                    _logger.Debug($"Stopped debugging self.");
                }

                if (_debugMode == 2)
                {
                    _logger.Debug($"Stopped debugging champions.");
                }

                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Stopped debugging.");
                _game.PacketNotifier.NotifyRemoveUnitHighlight(userId, _userChampion);

                if (_circleParticles.Count != 0)
                {
                    lock (_particlesLock)
                    {
                        foreach (var particle in _circleParticles)
                        {
                            if (!particle.Value.IsToRemove())
                            {
                                particle.Value.SetToRemove();
                            }
                        }
                    }
                    _circleParticles.Clear();
                }
                if (_arrowParticlesList.Count != 0)
                {
                    lock (_particlesLock)
                    {
                        foreach (var particleList in _arrowParticlesList)
                        {
                            particleList.Value.ForEach(arrowparticle =>
                            {
                                if (!arrowparticle.IsToRemove())
                                {
                                    arrowparticle.SetToRemove();
                                }
                            });
                            particleList.Value.Clear();
                        }
                    }
                    _arrowParticlesList.Clear();
                }
            }
            else
            {
                var split = arguments.ToLower().Split(' ');

                if (split.Length < 2 || split.Length > 3)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                }
                else if (split[1].Contains("self"))
                {
                    _debugMode = 1;
                    DebugSelf(userId);
                }
                else if (split[1].Contains("champions"))
                {
                    _debugMode = 2;
                    DebugChampions(userId);
                }
                else
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                }
            }
        }


        // Draws your unit's collision radius and waypoints
        public void DebugSelf(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.CollisionRadius;

            _logger.Debug($"Started debugging self. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging self. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);

            for (var i = 0.0f; i < 86400.00f && _debugMode == 1; i += 0.1f) //debug for 24 hours, drawing every 100 milliseconds
            {
                if (_debugMode != 1)
                {
                    break;
                }

                // Yes I know, timer bad, but CreateTimer works based on the normal Update() loop of Game.cs, so it's not that bad.
                CreateTimer(i, () =>
                {
                    if (_debugMode != 1)
                    {
                        return;
                    }

                    // Clear circle particles every draw in case the unit changes its position
                    if (_circleParticles.ContainsKey(_userChampion.NetId))
                    {
                        if (_circleParticles[_userChampion.NetId] != null)
                        {
                            _circleParticles.Remove(_userChampion.NetId);
                        }
                    }

                    var circleparticle = new Particle(_game, _userChampion, new Target(_userChampion.GetPosition()), "DebugCircle_green.troy", circlesize, "", 0, default, 0.1f, false, false);
                    _circleParticles.Add(_userChampion.NetId, circleparticle);
                    _game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

                    if (_userChampion.Waypoints.Count > 0)
                    {
                        // Clear arrow particles every draw in case the unit changes its waypoints
                        if (_arrowParticlesList.ContainsKey(_userChampion.NetId))
                        {
                            if (_arrowParticlesList[_userChampion.NetId].Count != 0)
                            {
                                lock (_particlesLock)
                                {
                                    _arrowParticlesList[_userChampion.NetId].Clear();
                                }
                                _arrowParticlesList.Remove(_userChampion.NetId);
                            }
                        }

                        for (int waypoint = _userChampion.WaypointIndex; waypoint < _userChampion.Waypoints.Count; waypoint++)
                        {
                            var current = _userChampion.Waypoints[waypoint - 1];

                            var wpTarget = new Target(_userChampion.Waypoints[waypoint]);

                            // Makes the arrow point to the next waypoint
                            var to = Vector2.Normalize(new Vector2(wpTarget.X, wpTarget.Y) - current);
                            if (_userChampion.Waypoints.Count - 1 > waypoint)
                            {
                                var nextTargetWp = new Target(_userChampion.Waypoints[waypoint + 1]);
                                to = Vector2.Normalize(new Vector2(nextTargetWp.X, nextTargetWp.Y) - _userChampion.Waypoints[waypoint]);
                            }
                            var direction = new Vector3(to.X, 0, to.Y);

                            if (!_arrowParticlesList.ContainsKey(_userChampion.NetId))
                            {
                                _arrowParticlesList.Add(_userChampion.NetId, new List<Particle>());
                            }

                            var arrowparticle = new Particle(_game, _userChampion, wpTarget, "DebugArrow_green.troy", 0.5f, "", 0, direction, 0.1f, false, false);
                            _arrowParticlesList[_userChampion.NetId].Add(arrowparticle);

                            _game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);

                            if (waypoint >= _userChampion.Waypoints.Count)
                            {
                                _logger.Debug("Waypoints Drawn: " + waypoint);
                            }
                        }
                    }
                });
            }
        }

        // Draws the collision radius and waypoints of all champions
        public void DebugChampions(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.CollisionRadius;

            var champions = Game.ObjectManager.GetAllChampions();

            _logger.Debug($"Started debugging " + champions.Count + " champions. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging " + champions.Count + " champions. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);

            for (var i = 0.0f; i < 86400.00f && _debugMode == 2; i += 0.1f) //debug for 24 hours, drawing every 100 milliseconds
            {
                if (_debugMode != 2)
                {
                    break;
                }

                // Yes I know, timer bad, but CreateTimer works based on the normal Update() loop of Game.cs, so it's not that bad.
                CreateTimer(i, () =>
                {
                    if (_debugMode != 2)
                    {
                        return;
                    }

                    // Same method as DebugSelf just for every champion
                    foreach (var champion in champions)
                    {
                        // Clear circle particles every draw in case the unit changes its position
                        if (_circleParticles.ContainsKey(champion.NetId))
                        {
                            if (_circleParticles[champion.NetId] != null)
                            {
                                _circleParticles.Remove(champion.NetId);
                            }
                        }
                        circlesize = (1f / 100f) * champion.CollisionRadius;

                        var circleparticle = new Particle(_game, champion, new Target(champion.GetPosition()), "DebugCircle_green.troy", circlesize, "", 0, default, 0, false, false);
                        _circleParticles.Add(champion.NetId, circleparticle);
                        _game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

                        if (champion.Waypoints.Count > 0)
                        {
                            // Clear arrow particles every draw in case the unit changes its waypoints
                            if (_arrowParticlesList.ContainsKey(champion.NetId))
                            {
                                if (_arrowParticlesList[champion.NetId].Count != 0)
                                {
                                    lock (_particlesLock)
                                    {
                                        _arrowParticlesList[champion.NetId].Clear();
                                    }
                                    _arrowParticlesList.Remove(champion.NetId);
                                }
                            }

                            for (int waypoint = champion.WaypointIndex; waypoint < champion.Waypoints.Count; waypoint++)
                            {
                                var current = champion.Waypoints[waypoint - 1];

                                var wpTarget = new Target(champion.Waypoints[waypoint]);

                                // Makes the arrow point to the next waypoint
                                var to = Vector2.Normalize(new Vector2(wpTarget.X, wpTarget.Y) - current);
                                if (champion.Waypoints.Count - 1 > waypoint)
                                {
                                    var nextTargetWp = new Target(champion.Waypoints[waypoint + 1]);
                                    to = Vector2.Normalize(new Vector2(nextTargetWp.X, nextTargetWp.Y) - champion.Waypoints[waypoint]);
                                }
                                var direction = new Vector3(to.X, 0, to.Y);

                                if (!_arrowParticlesList.ContainsKey(champion.NetId))
                                {
                                    _arrowParticlesList.Add(champion.NetId, new List<Particle>());
                                }

                                var arrowparticle = new Particle(_game, champion, wpTarget, "DebugArrow_green.troy", 0.5f, "", 0, direction, 0.1f, false, false);
                                _arrowParticlesList[champion.NetId].Add(arrowparticle);

                                _game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);

                                if (waypoint >= champion.Waypoints.Count)
                                {
                                    _logger.Debug("Waypoints Drawn: " + waypoint);
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}