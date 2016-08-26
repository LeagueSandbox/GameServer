using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
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
            var truePos = unit.GetGame().GetMap().getAIMesh().getClosestTerrainExit(coords);
            _game.PacketNotifier.notifyTeleport(unit, truePos.X, truePos.Y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return _game.GetMap().IsWalkable(x, y);
        }

        public static void AddBuff(string buffName, float duration, int stacks, Unit onto, Unit from)
        {
            var buff = new Buff(_game, buffName, duration, stacks, onto, from);
            onto.AddBuff(buff);
            _game.PacketNotifier.notifyAddBuff(buff);
        }

        public static void AddParticle(Champion champion, string particle, float toX, float toY)
        {
            Target t = new Target(toX, toY);
            _game.PacketNotifier.notifyParticleSpawn(champion, t, particle);
        }

        public static void AddParticleTarget(Champion champion, string particle, Target target)
        {
            _game.PacketNotifier.notifyParticleSpawn(champion, target, particle);
        }

        public static void PrintChat(string msg)
        {
            var dm = new DebugMessage(msg);
            _game.PacketHandlerManager.broadcastPacket(dm, Channel.CHL_S2C);
        }

        public static List<Unit> GetUnitsInRange(Target target, float range, bool isAlive)
        {
            return _game.GetMap().GetUnitsInRange(target, range, isAlive);
        }

        public static List<Champion> GetChampionsInRange(Target target, float range, bool isAlive)
        {
            return _game.GetMap().GetChampionsInRange(target, range, isAlive);
        }

        public static void SetChampionModel(Champion champion, string model)
        {
            champion.setModel(model);
        }

        public static void DashTo(Unit unit, float x, float y, float dashSpeed, float leapHeight, string animation = null)
        {
            if (animation != null)
            {
                List<string> animList = new List<string>();
                animList.Add("RUN");
                animList.Add(animation);
                _game.PacketNotifier.notifySetAnimation(unit, animList);
            }

            var newCoords = unit.GetGame().GetMap().getAIMesh().getClosestTerrainExit(new Vector2(x, y));
            unit.dashTo(newCoords.X, newCoords.Y, dashSpeed);
            unit.setTargetUnit(null);
            _game.PacketNotifier.notifyDash(unit, x, y, dashSpeed, leapHeight);
        }

        public static TeamId GetTeam(GameObject gameObject)
        {
            return gameObject.getTeam();
        }

        public static bool IsDead(Unit unit)
        {
            return unit.isDead();
        }

        public static void SendPacket(string packetString)
        {
            var packet = StringToByteArray(packetString);
            _game.PacketHandlerManager.broadcastPacket(packet, Channel.CHL_S2C);
        }

        public static void AddBaseFunctionToLuaScript(IScriptEngine scriptEngine)
        {
            if (scriptEngine == null)
                return;
            scriptEngine.RegisterFunction("setChampionModel", null, typeof(ApiFunctionManager).GetMethod("SetChampionModel", new Type[] { typeof(Champion), typeof(string) }));
            scriptEngine.RegisterFunction("teleportTo", null, typeof(ApiFunctionManager).GetMethod("TeleportTo", new Type[] { typeof(Unit), typeof(float), typeof(float) }));
            scriptEngine.RegisterFunction("addParticle", null, typeof(ApiFunctionManager).GetMethod("AddParticle", new Type[] { typeof(Champion), typeof(string), typeof(float), typeof(float) }));
            scriptEngine.RegisterFunction("addParticleTarget", null, typeof(ApiFunctionManager).GetMethod("AddParticleTarget", new Type[] { typeof(Champion), typeof(string), typeof(Target) }));
            scriptEngine.RegisterFunction("addBuff", null, typeof(ApiFunctionManager).GetMethod("AddBuff", new Type[] { typeof(string), typeof(float), typeof(int), typeof(Unit), typeof(Unit) }));
            scriptEngine.RegisterFunction("printChat", null, typeof(ApiFunctionManager).GetMethod("PrintChat", new Type[] { typeof(string) }));
            scriptEngine.RegisterFunction("getUnitsInRange", null, typeof(ApiFunctionManager).GetMethod("GetUnitsInRange", new Type[] { typeof(Target), typeof(float), typeof(bool) }));
            scriptEngine.RegisterFunction("getChampionsInRange", null, typeof(ApiFunctionManager).GetMethod("GetChampionsInRange", new Type[] { typeof(Target), typeof(float), typeof(bool) }));
            scriptEngine.RegisterFunction("dashTo", null, typeof(ApiFunctionManager).GetMethod("DashTo", new Type[] { typeof(Unit), typeof(float), typeof(float), typeof(float), typeof(float), typeof(string) }));
            scriptEngine.RegisterFunction("getTeam", null, typeof(ApiFunctionManager).GetMethod("GetTeam", new Type[] { typeof(GameObject) }));
            scriptEngine.RegisterFunction("isDead", null, typeof(ApiFunctionManager).GetMethod("IsDead", new Type[] { typeof(Unit) }));
            scriptEngine.RegisterFunction("sendPacket", null, typeof(ApiFunctionManager).GetMethod("SendPacket", new Type[] { typeof(string) }));
        }
    }
}
