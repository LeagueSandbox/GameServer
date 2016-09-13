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
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;
using Ninject;

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
        private Logger _logger = Program.ResolveDependency<Logger>();
        public float Duration { get; private set; }
        protected float _movementSpeedPercentModifier;
        public float TimeElapsed { get; set; }
        protected bool _remove;
        public Unit TargetUnit { get; private set; }
        public Unit SourceUnit { get; private set; } // who added this buff to the unit it's attached to
        public BuffType BuffType { get; private set; }
        protected IScriptEngine _scriptEngine = new LuaScriptEngine();
        public string Name { get; private set; }
        public int Stacks { get; private set; }
        protected Dictionary<Pair<MasterMask, FieldMask>, float> StatsModified = new Dictionary<Pair<MasterMask, FieldMask>, float>();
        protected Game _game;

        public bool NeedsToRemove()
        {
            return _remove;
        }

        public Buff(Game game, string buffName, float dur, int stacks, Unit onto, Unit from)
        {
            _game = game;
            this.Duration = dur;
            this.Stacks = stacks;
            this.Name = buffName;
            this.TimeElapsed = 0;
            _remove = false;
            this.TargetUnit = onto;
            this.SourceUnit = from;
            this.BuffType = BuffType.Aura;
            LoadLua();
            try
            {
                _scriptEngine.Execute("onAddBuff()");
            }
            catch (LuaException e)
            {
                _logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }

        public Buff(Game game, string buffName, float dur, int stacks, Unit onto)
               : this(game, buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }

        public void LoadLua()
        {
            var scriptLoc = _game.Config.ContentManager.GetBuffScriptPath(Name);
            _logger.LogCoreInfo("Loading buff from " + scriptLoc);

            _scriptEngine.Execute("package.path = 'LuaLib/?.lua;' .. package.path");
            _scriptEngine.Execute(@"
                function onAddBuff()
                end");
            _scriptEngine.Execute(@"
                function onUpdate(diff)
                end");
            _scriptEngine.Execute(@"
                function onBuffEnd()
                end");
            _scriptEngine.RegisterFunction("getSourceUnit", this, typeof(Buff).GetMethod("GetSourceUnit"));
            _scriptEngine.RegisterFunction("getUnit", this, typeof(Buff).GetMethod("GetUnit"));
            _scriptEngine.RegisterFunction("getStacks", this, typeof(Buff).GetMethod("GetStacks"));
            _scriptEngine.RegisterFunction("addStat", this, typeof(Buff).GetMethod("GetStacks"));
            _scriptEngine.RegisterFunction("substractStat", this, typeof(Buff).GetMethod("GetStacks"));
            _scriptEngine.RegisterFunction("setStat", this, typeof(Buff).GetMethod("GetStacks"));

            ApiFunctionManager.AddBaseFunctionToLuaScript(_scriptEngine);

            _scriptEngine.Load(scriptLoc);
        }

        public void Update(long diff)
        {
            TimeElapsed += (float)diff / 1000.0f;

            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("diff", diff);
                    _scriptEngine.Execute("onUpdate(diff)");
                }
                catch (LuaException e)
                {
                    _logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }

            if (Duration != 0.0f)
            {
                if (TimeElapsed >= Duration)
                {
                    try
                    {
                        _scriptEngine.Execute("onBuffEnd()");
                    }
                    catch (LuaException e)
                    {
                        _logger.LogCoreError("LUA ERROR : " + e.Message);
                    }
                    _remove = true;
                }
            }
        }
    }
}
