using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.API
{
    public static class ApiFunctionManager
    {
        private static Game Game;

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
            Game = game;
        }

        public static void LogInfo(string format)
        {
            Logger.LogCoreInfo(format);
        }

        public static void LogInfo(string format, params object[] args)
        {
            Logger.LogCoreInfo(format, args);
        }

        public static GameScriptTimer CreateTimer(float duration, Action callback)
        {
            var newTimer = new GameScriptTimer(duration, callback);
            Game.AddGameScriptTimer(newTimer);

            return newTimer;
        }

        public static Buff AddBuffHudVisual(string buffName, float duration, int stacks, ObjAiBase onto, float removeAfter = -1.0f)
        {
            return AddBuffHudVisual(buffName, duration, stacks, onto, onto, removeAfter);
        }

        public static Buff AddBuffHudVisual(string buffName, float duration, int stacks, ObjAiBase onto, ObjAiBase from, float removeAfter = -1.0f)
        {
            var b = new Buff(Game, buffName, duration, stacks, onto, from);
            Game.PacketNotifier.NotifyAddBuff(b);
            if (removeAfter >= 0)
            {
                CreateTimer(removeAfter, () => RemoveBuffHudVisual(b));
            }

            return b;
        }

        public static void RemoveBuffHudVisual(Buff b)
        {
            Game.PacketNotifier.NotifyRemoveBuff(b.TargetUnit, b.Name, b.Slot);
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
            return Game.ObjectManager.Teams;
        }

        public static void TeleportTo(ObjAiBase unit, float x, float y)
        {
            var coords = new Vector2(x, y);
            var truePos = Game.Map.NavGrid.GetClosestTerrainExit(coords);

            CancelDash(unit);
            Game.PacketNotifier.NotifyTeleport(unit, truePos.X, truePos.Y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return Game.Map.NavGrid.IsWalkable(x, y);
        }

        public static void AddBuff(string buffName, float duration, int stacks, ObjAiBase onto, ObjAiBase from)
        {
            var buff = new Buff(Game, buffName, duration, stacks, onto, from);
            onto.AddBuff(buff);
            Game.PacketNotifier.NotifyAddBuff(buff);
        }

        public static void EditBuff(Buff b, int newStacks)
        {
            b.SetStacks(newStacks);
            Game.PacketNotifier.NotifyEditBuff(b, newStacks);
        }

        public static Particle AddParticle(Champion champion, string particle, float toX, float toY, float size = 1.0f, string bone = "")
        {
            var t = new Target(toX, toY);
            var p = new Particle(champion, t, particle, size, bone);
            Game.PacketNotifier.NotifyParticleSpawn(p);
            return p;
        }

        public static Particle AddParticleTarget(Champion champion, string particle, Target target, float size = 1.0f, string bone = "")
        {
            var p = new Particle(champion, target, particle, size, bone);
            Game.PacketNotifier.NotifyParticleSpawn(p);
            return p;
        }

        public static void RemoveParticle(Particle p)
        {
            Game.PacketNotifier.NotifyParticleDestroy(p);
        }

        public static void PrintChat(string msg)
        {
            var dm = new DebugMessage(msg);
            Game.PacketHandlerManager.BroadcastPacket(dm, Channel.CHL_S2_C);
        }

        public static void FaceDirection(AttackableUnit unit, Vector2 direction, bool instant = true, float turnTime = 0.0833f)
        {
            Game.PacketNotifier.NotifyFaceDirection(unit, direction, instant, turnTime);
            // todo change units direction
        }

        public static List<AttackableUnit> GetUnitsInRange(Target target, float range, bool isAlive)
        {
            return Game.ObjectManager.GetUnitsInRange(target, range, isAlive);
        }

        public static List<Champion> GetChampionsInRange(Target target, float range, bool isAlive)
        {
            return Game.ObjectManager.GetChampionsInRange(target, range, isAlive);
        }

        public static void CancelDash(ObjAiBase unit)
        {
            // Allow the user to move the champion
            unit.SetDashingState(false);

            // Reset the default run animation
            var animList = new List<string> {"RUN", ""};
            Game.PacketNotifier.NotifySetAnimation(unit, animList);
        }

        public static void DashToUnit(ObjAiBase unit,
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
                Game.PacketNotifier.NotifySetAnimation(unit, animList);
            }

            if (target.IsSimpleTarget)
            {
                var newCoords = Game.Map.NavGrid.GetClosestTerrainExit(new Vector2(target.X, target.Y));
                var newTarget = new Target(newCoords);
                unit.DashToTarget(newTarget, dashSpeed, followTargetMaxDistance, backDistance, travelTime);
                Game.PacketNotifier.NotifyDash(
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
                Game.PacketNotifier.NotifyDash(
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

        public static void DashToLocation(ObjAiBase unit,
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

        public static void SendPacket(string packetString)
        {
            var packet = StringToByteArray(packetString);
            Game.PacketHandlerManager.BroadcastPacket(packet, Channel.CHL_S2_C);
        }
    }
}
