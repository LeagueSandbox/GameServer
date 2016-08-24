using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Packets;
using NLua.Exceptions;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum BuffType : byte
    {
        Internal,
        Aura,
        CombatEnchancer,
        CombatDehancer,
        SpellShield,
        Stun,
        Invisibility,
        Silence,
        Taunt,
        Polymorph,
        Slow,
        Snare,
        Damage,
        Heal,
        Haste,
        SpellImmunity,
        PhysicalImmunity,
        Invulnerability,
        Sleep,
        NearSight,
        Frenzy,
        Fear,
        Charm,
        Poison,
        Suppression,
        Blind,
        Counter,
        Shred,
        Flee,
        Knockup,
        Knockback,
        Disarm
    }

    public class Buff
    {
        protected float _duration;
        protected float _movementSpeedPercentModifier;
        protected float _timeElapsed;
        protected bool _remove;
        protected Unit _attachedTo;
        protected Unit _attacker; // who added this buff to the unit it's attached to
        protected BuffType _buffType;
        protected LuaScript _buffScript = new LuaScript();
        protected string _name;
        protected int _stacks;
        protected Dictionary<Pair<MasterMask, FieldMask>, float> StatsModified = new Dictionary<Pair<MasterMask, FieldMask>, float>();
        protected Game _game;

        public BuffType GetBuffType()
        {
            return _buffType;
        }

        public Unit GetUnit()
        {
            return _attachedTo;
        }

        public Unit GetSourceUnit()
        {
            return _attacker;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public bool NeedsToRemove()
        {
            return _remove;
        }

        public Buff(Game game, string buffName, float dur, int stacks, Unit onto, Unit from)
        {
            _game = game;
            _duration = dur;
            _stacks = stacks;
            _name = buffName;
            _timeElapsed = 0;
            _remove = false;
            _attachedTo = onto;
            _attacker = from;
            _buffType = BuffType.Aura;
            LoadLua();
            try
            {
                _buffScript.Lua.DoString("onAddBuff()");
            }
            catch (LuaException e)
            {
                Logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }
        
        public Buff(Game game, string buffName, float dur, int stacks, Unit onto) : this(game, buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }

        public void LoadLua()
        {
            var scriptLoc = _game.Config.ContentManager.GetBuffScriptPath(_name);
            Logger.LogCoreInfo("Loading buff from " + scriptLoc);

            _buffScript.Lua.DoString("package.path = 'LuaLib/?.lua;' .. package.path");
            _buffScript.Lua.DoString(@"
                function onAddBuff()
                end");
            _buffScript.Lua.DoString(@"
                function onUpdate(diff)
                end");
            _buffScript.Lua.DoString(@"
                function onBuffEnd()
                end");
            _buffScript.Lua.RegisterFunction("getSourceUnit", this, typeof(Buff).GetMethod("GetSourceUnit"));
            _buffScript.Lua.RegisterFunction("getUnit", this, typeof(Buff).GetMethod("GetUnit"));
            _buffScript.Lua.RegisterFunction("getStacks", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.Lua.RegisterFunction("addStat", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.Lua.RegisterFunction("substractStat", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.Lua.RegisterFunction("setStat", this, typeof(Buff).GetMethod("GetStacks"));
        
            ApiFunctionManager.AddBaseFunctionToLuaScript(_buffScript);

            _buffScript.Load(scriptLoc);
        }

        public string GetName()
        {
            return _name;
        }

        public void SetTimeElapsed(float time)
        {
            _timeElapsed = time;
        }
        
        public void Update(long diff)
        {
            _timeElapsed += (float)diff / 1000.0f;
            
            if (_buffScript.IsLoaded())
            {
                try
                {
                    _buffScript.Lua["diff"] = diff;
                    _buffScript.Lua.DoString("onUpdate(diff)");
                }
                catch (LuaException e)
                {
                    Logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }

            if (_duration != 0.0f)
            {
                if (_timeElapsed >= _duration)
                {
                    try
                    {
                        _buffScript.Lua.DoString("onBuffEnd()");
                    }
                    catch (LuaException e)
                    {
                        Logger.LogCoreError("LUA ERROR : " + e.Message);
                    }
                    _remove = true;
                }
            }
        }

        public int GetStacks()
        {
            return _stacks;
        }

        public float GetDuration()
        {
            return _duration;
        }
    }
}
