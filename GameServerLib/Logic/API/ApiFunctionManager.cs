﻿using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.API
{
    public static class ApiFunctionManager
    {
        private static Game _game;
        private static Logger _logger;

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
            _logger = Program.ResolveDependency<Logger>();
        }

        public static void LogInfo(string format)
        {
            _logger.LogCoreInfo(format);
        }

        public static void LogInfo(string format, params object[] args)
        {
            _logger.LogCoreInfo(format, args);
        }

        public static GameScriptTimer CreateTimer(float duration, Action callback)
        {
            var newTimer = new GameScriptTimer(duration, callback);
            _game.AddGameScriptTimer(newTimer);
            return newTimer;
        }

        public static Buff AddBuffHUDVisual(String buffName, float duration, int stacks, BuffType type, ObjAIBase onto, float removeAfter = -1.0f)
        {
            return AddBuffHUDVisual(buffName, duration, stacks, type, onto, onto, removeAfter);
        }

        public static Buff AddBuffHUDVisual(String buffName, float duration, int stacks, BuffType type, ObjAIBase onto, ObjAIBase from, float removeAfter = -1.0f)
        {
            var b = new Buff(_game, buffName, duration, stacks, type, onto, from);
            _game.PacketNotifier.NotifyAddBuff(b);
            if (removeAfter >= 0)
            {
                CreateTimer(removeAfter, () => {
                    RemoveBuffHUDVisual(b);
                });
            }
            return b;
        }

        public static void RemoveBuffHUDVisual(Buff b)
        {
            _game.PacketNotifier.NotifyRemoveBuff(b.TargetUnit, b.Name, b.Slot);
            b.TargetUnit.RemoveBuffSlot(b);
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

        public static void TeleportTo(ObjAIBase unit, float x, float y)
        {
            var coords = new Vector2(x, y);
            var truePos = _game.Map.NavGrid.GetClosestTerrainExit(coords);

            CancelDash(unit);
            _game.PacketNotifier.NotifyTeleport(unit, truePos.X, truePos.Y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return _game.Map.NavGrid.IsWalkable(x, y);
        }

        public static void AddBuff(string buffName, float duration, int stacks, BuffType type, ObjAIBase onto, ObjAIBase from)
        {
            var buff = new Buff(_game, buffName, duration, stacks, type, onto, from);
            onto.AddBuff(buff);
            _game.PacketNotifier.NotifyAddBuff(buff);
        }

        public static void EditBuff(Buff b, int newStacks)
        {
            b.SetStacks(newStacks);
            _game.PacketNotifier.NotifyEditBuff(b, newStacks);
        }

        public static Particle AddParticle(Champion champion, string particle, float toX, float toY, float size = 1.0f, string bone = "")
        {
            var t = new Target(toX, toY);
            var p = new Particle(champion, t, particle, size, bone);
            _game.PacketNotifier.NotifyParticleSpawn(p);
            return p;
        }

        public static Particle AddParticleTarget(Champion champion, string particle, Target target, float size = 1.0f, string bone = "")
        {
            var p = new Particle(champion, target, particle, size, bone);
            _game.PacketNotifier.NotifyParticleSpawn(p);
            return p;
        }

        public static void RemoveParticle(Particle p)
        {
            _game.PacketNotifier.NotifyParticleDestroy(p);
        }

        public static void PrintChat(string msg)
        {
            var dm = new DebugMessage(msg);
            _game.PacketHandlerManager.broadcastPacket(dm, Channel.CHL_S2C);
        }

        public static void FaceDirection(AttackableUnit unit, Vector2 direction, bool instant = true, float turnTime = 0.0833f)
        {
            _game.PacketNotifier.NotifyFaceDirection(unit, direction, instant, turnTime);
            // todo change units direction
        }

        public static List<AttackableUnit> GetUnitsInRange(Target target, float range, bool isAlive)
        {
            return _game.ObjectManager.GetUnitsInRange(target, range, isAlive);
        }

        public static List<Champion> GetChampionsInRange(Target target, float range, bool isAlive)
        {
            return _game.ObjectManager.GetChampionsInRange(target, range, isAlive);
        }

        public static void SetChampionModel(Champion champion, string model)
        {
            champion.Model = model;
        }

        public static void CancelDash(ObjAIBase unit) {
            // Allow the user to move the champion
            unit.IsDashing = false;

            // Reset the default run animation
            var animList = new List<string> {"RUN", ""};
            _game.PacketNotifier.NotifySetAnimation(unit, animList);
        }

        public static void DashToUnit(ObjAIBase unit,
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
                var newCoords = _game.Map.NavGrid.GetClosestTerrainExit(new Vector2(target.X, target.Y));
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

        public static void DashToLocation(ObjAIBase unit,
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
        

        public static bool IsDead(AttackableUnit unit)
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
