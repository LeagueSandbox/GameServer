using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.GameObjects.Spells;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;

namespace LeagueSandbox.GameServer.API
{
    public static class ApiFunctionManager
    {
        private static Game _game;
        private static ILog _logger;

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        internal static void SetGame(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
        }

        public static void LogInfo(string format)
        {
            _logger.Info(format);
        }

        public static void LogInfo(string format, params object[] args)
        {
            _logger.Info(string.Format(format, args));
        }

        public static void LogDebug(string format)
        {
            _logger.Debug(format);
        }

        public static void LogDebug(string format, params object[] args)
        {
            _logger.Debug(string.Format(format, args));
        }

        public static GameScriptTimer CreateTimer(float duration, Action callback)
        {
            var newTimer = new GameScriptTimer(duration, callback);
            _game.AddGameScriptTimer(newTimer);

            return newTimer;
        }

        public static void SetGameObjectVisibility(GameObject gameObject, bool visibility)
        {
            var teams = GetTeams();
            foreach (var id in teams)
            {
                gameObject.SetVisibleByTeam(id, visibility);
            }
        }

        public static List<TeamId> GetTeams()
        {
            return _game.ObjectManager.Teams;
        }

        public static void TeleportTo(IObjAiBase unit, float x, float y)
        {
            var coords = new Vector2(x, y);
            var truePos = _game.Map.NavigationGrid.GetClosestTerrainExit(coords);

            CancelDash(unit);
            unit.TeleportTo(truePos.X, truePos.Y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return _game.Map.NavigationGrid.IsWalkable(x, y);
        }

        public static IBuff AddBuff(string buffName, float duration, byte stacks, ISpell originspell, IObjAiBase onto, IObjAiBase from, bool infiniteduration = false)
        {
            IBuff buff;

            try
            {
                buff = new Buff(_game, buffName, duration, stacks, originspell, onto, from, infiniteduration);
            }
            catch (ArgumentException exception)
            {
                _logger.Error(exception);
                return null;
            }

            onto.AddBuff(buff);
            return buff;
        }

        public static bool HasBuff(IObjAiBase unit, IBuff b)
        {
            return unit.HasBuff(b);
        }

        public static bool HasBuff(IObjAiBase unit, string b)
        {
            return unit.HasBuff(b);
        }

        public static void EditBuff(IBuff b, byte newStacks)
        {
            b.SetStacks(newStacks);
        }

        public static void RemoveBuff(IBuff buff)
        {
            buff.DeactivateBuff();
        }

        public static void RemoveBuff(IObjAiBase target, string buff)
        {
            target.RemoveBuffsWithName(buff);
        }

        public static IParticle AddParticle(IObjAiBase unit, string particle, float toX, float toY, float size = 1.0f, string bone = "", Vector3 direction = new Vector3(), float lifetime = 0, bool reqVision = true)
        {
            var t = new Target(toX, toY);
            var p = new Particle(_game, unit, t, particle, size, bone, 0, direction, lifetime, reqVision);
            return p;
        }

        public static IParticle AddParticleTarget(IObjAiBase unit, string particle, ITarget target, float size = 1.0f, string bone = "", Vector3 direction = new Vector3(), float lifetime = 0, bool reqVision = true)
        {
            var p = new Particle(_game, unit, target, particle, size, bone, 0, direction, lifetime, reqVision);
            return p;
        }

        public static void RemoveParticle(IParticle p)
        {
            p.SetToRemove();
        }

        public static Minion AddMinion(IChampion champion, string model, string name, float toX, float toY, int visionRadius = 0)
        {
            var m = new Minion(_game, champion, toX, toY, model, name, visionRadius);
            _game.ObjectManager.AddObject(m);
            m.SetVisibleByTeam(champion.Team, true);
            return m;
        }

        public static Minion AddMinionTarget(IChampion champion, string model, string name, ITarget target, int visionRadius = 0)
        {
            var m = new Minion(_game, champion, target.X, target.Y, model, name, visionRadius);
            _game.ObjectManager.AddObject(m);
            m.SetVisibleByTeam(champion.Team, true);
            return m;
        }

        public static void PrintChat(string msg)
        {
            _game.PacketNotifier.NotifyDebugMessage(msg); // TODO: Move PacketNotifier usage to less abstract classes
        }

        public static void FaceDirection(IAttackableUnit unit, Vector2 direction, bool instant = true, float turnTime = 0.0833f)
        {
            _game.PacketNotifier.NotifyFaceDirection(unit, direction, instant, turnTime); // TODO: Move PacketNotifier usage to less abstract classes (in this case GameObject)
            // TODO: Change direction of actual GameObject
        }

        public static List<IAttackableUnit> GetUnitsInRange(ITarget target, float range, bool isAlive)
        {
            return _game.ObjectManager.GetUnitsInRange(target, range, isAlive);
        }

        public static List<IChampion> GetChampionsInRange(ITarget target, float range, bool isAlive)
        {
            return _game.ObjectManager.GetChampionsInRange(target, range, isAlive);
        }

        public static void CancelDash(IObjAiBase unit)
        {
            // Allow the user to move the champion
            unit.SetDashingState(false);

            // Reset the default run animation
            var animList = new List<string> { "RUN", "" };
            _game.PacketNotifier.NotifySetAnimation(unit, animList); // TODO: Move PacketNotifier usage to less abstract classes (in this case ObjAiBase)
        }

        public static void DashToUnit(IObjAiBase unit,
                                  ITarget target,
                                  float dashSpeed,
                                  bool keepFacingLastDirection,
                                  string animation = null,
                                  float leapHeight = 0.0f,
                                  float followTargetMaxDistance = 0.0f,
                                  float backDistance = 0.0f,
                                  float travelTime = 0.0f
                                  )
        {
            if (animation != null)
            {
                var animList = new List<string> { "RUN", animation };
                _game.PacketNotifier.NotifySetAnimation(unit, animList); // TODO: Move PacketNotifier usage to less abstract classes (in this case ObjAiBase)
            }

            if (target.IsSimpleTarget)
            {
                var newCoords = _game.Map.NavigationGrid.GetClosestTerrainExit(new Vector2(target.X, target.Y));
                var newTarget = new Target(newCoords);
                unit.DashToTarget(newTarget, dashSpeed, followTargetMaxDistance, backDistance, travelTime);
                _game.PacketNotifier.NotifyDash(
                    unit,
                    newTarget,
                    dashSpeed,
                    keepFacingLastDirection,
                    leapHeight,
                    followTargetMaxDistance,
                    backDistance,
                    travelTime
                );
            } // TODO: Move PacketNotifier usage to less abstract classes (in this case ObjAiBase)
            else
            {
                unit.DashToTarget(target, dashSpeed, followTargetMaxDistance, backDistance, travelTime);
                _game.PacketNotifier.NotifyDash(
                    unit,
                    target,
                    dashSpeed,
                    keepFacingLastDirection,
                    leapHeight,
                    followTargetMaxDistance,
                    backDistance,
                    travelTime
                ); // TODO: Move PacketNotifier usage to less abstract classes (in this case ObjAiBase)
            }
            unit.TargetUnit = null;
        }

        public static void DashToLocation(IObjAiBase unit,
                                 float x,
                                 float y,
                                 float dashSpeed,
                                 bool keepFacingLastDirection,
                                 string animation = null,
                                 float leapHeight = 0.0f,
                                 float followTargetMaxDistance = 0.0f,
                                 float backDistance = 0.0f,
                                 float travelTime = 0.0f
                                 )
        {
            DashToUnit(
                unit,
                new Target(x, y),
                dashSpeed,
                keepFacingLastDirection,
                animation,
                leapHeight,
                followTargetMaxDistance,
                backDistance,
                travelTime
            );
        }
    }
}
