using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Linq;
using LeagueSandbox.GameServer.Logic.Scripting;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.API
{
    public static class ApiFunctionManager
    {
        private static Game _game;

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
        }

        public static void TeleportTo(Unit unit, float x, float y)
        {
            var coords = new Vector2(x, y);
            var truePos = _game.Map.AIMesh.getClosestTerrainExit(coords);
            _game.PacketNotifier.NotifyTeleport(unit, truePos.X, truePos.Y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return _game.Map.IsWalkable(x, y);
        }

        public static void AddBuff(string buffName, float duration, int stacks, Unit onto, Unit from)
        {
            var buff = new Buff(_game, buffName, duration, stacks, onto, from);
            onto.AddBuff(buff);
            _game.PacketNotifier.NotifyAddBuff(buff);
        }

        public static void AddParticle(Champion champion, string particle, float toX, float toY, float size = 1.0f, string bone = "")
        {
            var t = new Target(toX, toY);
            _game.PacketNotifier.NotifyParticleSpawn(new Particle(champion, t, particle, size, bone));
        }

        public static void AddParticleTarget(Champion champion, string particle, Target target, float size = 1.0f, string bone = "")
        {
            _game.PacketNotifier.NotifyParticleSpawn(new Particle(champion, target, particle, size, bone));
        }

        public static void PrintChat(string msg)
        {
            var dm = new DebugMessage(msg);
            _game.PacketHandlerManager.broadcastPacket(dm, Channel.CHL_S2C);
        }

        public static void FaceDirection(Unit unit, Vector2 direction, bool instant = true, float turnTime = 0.0833f)
        {
            _game.PacketNotifier.NotifyFaceDirection(unit, direction, instant, turnTime);
            // todo change units direction
        }

        public static List<Unit> GetUnitsInRange(Target target, float range, bool isAlive)
        {
            return _game.Map.GetUnitsInRange(target, range, isAlive);
        }

        public static List<Champion> GetChampionsInRange(Target target, float range, bool isAlive)
        {
            return _game.Map.GetChampionsInRange(target, range, isAlive);
        }

        public static void SetChampionModel(Champion champion, string model)
        {
            champion.Model = model;
        }

        public static void DashToUnit(Unit unit,
                                  Target target,
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
                var animList = new List<string> {"RUN", animation};
                _game.PacketNotifier.NotifySetAnimation(unit, animList);
            }

            if (target.IsSimpleTarget)
            {
                var newCoords = _game.Map.AIMesh.getClosestTerrainExit(new Vector2(target.X, target.Y));
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
            }
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
                );
            }
            unit.TargetUnit = null;
        }

        public static void DashToLocation(Unit unit,
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

        public static TeamId GetTeam(GameObject gameObject)
        {
            return gameObject.Team;
        }

        public static bool IsDead(Unit unit)
        {
            return unit.IsDead;
        }

        public static void SendPacket(string packetString)
        {
            var packet = StringToByteArray(packetString);
            _game.PacketHandlerManager.broadcastPacket(packet, Channel.CHL_S2C);
        }

        public static bool UnitIsChampion(GameObject unit)
        {
            return unit is Champion;
        }

        public static bool UnitIsMinion(GameObject unit)
        {
            return unit is Minion;
        }

        public static bool UnitIsTurret(GameObject unit)
        {
            return unit is BaseTurret;
        }

        public static bool UnitIsInhibitor(GameObject unit)
        {
            return unit is Inhibitor;
        }

        public static bool UnitIsNexus(GameObject unit)
        {
            return unit is Nexus;
        }

        public static bool UnitIsPlaceable(GameObject unit)
        {
            return unit is Placeable;
        }

        public static bool UnitIsMonster(GameObject unit)
        {
            return unit is Monster;
        }
    }
}
