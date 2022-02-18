using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class DebugParticlesCommand : ChatCommandBase
    {
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;
        private readonly Game _game;
        private float lastDrawTime;
        private int _userId;
        private IChampion _userChampion;
        private static readonly Dictionary<uint, Particle> _circleParticles = new Dictionary<uint, Particle>();
        private static readonly Dictionary<uint, List<Particle>> _circleParticlesList = new Dictionary<uint, List<Particle>>();
        private static readonly Dictionary<uint, List<Particle>> _arrowParticlesList = new Dictionary<uint, List<Particle>>();
        private int _debugMode = 0;

        private static readonly object _particlesLock = new object();

        public override string Command => "debugmode";
        public override string Syntax => $"{Command} self/all/champions/minions/projectiles/sectors";

        public DebugParticlesCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
            _game = game;
            lastDrawTime = _game.GameTime;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            _userId = userId;
            _userChampion = _playerManager.GetPeerInfo(userId).Champion;
            if (_debugMode != 0)
            {
                if (_debugMode == 1)
                {
                    _logger.Debug($"Stopped debugging self.");
                }

                if (_debugMode == 2)
                {
                    _logger.Debug($"Stopped debugging champions.");
                }

                if (_debugMode == 3)
                {
                    _logger.Debug($"Stopped debugging minions.");
                }

                if (_debugMode == 4)
                {
                    _logger.Debug($"Stopped debugging projectiles.");
                }

                if (_debugMode == 5)
                {
                    _logger.Debug($"Stopped debugging sectors.");
                }

                if (_debugMode == 6)
                {
                    _logger.Debug($"Stopped debugging all.");
                }

                _debugMode = 0;

                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Stopped debugging.");
                _game.PacketNotifier.NotifyRemoveUnitHighlight(userId, _userChampion);

                if (_circleParticles.Count != 0)
                {
                    lock (_particlesLock)
                    {
                        foreach (var particle in _circleParticles)
                        {
                            particle.Value.SetToRemove();
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
                                arrowparticle.SetToRemove();
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
                else if (split[1].Contains("minions"))
                {
                    _debugMode = 3;
                    DebugMinions(userId);
                }
                else if (split[1].Contains("projectiles"))
                {
                    _debugMode = 4;
                    DebugProjectiles(userId);
                }
                else if (split[1].Contains("sectors"))
                {
                    _debugMode = 5;
                    DebugSectors(userId);
                }
                else if (split[1].Contains("all"))
                {
                    _debugMode = 6;
                    DebugAll(userId);
                }
                else
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                }
            }
        }

        public override void Update(float diff)
        {
            if (_game.GameTime - lastDrawTime > 30.0f)
            {
                if (_debugMode == 1)
                {
                    DrawSelf(_userId);
                }
                if (_debugMode == 2)
                {
                    DrawChampions(_userId);
                }
                if (_debugMode == 3)
                {
                    DrawMinions(_userId);
                }
                if (_debugMode == 4)
                {
                    DrawProjectiles(_userId);
                }
                if (_debugMode == 5)
                {
                    DrawSectors(_userId);
                }
                if (_debugMode == 6)
                {
                    DrawAll(_userId);
                }

                lastDrawTime = _game.GameTime;
            }
        }

        // Draws your unit's collision radius and waypoints
        public void DebugSelf(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.PathfindingRadius;

            _logger.Debug($"Started debugging self. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging self. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
        }

        public void DrawSelf(int userId)
        {
            if (_debugMode != 1)
            {
                return;
            }

            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.PathfindingRadius;

            // Clear circle particles every draw in case the unit changes its position
            if (_circleParticles.ContainsKey(_userChampion.NetId))
            {
                if (_circleParticles[_userChampion.NetId] != null)
                {
                    _circleParticles.Remove(_userChampion.NetId);
                }
            }

            var circleparticle = new Particle(_game, null, null, _userChampion.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
            _circleParticles.Add(_userChampion.NetId, circleparticle);
            //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

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

                for (int waypoint = _userChampion.CurrentWaypoint.Key; waypoint < _userChampion.Waypoints.Count; waypoint++)
                {
                    var current = _userChampion.Waypoints[waypoint - 1];

                    var wpTarget = _userChampion.Waypoints[waypoint];

                    // Makes the arrow point to the next waypoint
                    var to = Vector2.Normalize(new Vector2(wpTarget.X, wpTarget.Y) - current);
                    if (_userChampion.Waypoints.Count - 1 > waypoint)
                    {
                        var nextTargetWp = _userChampion.Waypoints[waypoint + 1];
                        to = Vector2.Normalize(new Vector2(nextTargetWp.X, nextTargetWp.Y) - _userChampion.Waypoints[waypoint]);
                    }
                    var direction = new Vector3(to.X, 0, to.Y);

                    if (!_arrowParticlesList.ContainsKey(_userChampion.NetId))
                    {
                        _arrowParticlesList.Add(_userChampion.NetId, new List<Particle>());
                    }

                    var arrowparticle = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, direction, false, 0.1f);
                    _arrowParticlesList[_userChampion.NetId].Add(arrowparticle);

                    //_game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);

                    if (waypoint >= _userChampion.Waypoints.Count)
                    {
                        _logger.Debug("Waypoints Drawn: " + waypoint);
                    }
                }
            }
        }

        // Draws the collision radius and waypoints of all champions
        public void DebugChampions(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.PathfindingRadius;

            var champions = Game.ObjectManager.GetAllChampions();

            _logger.Debug($"Started debugging " + champions.Count + " champions. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging " + champions.Count + " champions. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
        }

        public void DrawChampions(int userId)
        {
            if (_debugMode != 2)
            {
                return;
            }

            var champions = Game.ObjectManager.GetAllChampions();

            // Same method as DebugSelf just for every champion
            foreach (var champion in champions)
            {
                // Arbitrary ratio is required for the DebugCircle particle to look accurate
                var circlesize = (1f / 100f) * champion.PathfindingRadius;

                if (champion.PathfindingRadius < 5)
                {
                    circlesize = (1f / 100f) * 35;
                }

                // Clear circle particles every draw in case the unit changes its position
                if (_circleParticles.ContainsKey(champion.NetId))
                {
                    if (_circleParticles[champion.NetId] != null)
                    {
                        _circleParticles.Remove(champion.NetId);
                    }
                }

                var circleparticle = new Particle(_game, null, null, champion.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
                _circleParticles.Add(champion.NetId, circleparticle);
                //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

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

                    for (int waypoint = champion.CurrentWaypoint.Key; waypoint < champion.Waypoints.Count; waypoint++)
                    {
                        var current = champion.Waypoints[waypoint - 1];

                        var wpTarget = champion.Waypoints[waypoint];

                        // Makes the arrow point to the next waypoint
                        var to = Vector2.Normalize(new Vector2(wpTarget.X, wpTarget.Y) - current);
                        if (champion.Waypoints.Count - 1 > waypoint)
                        {
                            var nextTargetWp = champion.Waypoints[waypoint + 1];
                            to = Vector2.Normalize(new Vector2(nextTargetWp.X, nextTargetWp.Y) - champion.Waypoints[waypoint]);
                        }
                        var direction = new Vector3(to.X, 0, to.Y);

                        if (!_arrowParticlesList.ContainsKey(champion.NetId))
                        {
                            _arrowParticlesList.Add(champion.NetId, new List<Particle>());
                        }

                        var arrowparticle = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, direction, false, 0.1f);
                        _arrowParticlesList[champion.NetId].Add(arrowparticle);

                        //_game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);

                        if (waypoint >= champion.Waypoints.Count)
                        {
                            _logger.Debug("Waypoints Drawn: " + waypoint);
                        }
                    }
                }
            }
        }

        // Draws the collision radius and waypoints of all Minions
        public void DebugMinions(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.PathfindingRadius;

            _logger.Debug($"Started debugging minions. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging minions. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
        }

        public void DrawMinions(int userId)
        {
            if (_debugMode != 3)
            {
                return;
            }

            // Same method as DebugSelf just for every minion
            foreach (IGameObject obj in _game.ObjectManager.GetObjects().Values)
            {
                if (obj is IMinion minion)
                {
                    // Arbitrary ratio is required for the DebugCircle particle to look accurate
                    var circlesize = (1f / 100f) * minion.PathfindingRadius;

                    if (minion.PathfindingRadius < 5)
                    {
                        circlesize = (1f / 100f) * 35;
                    }

                    // Clear circle particles every draw in case the unit changes its position
                    if (_circleParticles.ContainsKey(minion.NetId))
                    {
                        if (_circleParticles[minion.NetId] != null)
                        {
                            _circleParticles.Remove(minion.NetId);
                        }
                    }

                var circleparticle = new Particle(_game, null, null, minion.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
                _circleParticles.Add(minion.NetId, circleparticle);
                //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

                    if (minion.Waypoints.Count > 0)
                    {
                        // Clear arrow particles every draw in case the unit changes its waypoints
                        if (_arrowParticlesList.ContainsKey(minion.NetId))
                        {
                            if (_arrowParticlesList[minion.NetId].Count != 0)
                            {
                                lock (_particlesLock)
                                {
                                    _arrowParticlesList[minion.NetId].Clear();
                                }
                                _arrowParticlesList.Remove(minion.NetId);
                            }
                        }

                        for (int waypoint = minion.CurrentWaypoint.Key; waypoint < minion.Waypoints.Count; waypoint++)
                        {
                            var current = minion.Waypoints[waypoint - 1];

                            var wpTarget = minion.Waypoints[waypoint];

                            // Makes the arrow point to the next waypoint
                            var to = Vector2.Normalize(new Vector2(wpTarget.X, wpTarget.Y) - current);
                            if (minion.Waypoints.Count - 1 > waypoint)
                            {
                                var nextTargetWp = minion.Waypoints[waypoint + 1];
                                to = Vector2.Normalize(new Vector2(nextTargetWp.X, nextTargetWp.Y) - minion.Waypoints[waypoint]);
                            }
                            var direction = new Vector3(to.X, 0, to.Y);

                            if (!_arrowParticlesList.ContainsKey(minion.NetId))
                            {
                                _arrowParticlesList.Add(minion.NetId, new List<Particle>());
                            }

                        var arrowparticle = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, direction, false, 0.1f);
                        _arrowParticlesList[minion.NetId].Add(arrowparticle);

                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);

                            if (waypoint >= minion.Waypoints.Count)
                            {
                                _logger.Debug("Waypoints Drawn: " + waypoint);
                            }
                        }
                    }
                }
            }
        }

        // Draws the collision radius and waypoints of all projectiles
        public void DebugProjectiles(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.CollisionRadius;

            _logger.Debug($"Started debugging projectiles. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging projectiles. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
        }

        public void DrawProjectiles(int userId)
        {
            if (_debugMode != 4)
            {
                return;
            }

            var tempObjects = Game.ObjectManager.GetObjects();

            foreach (KeyValuePair<uint, IGameObject> obj in tempObjects)
            {
                if (obj.Value is ISpellMissile missile)
                {
                    // Arbitrary ratio is required for the DebugCircle particle to look accurate
                    var circlesize = (1f / 100f) * missile.CollisionRadius;

                    if (missile.CollisionRadius < 5)
                    {
                        circlesize = (1f / 100f) * 35;
                    }

                    // Clear circle particles every draw in case the unit changes its position
                    if (_circleParticles.ContainsKey(missile.NetId))
                    {
                        if (_circleParticles[missile.NetId] != null)
                        {
                            _circleParticles.Remove(missile.NetId);
                        }
                    }

                    var circleparticle = new Particle(_game, null, null, missile.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
                    _circleParticles.Add(missile.NetId, circleparticle);
                    //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

                    if (missile.CastInfo.Targets[0] != null || (missile.CastInfo.TargetPosition != Vector3.Zero && missile.CastInfo.TargetPositionEnd != Vector3.Zero))
                    {
                        // Clear arrow particles every draw in case the unit changes its waypoints
                        if (_arrowParticlesList.ContainsKey(missile.NetId))
                        {
                            if (_arrowParticlesList[missile.NetId].Count != 0)
                            {
                                lock (_particlesLock)
                                {
                                    _arrowParticlesList[missile.NetId].Clear();
                                }
                                _arrowParticlesList.Remove(missile.NetId);
                            }
                        }

                        if (missile.CastInfo.Targets[0].Unit != null)
                        {
                            if (!_arrowParticlesList.ContainsKey(missile.NetId))
                            {
                                _arrowParticlesList.Add(missile.NetId, new List<Particle>());
                            }

                            var current = missile.Position;

                            var wpTarget = missile.GetTargetPosition();

                            // Makes the arrow point to the target
                            var to = Vector2.Normalize(new Vector2(wpTarget.X, wpTarget.Y) - current);

                            var direction = new Vector3(to.X, 0, to.Y);
                            var arrowparticle = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowparticle);

                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);
                        }
                        else if (missile is ISpellCircleMissile skillshot)
                        {
                            if (!_arrowParticlesList.ContainsKey(missile.NetId))
                            {
                                _arrowParticlesList.Add(missile.NetId, new List<Particle>());
                            }

                            var current = new Vector2(missile.CastInfo.SpellCastLaunchPosition.X, missile.CastInfo.SpellCastLaunchPosition.Z);

                            var wpTarget = skillshot.Destination;

                            // Points the arrow towards the target
                            var dirTangent = Extensions.Rotate(new Vector2(missile.Direction.X, missile.Direction.Z), 90.0f) * missile.CollisionRadius;
                            var dirTangent2 = Extensions.Rotate(new Vector2(missile.Direction.X, missile.Direction.Z), 270.0f) * missile.CollisionRadius;

                            var arrowParticleStart = new Particle(_game, null, null, current, "DebugArrow_green.troy", 0.5f, "", "", 0, missile.Direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowParticleStart);
                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleStart, userId);

                            var arrowParticleEnd = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, missile.Direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowParticleEnd);
                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleEnd, userId);

                            var arrowParticleEnd2Temp = new Particle(_game, null, null, new Vector2(wpTarget.X + dirTangent.X, wpTarget.Y + dirTangent.Y), "Global_Indicator_Line_Beam.troy", 0.0f, "", "", 0, missile.Direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowParticleEnd2Temp);
                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleEnd2Temp, userId);

                            var arrowParticleStart2 = new Particle(_game, null, arrowParticleEnd2Temp, new Vector2(current.X + dirTangent.X, current.Y + dirTangent.Y), "Global_Indicator_Line_Beam.troy", 1.0f, "", "", 0, missile.Direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowParticleStart2);
                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleStart2, userId);

                            var arrowParticleEnd3Temp = new Particle(_game, null, null, new Vector2(wpTarget.X + dirTangent2.X, wpTarget.Y + dirTangent2.Y), "Global_Indicator_Line_Beam.troy", 0.0f, "", "", 0, missile.Direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowParticleEnd3Temp);
                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleEnd3Temp, userId);

                            var arrowParticleStart3 = new Particle(_game, null, arrowParticleEnd3Temp, new Vector2(current.X + dirTangent2.X, current.Y + dirTangent2.Y), "Global_Indicator_Line_Beam.troy", 1.0f, "", "", 0, missile.Direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowParticleStart3);
                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleStart3, userId);
                        }
                    }
                }
            }
        }

        // Draws the effected area of all sectors
        public void DebugSectors(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.CollisionRadius;

            _logger.Debug($"Started debugging sectors. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging sectors. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.CollisionRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.CollisionRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
        }

        public void DrawSectors(int userId)
        {
            if (_debugMode != 5)
            {
                return;
            }

            var tempObjects = Game.ObjectManager.GetObjects();

            foreach (KeyValuePair<uint, IGameObject> obj in tempObjects)
            {
                if (obj.Value is ISpellSector sector)
                {
                    // Arbitrary ratio is required for the DebugCircle particle to look accurate
                    var circlesize = (1f / 100f) * sector.CollisionRadius;

                    if (sector.Parameters.Width < 5)
                    {
                        circlesize = (1f / 100f) * 35;
                    }

                    // Clear circle particles every draw in case the unit changes its position
                    if (_circleParticles.ContainsKey(sector.NetId))
                    {
                        if (_circleParticles[sector.NetId] != null)
                        {
                            _circleParticles.Remove(sector.NetId);
                        }
                    }

                    if (sector.CastInfo.Targets[0] != null || (sector.CastInfo.TargetPosition != Vector3.Zero && sector.CastInfo.TargetPositionEnd != Vector3.Zero))
                    {
                        // Clear arrow particles every draw in case the unit changes its waypoints
                        if (_arrowParticlesList.ContainsKey(sector.NetId))
                        {
                            if (_arrowParticlesList[sector.NetId].Count != 0)
                            {
                                lock (_particlesLock)
                                {
                                    _arrowParticlesList[sector.NetId].Clear();
                                }
                                _arrowParticlesList.Remove(sector.NetId);
                            }
                        }

                        if (sector is ISpellSectorPolygon polygon)
                        {
                            if (!_arrowParticlesList.ContainsKey(polygon.NetId))
                            {
                                _arrowParticlesList.Add(polygon.NetId, new List<Particle>());
                            }

                            var current = polygon.Position;
                            var bindObj = polygon.Parameters.BindObject;
                            var wpTarget = polygon.CastInfo.TargetPositionEnd;

                            if (bindObj == null)
                            {
                                return;
                            }

                            var circleparticle = new Particle(_game, null, null, polygon.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
                            _circleParticles.Add(polygon.NetId, circleparticle);
                            //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

                            foreach (Vector2 vert in polygon.GetPolygonVertices())
                            {
                                var truePos = bindObj.Position + Extensions.Rotate(vert, -Extensions.UnitVectorToAngle(new Vector2(bindObj.Direction.X, bindObj.Direction.Z)) + 90f);
                                var arrowParticleVert = new Particle(_game, null, null, truePos, "DebugArrow_green.troy", 0.5f, "", "", 0, bindObj.Direction, false, 0.1f);
                                _arrowParticlesList[polygon.NetId].Add(arrowParticleVert);
                                //_game.PacketNotifier.NotifyFXCreateGroup(arrowParticleVert, userId);
                            }
                        }
                    }
                }
            }
        }

        // Draws the effected area of all game objects
        public void DebugAll(int userId)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = (1f / 100f) * _userChampion.PathfindingRadius;

            _logger.Debug($"Started debugging all. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize);
            var startdebugmsg = $"Started debugging all. Your Debug Circle Radius: " + "(1 / 100) * " + _userChampion.PathfindingRadius + " = " + "(" + (1f / 100f) + ") * " + _userChampion.PathfindingRadius + " = " + circlesize;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

            // Creates a blue flashing highlight around your unit
            _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
        }

        public void DrawAll(int userId)
        {
            if (_debugMode != 6)
            {
                return;
            }

            var tempObjects = Game.ObjectManager.GetObjects();

            foreach (IGameObject obj in tempObjects.Values)
            {
                // Arbitrary ratio is required for the DebugCircle particle to look accurate
                var circlesize = (1f / 100f) * obj.PathfindingRadius;

                if (obj.PathfindingRadius < 5)
                {
                    circlesize = (1f / 100f) * 35;
                }

                // Clear circle particles every draw in case the unit changes its position
                if (_circleParticles.ContainsKey(obj.NetId))
                {
                    if (_circleParticles[obj.NetId] != null)
                    {
                        _circleParticles.Remove(obj.NetId);
                    }
                }

                var circleparticle = new Particle(_game, null, null, obj.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
                _circleParticles.Add(obj.NetId, circleparticle);
                //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

                // TODO: Add check for AttackableUnit and draw waypoints.
            }
        }
    }
}