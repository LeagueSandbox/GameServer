using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Logic
{
    public static class ApiFunctionManager
    {
        private static Map _map;

        internal static void SetMap(Map m)
        {
            _map = m;
        }

        public static void TeleportTo(Unit unit, float x, float y)
        {
            PacketNotifier.notifyTeleport(unit, x, y);
        }

        public static bool IsWalkable(float x, float y)
        {
            return _map.isWalkable(x, y);
        }
        
        public static void AddBuff(string buffName, float duration, int stacks, Unit onto, Unit from)
        {
            onto.addBuff(new Buff(buffName, duration, stacks, onto, from));
        }

        public static void AddParticle(Champion champion, string particle, float toX, float toY)
        {
            Target t = new Target(toX, toY);
            PacketNotifier.notifyParticleSpawn(champion, t, particle);
        }

        public static void AddParticleTarget(Champion champion, string particle, Target target)
        {
            PacketNotifier.notifyParticleSpawn(champion, target, particle);
        }

        public static void PrintChat(string msg)
        {
            var dm = new DebugMessage(msg);
            PacketHandlerManager.getInstace().broadcastPacket(dm, Channel.CHL_S2C);
        }

        public static List<Unit> GetUnitsInRange(Target target, float range, bool isAlive)
        {
            return _map.getUnitsInRange(target, range, isAlive);
        }

        public static List<Champion> GetChampionsInRange(Target target, float range, bool isAlive)
        {
            return _map.getChampionsInRange(target, range, isAlive);
        }

        public static void SetChampionModel(Champion champion, string model)
        {
            champion.setModel(model);
        }

        public static void DashTo(Unit unit, float x, float y, float dashSpeed)
        {
            unit.dashTo(x, y, dashSpeed);
            unit.setTargetUnit(null);
            PacketNotifier.notifyDash(unit, x, y, dashSpeed);
        }

        public static TeamId GetTeam(GameObject gameObject)
        {
            return gameObject.getTeam();
        }

        public static bool IsDead(Unit unit)
        {
            return unit.isDead();
        }
        
        public static void AddBaseFunctionToLuaScript(LuaScript luaScript)
        {
            if (luaScript == null)
                return;
            luaScript.lua.RegisterFunction("setChampionModel", null, typeof(ApiFunctionManager).GetMethod("SetChampionModel", new Type[] { typeof(Champion), typeof(string) }));
            luaScript.lua.RegisterFunction("teleportTo", null, typeof(ApiFunctionManager).GetMethod("TeleportTo", new Type[] { typeof(Unit), typeof(float), typeof(float) }));
            luaScript.lua.RegisterFunction("addParticle", null, typeof(ApiFunctionManager).GetMethod("AddParticle", new Type[] { typeof(Champion), typeof(string), typeof(float), typeof(float) }));
            luaScript.lua.RegisterFunction("addParticleTarget", null, typeof(ApiFunctionManager).GetMethod("AddParticleTarget", new Type[] { typeof(Champion), typeof(string), typeof(Target) }));
            luaScript.lua.RegisterFunction("addBuff", null, typeof(ApiFunctionManager).GetMethod("AddBuff", new Type[] { typeof(string), typeof(float), typeof(int), typeof(Unit), typeof(Unit) }));
            luaScript.lua.RegisterFunction("printChat", null, typeof(ApiFunctionManager).GetMethod("PrintChat", new Type[] { typeof(string) }));
            luaScript.lua.RegisterFunction("getUnitsInRange", null, typeof(ApiFunctionManager).GetMethod("GetUnitsInRange", new Type[] { typeof(Target), typeof(float), typeof(bool) }));
            luaScript.lua.RegisterFunction("getChampionsInRange", null, typeof(ApiFunctionManager).GetMethod("GetChampionsInRange", new Type[] { typeof(Target), typeof(float), typeof(bool) }));
            luaScript.lua.RegisterFunction("dashTo", null, typeof(ApiFunctionManager).GetMethod("DashTo", new Type[] { typeof(Unit), typeof(float), typeof(float), typeof(float) }));
            luaScript.lua.RegisterFunction("getTeam", null, typeof(ApiFunctionManager).GetMethod("GetTeam", new Type[] { typeof(GameObject) }));
            luaScript.lua.RegisterFunction("isDead", null, typeof(ApiFunctionManager).GetMethod("IsDead", new Type[] { typeof(Unit) }));
        }
    }
}