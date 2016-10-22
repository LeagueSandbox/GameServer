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
            _game.PacketNotifier.notifyTeleport(unit, truePos.X, truePos.Y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return _game.Map.IsWalkable(x, y);
        }

        public static void AddBuff(string buffName, float duration, int stacks, Unit onto, Unit from)
        {
            var buff = new Buff(_game, buffName, duration, stacks, onto, from);
            onto.AddBuff(buff);
            _game.PacketNotifier.notifyAddBuff(buff);
        }

        public static void AddParticle(Champion champion, string particle, float toX, float toY, float size = 1.0f, string bone = "")
        {
            var t = new Target(toX, toY);
            _game.PacketNotifier.notifyParticleSpawn(new Particle(champion, t, particle, size, bone));
        }

        public static void AddParticleTarget(Champion champion, string particle, Target target, float size = 1.0f, string bone = "")
        {
            _game.PacketNotifier.notifyParticleSpawn(new Particle(champion, target, particle, size, bone));
        }

        public static void PrintChat(string msg)
        {
            var dm = new DebugMessage(msg);
            _game.PacketHandlerManager.broadcastPacket(dm, Channel.CHL_S2C);
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
                _game.PacketNotifier.notifySetAnimation(unit, animList);
            }

            if (target.IsSimpleTarget)
            {
                var newCoords = _game.Map.AIMesh.getClosestTerrainExit(new Vector2(target.X, target.Y));
                var newTarget = new Target(newCoords);
                unit.DashToTarget(newTarget, dashSpeed, followTargetMaxDistance, backDistance, travelTime);
                _game.PacketNotifier.notifyDash(
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
                _game.PacketNotifier.notifyDash(
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

        public static void AddBaseFunctionToLuaScript(IScriptEngine scriptEngine)
        {
            if (scriptEngine == null)
                return;
            scriptEngine.RegisterFunction("setChampionModel", null, typeof(ApiFunctionManager).GetMethod("SetChampionModel", new Type[] { typeof(Champion), typeof(string) }));
            scriptEngine.RegisterFunction("teleportTo", null, typeof(ApiFunctionManager).GetMethod("TeleportTo", new Type[] { typeof(Unit), typeof(float), typeof(float) }));
            scriptEngine.RegisterFunction("addParticle", null, typeof(ApiFunctionManager).GetMethod("AddParticle", new Type[] { typeof(Champion), typeof(string), typeof(float), typeof(float), typeof(float), typeof(string) }));
            scriptEngine.RegisterFunction("addParticleTarget", null, typeof(ApiFunctionManager).GetMethod("AddParticleTarget", new Type[] { typeof(Champion), typeof(string), typeof(Target), typeof(float), typeof(string) }));
            scriptEngine.RegisterFunction("addBuff", null, typeof(ApiFunctionManager).GetMethod("AddBuff", new Type[] { typeof(string), typeof(float), typeof(int), typeof(Unit), typeof(Unit) }));
            scriptEngine.RegisterFunction("printChat", null, typeof(ApiFunctionManager).GetMethod("PrintChat", new Type[] { typeof(string) }));
            scriptEngine.RegisterFunction("getUnitsInRange", null, typeof(ApiFunctionManager).GetMethod("GetUnitsInRange", new Type[] { typeof(Target), typeof(float), typeof(bool) }));
            scriptEngine.RegisterFunction("getChampionsInRange", null, typeof(ApiFunctionManager).GetMethod("GetChampionsInRange", new Type[] { typeof(Target), typeof(float), typeof(bool) }));
            scriptEngine.RegisterFunction("dashToLocation", null, typeof(ApiFunctionManager).GetMethod("DashToLocation", new Type[] { typeof(Unit), typeof(float), typeof(float), typeof(float), typeof(bool), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float) }));
            scriptEngine.RegisterFunction("dashToUnit", null, typeof(ApiFunctionManager).GetMethod("DashToUnit", new Type[] { typeof(Unit), typeof(Target), typeof(float), typeof(bool), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float) }));
            scriptEngine.RegisterFunction("getTeam", null, typeof(ApiFunctionManager).GetMethod("GetTeam", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("isDead", null, typeof(ApiFunctionManager).GetMethod("IsDead", new Type[] { typeof(Unit) }));
            scriptEngine.RegisterFunction("sendPacket", null, typeof(ApiFunctionManager).GetMethod("SendPacket", new Type[] { typeof(string) }));
            scriptEngine.RegisterFunction("unitIsChampion", null, typeof(ApiFunctionManager).GetMethod("UnitIsChampion", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("unitIsMinion", null, typeof(ApiFunctionManager).GetMethod("UnitIsMinion", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("unitIsTurret", null, typeof(ApiFunctionManager).GetMethod("UnitIsTurret", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("unitIsInhibitor", null, typeof(ApiFunctionManager).GetMethod("UnitIsInhibitor", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("unitIsNexus", null, typeof(ApiFunctionManager).GetMethod("UnitIsNexus", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("unitIsPlaceable", null, typeof(ApiFunctionManager).GetMethod("UnitIsPlaceable", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("unitIsMonster", null, typeof(ApiFunctionManager).GetMethod("UnitIsMonster", new Type[] { typeof(GameObject) }));
        }
    }
}
