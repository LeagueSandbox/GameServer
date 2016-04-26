using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
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

        public void ModifyStats(Pair<MasterMask, FieldMask> stat, float modifier)
        {
            if (StatsModified.ContainsKey(stat))
            {
                StatsModified[stat] += modifier;
            }
            else
            {
                StatsModified.Add(stat, modifier);
            }

            var stats = _attachedTo.getStats();
            stats.setStat(stat.Item1, stat.Item2, stats.getStat(stat.Item1, stat.Item2) +StatsModified[stat]);
        }

        public void ResetStats()
        {
            var stats = _attachedTo.getStats();
            foreach (var stat in StatsModified)
            {
                stats.setStat(stat.Key.Item1, stat.Key.Item2, stats.getStat(stat.Key.Item1, stat.Key.Item2) - stat.Value);
            }
        }

        public Buff(string buffName, float dur, int stacks, Unit onto, Unit from)
        {
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
                _buffScript.lua.DoString("onAddBuff()");
            }
            catch (LuaException e)
            {
                Logger.LogCoreError("LUA ERROR : " + e.Message);
            }
            PacketNotifier.notifyAddBuff(this);
        }
        
        public Buff(string buffName, float dur, int stacks, Unit onto) : this(buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }

        public void LoadLua()
        {
            var scriptLoc = Config.contentManager.GetBuffScriptPath(_name);
            Logger.LogCoreInfo("Loading buff from " + scriptLoc);

            _buffScript.lua.DoString("package.path = 'LuaLib/?.lua;' .. package.path");
            _buffScript.lua.RegisterFunction("getSourceUnit", this, typeof(Buff).GetMethod("GetSourceUnit"));
            _buffScript.lua.RegisterFunction("getUnit", this, typeof(Buff).GetMethod("GetUnit"));
            _buffScript.lua.RegisterFunction("getStacks", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.lua.RegisterFunction("addStat", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.lua.RegisterFunction("substractStat", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.lua.RegisterFunction("setStat", this, typeof(Buff).GetMethod("GetStacks"));
            _buffScript.lua.RegisterFunction("modifyStats", this, typeof(Buff).GetMethod("ModifyStats", new Type[] { typeof(Pair<MasterMask, FieldMask>), typeof(float) }));
            
            _buffScript.lua["crit"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Crit_Chance);
            _buffScript.lua["armor"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Armor);
            _buffScript.lua["mr"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Magic_Armor);
            _buffScript.lua["hp5"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Hp5);
            _buffScript.lua["mp5"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Mp5);
            _buffScript.lua["range"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Range);
            _buffScript.lua["adFlat"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat);
            _buffScript.lua["adPct"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Pct);
            _buffScript.lua["apFlat"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat);
            _buffScript.lua["apPct"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Pct);
            _buffScript.lua["attackSpeed"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier);
            _buffScript.lua["cdr"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_cdr);
            _buffScript.lua["armorPenFlat"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Flat);
            _buffScript.lua["armorPenPct"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Pct);
            _buffScript.lua["lifeSteal"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_LifeSteal);
            _buffScript.lua["spellVamp"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_SpellVamp);
            _buffScript.lua["tenacity"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Two, FieldMask.FM2_Tenacity);
            _buffScript.lua["maxHp"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Four, FieldMask.FM4_MaxHp);
            _buffScript.lua["maxMp"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Four, FieldMask.FM4_MaxMp);
            _buffScript.lua["moveSpeed"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Four, FieldMask.FM4_Speed);
            _buffScript.lua["size"] = new Pair<MasterMask, FieldMask>(MasterMask.MM_Four, FieldMask.FM4_ModelSize);
        
            ApiFunctionManager.AddBaseFunctionToLuaScript(_buffScript);

            _buffScript.loadScript(scriptLoc);
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
            
            if (_buffScript.isLoaded())
            {
                try
                {
                    _buffScript.lua["diff"] = diff;
                    _buffScript.lua.DoString("onUpdate(diff)");
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
                        _buffScript.lua.DoString("onBuffEnd()");
                    }
                    catch (LuaException e)
                    {
                        Logger.LogCoreError("LUA ERROR : " + e.Message);
                    }
                    if (_name != "")
                    {
                        attachedTo.PacketNotifier.notifyRemoveBuff(_attachedTo, _name);
                    }
                    _remove = true;
                    ResetStats();
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
