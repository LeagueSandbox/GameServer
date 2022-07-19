using GameServerCore;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class DebugParticlesCommand : ChatCommandBase
    {
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly PlayerManager _playerManager;
        private readonly Game _game;
        private float lastDrawTime;
        private int _userId;
        private Champion _userChampion;
        private static readonly Dictionary<uint, Particle> _circleParticles = new Dictionary<uint, Particle>();
        private static readonly Dictionary<uint, List<Particle>> _arrowParticlesList = new Dictionary<uint, List<Particle>>();
        private enum DebugMode: int
        {
            None, Self, Champions, Minions, Projectiles, Sectors, All
        }
        private DebugMode _debugMode = DebugMode.None;

        public override string Command => "debugmode";
        private string[] _modes;
        public override string Syntax => $"{Command} {String.Join('/', _modes)}";

        private const float _debugCircleScale = 0.01f;

        public DebugParticlesCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
            _game = game;
            lastDrawTime = _game.GameTime;
            
            _modes = Enum.GetNames(typeof(DebugMode));
            for(int i = 0; i < _modes.Length; i++)
            {
                _modes[i] = _modes[i].ToLowerInvariant();
            }
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            _userId = userId;
            _userChampion = _playerManager.GetPeerInfo(userId).Champion;
            
            var split = arguments.ToLower().Split(' ');
            int idx = 0;

            if(split.Length < 2 || split.Length > 3 || (idx = Array.IndexOf(_modes, split[1])) == -1)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else
            {
                if(idx == (int)_debugMode)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Already debugging {_modes[idx]}.");
                }
                else if(idx == 0)
                {
                    var stopdebugmsg = $"Stopped debugging {_modes[(int)_debugMode]}.";
                    _logger.Debug(stopdebugmsg);

                    _debugMode = DebugMode.None;

                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, stopdebugmsg);
                    _game.PacketNotifier.NotifyRemoveUnitHighlight(userId, _userChampion);

                    if (_circleParticles.Count != 0)
                    {
                        foreach (var particle in _circleParticles)
                        {
                            particle.Value.SetToRemove();
                        }
                        _circleParticles.Clear();
                    }
                    if (_arrowParticlesList.Count != 0)
                    {
                        foreach (var particleList in _arrowParticlesList)
                        {
                            foreach(var arrowparticle in particleList.Value)
                            {
                                arrowparticle.SetToRemove();
                            }
                            particleList.Value.Clear();
                        }
                        _arrowParticlesList.Clear();
                    }
                }
                else
                {
                    // Arbitrary ratio is required for the DebugCircle particle to look accurate
                    var circlesize = _debugCircleScale * _userChampion.PathfindingRadius;

                    var startdebugmsg = $"Started debugging {_modes[idx]}. Your Debug Circle Radius: {_debugCircleScale} * {_userChampion.PathfindingRadius} = {circlesize}";
                    _logger.Debug(startdebugmsg);
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, startdebugmsg);

                    if(_debugMode == DebugMode.None)
                    {
                        // Creates a blue flashing highlight around your unit
                        _game.PacketNotifier.NotifyCreateUnitHighlight(userId, _userChampion);
                    }

                    _debugMode = (DebugMode)idx;
                }
            }
        }

        public override void Update(float _diff = 0)
        {
            if (_game.GameTime - lastDrawTime > 30.0f)
            {
                if (_debugMode == DebugMode.Self)
                {
                    DrawSelf(_userId);
                }
                else if (_debugMode == DebugMode.Champions)
                {
                    DrawChampions(_userId);
                }
                else if (_debugMode == DebugMode.Minions)
                {
                    DrawMinions(_userId);
                }
                else if (_debugMode == DebugMode.Projectiles)
                {
                    DrawProjectiles(_userId);
                }
                else if (_debugMode == DebugMode.Sectors)
                {
                    DrawSectors(_userId);
                }
                else if (_debugMode == DebugMode.All)
                {
                    DrawAll(_userId);
                }

                lastDrawTime = _game.GameTime;
            }
        }

        // Draws your unit's collision radius and waypoints
        public void DrawSelf(int userId)
        {
            DrawAttackableUnit(_userChampion, userId);
        }

        void DrawAttackableUnit(AttackableUnit u, int userId = -1)
        {
            // Arbitrary ratio is required for the DebugCircle particle to look accurate
            var circlesize = _debugCircleScale * u.PathfindingRadius;
            if (u.PathfindingRadius < 5)
            {
                circlesize = _debugCircleScale * 35;
            }

            // Clear circle particles every draw in case the unit changes its position
            if (_circleParticles.ContainsKey(u.NetId))
            {
                if (_circleParticles[u.NetId] != null)
                {
                    _circleParticles.Remove(u.NetId);
                }
            }

            var circleparticle = new Particle(_game, null, null, u.Position, "DebugCircle_green.troy", circlesize, "", "", 0, default, false, 0.1f);
            _circleParticles.Add(u.NetId, circleparticle);
            //_game.PacketNotifier.NotifyFXCreateGroup(circleparticle, userId);

            if (u.Waypoints.Count > 0)
            {
                // Clear arrow particles every draw in case the unit changes its waypoints
                if (_arrowParticlesList.ContainsKey(u.NetId))
                {
                    if (_arrowParticlesList[u.NetId].Count != 0)
                    {
                        _arrowParticlesList[u.NetId].Clear();
                        _arrowParticlesList.Remove(u.NetId);
                    }
                }

                for (int waypoint = u.CurrentWaypointKey; waypoint < u.Waypoints.Count; waypoint++)
                {
                    var current = u.Waypoints[waypoint - 1];

                    var wpTarget = u.Waypoints[waypoint];

                    // Makes the arrow point to the next waypoint
                    var to = Vector2.Normalize(wpTarget - current);
                    if (u.Waypoints.Count - 1 > waypoint)
                    {
                        var nextTargetWp = u.Waypoints[waypoint + 1];
                        to = Vector2.Normalize(nextTargetWp - u.Waypoints[waypoint]);
                    }
                    var direction = new Vector3(to.X, 0, to.Y);

                    if (!_arrowParticlesList.ContainsKey(u.NetId))
                    {
                        _arrowParticlesList.Add(u.NetId, new List<Particle>());
                    }

                    var arrowparticle = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, direction, false, 0.1f);
                    _arrowParticlesList[u.NetId].Add(arrowparticle);

                    //_game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);

                    if (waypoint >= u.Waypoints.Count)
                    {
                        _logger.Debug("Waypoints Drawn: " + waypoint);
                    }
                }
            }
        }

        // Draws the collision radius and waypoints of all champions
        public void DrawChampions(int userId)
        {
            // Same method as DebugSelf just for every champion
            foreach (var champion in Game.ObjectManager.GetAllChampions())
            {
                DrawAttackableUnit(champion, userId);
            }
        }

        // Draws the collision radius and waypoints of all Minions
        public void DrawMinions(int userId)
        {
            // Same method as DebugSelf just for every minion
            foreach (GameObject obj in _game.ObjectManager.GetObjects().Values)
            {
                if (obj is Minion minion)
                {
                    DrawAttackableUnit(minion, userId);
                }
            }
        }

        // Draws the collision radius and waypoints of all projectiles
        public void DrawProjectiles(int userId)
        {
            var tempObjects = Game.ObjectManager.GetObjects();

            foreach (KeyValuePair<uint, GameObject> obj in tempObjects)
            {
                if (obj.Value is SpellMissile missile)
                {
                    // Arbitrary ratio is required for the DebugCircle particle to look accurate
                    var circlesize = _debugCircleScale * missile.CollisionRadius;
                    if (missile.CollisionRadius < 5)
                    {
                        circlesize = _debugCircleScale * 35;
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
                                _arrowParticlesList[missile.NetId].Clear();
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
                            var to = Vector2.Normalize(wpTarget - current);

                            var direction = new Vector3(to.X, 0, to.Y);
                            var arrowparticle = new Particle(_game, null, null, wpTarget, "DebugArrow_green.troy", 0.5f, "", "", 0, direction, false, 0.1f);
                            _arrowParticlesList[missile.NetId].Add(arrowparticle);

                            //_game.PacketNotifier.NotifyFXCreateGroup(arrowparticle, userId);
                        }
                        else if (missile is SpellCircleMissile skillshot)
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
        public void DrawSectors(int userId)
        {
            var tempObjects = Game.ObjectManager.GetObjects();

            foreach (KeyValuePair<uint, GameObject> obj in tempObjects)
            {
                if (obj.Value is SpellSector sector)
                {
                    // Arbitrary ratio is required for the DebugCircle particle to look accurate
                    var circlesize = _debugCircleScale * sector.CollisionRadius;
                    if (sector.Parameters.Width < 5)
                    {
                        circlesize = _debugCircleScale * 35;
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
                                _arrowParticlesList[sector.NetId].Clear();
                                _arrowParticlesList.Remove(sector.NetId);
                            }
                        }

                        if (sector is SpellSectorPolygon polygon)
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
        public void DrawAll(int userId)
        {
            var tempObjects = Game.ObjectManager.GetObjects();

            foreach (GameObject obj in tempObjects.Values)
            {
                // Arbitrary ratio is required for the DebugCircle particle to look accurate
                var circlesize = _debugCircleScale * obj.PathfindingRadius;
                if (obj.PathfindingRadius < 5)
                {
                    circlesize = _debugCircleScale * 35;
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