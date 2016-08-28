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

    public class Buff : IBuff
    {
        public float Duration { get; set; }
        public float TimeElapsed { get; set; }
        public bool NeedsToRemove { get; set; }
        public Unit AttachedTo { get; set; }
        public Unit Attacker { get; set; } // who added this buff to the unit it's attached to
        public BuffType BuffType { get; set; }
        public string Name { get; set; }
        public int Stacks { get; set; }
        protected IScriptEngine _scriptEngine = new LuaScriptEngine();
        protected Dictionary<Pair<MasterMask, FieldMask>, float> StatsModified = new Dictionary<Pair<MasterMask, FieldMask>, float>();
        protected Game _game;

        public Buff(Game game, string buffName, float duration, Unit onto, Unit from)
        {
            _game = game;
            Duration = duration;
            Stacks = 1;
            Name = buffName;
            TimeElapsed = 0;
            NeedsToRemove = false;
            AttachedTo = onto;
            Attacker = from;
            BuffType = BuffType.Aura; //Will be loaded from the files
            LoadLua();
            try
            {
                _scriptEngine.Execute("onAddBuff()");
            }
            catch (LuaException e)
            {
                Logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }
        
        public Buff(Game game, string buffName, float dur, int stacks, Unit onto) : this(game, buffName, dur, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }

        public Unit GetUnit()
        {
            return AttachedTo;
        }

        public Unit GetSourceUnit()
        {
            return Attacker;
        }

        public void LoadLua()
        {
            var scriptLoc = _game.Config.ContentManager.GetBuffScriptPath(Name);
            Logger.LogCoreInfo("Loading buff from " + scriptLoc);

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
        
            ApiFunctionManager.AddBaseFunctionToLuaScript(_scriptEngine);

            _scriptEngine.Load(scriptLoc);
            _scriptEngine.SetGlobalVariable("buff", this);
        }
        
        public void Update(long diff)
        {
            TimeElapsed += diff / 1000.0f;
            
            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("diff", diff);
                    _scriptEngine.Execute("onUpdate(diff)");
                }
                catch (LuaException e)
                {
                    Logger.LogCoreError("LUA ERROR : " + e.Message);
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
                        Logger.LogCoreError("LUA ERROR : " + e.Message);
                    }
                    NeedsToRemove = true;
                }
            }
        }

        public StatModifcator HealthPoints { get; set; }
        public StatModifcator HealthRegeneration { get; set; }
        public StatModifcator AttackDamage { get; set; }
        public StatModifcator AbilityPower { get; set; }
        public StatModifcator CriticalChance { get; set; }
        public StatModifcator Armor { get; set; }
        public StatModifcator MagicResist { get; set; }
        public StatModifcator AttackSpeed { get; set; }
        public StatModifcator ArmorPenetration { get; set; }
        public StatModifcator MagicPenetration { get; set; }
        public StatModifcator ManaPoints { get; set; }
        public StatModifcator ManaRegeneration { get; set; }
        public StatModifcator LifeSteel { get; set; }
        public StatModifcator SpellVamp { get; set; }
        public StatModifcator Tenacity { get; set; }
        public StatModifcator Size { get; set; }
        public StatModifcator Range { get; set; }
        public StatModifcator MoveSpeed { get; set; }
        public StatModifcator GoldPerSecond { get; set; }
    }
}
