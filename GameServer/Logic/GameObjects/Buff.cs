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


        public event EventHandler AddNew;
        public void OnAddNew(float dur, int stacks, Unit from)
        {
            SetSource(from);
            _duration = dur;
            _stacks = stacks;
            _timeElapsed = 0;
            if (AddNew != null)
            {
                try
                {
                    AddNew(this, new EventArgs());
                }
                catch(LuaException e)
                {
                    Logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }
        }


        public event EventHandler<long> UpdateBuff;
        public void OnUpdateBuff(long diff)
        {
            if (UpdateBuff != null)
            {
                try
                {
                    UpdateBuff(this, diff);
                }
                catch (LuaException e)
                {
                    Logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }
        }

        public event EventHandler EndBuff;
        public void OnEndBuff()
        {
            if (EndBuff != null)
            {
                try
                {
                    EndBuff(this, new EventArgs());
                }
                catch (LuaException e)
                {
                    Logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }
        }


        public void SetDuration(float dur)
        {
            _duration = dur;
        }

        public void SetTimeElapsed(float time)
        {
            _timeElapsed = time;
        }

        public void SetSource(Unit source)
        {
            _buffScript.lua["source"] = _attacker;
            _attacker = source;
        }

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
        }
        
        public Buff(Game game, string buffName, float dur, int stacks, Unit onto) : this(game, buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }

        public void LoadLua()
        {
            var scriptLoc = _game.Config.ContentManager.GetBuffScriptPath(_name);
            Logger.LogCoreInfo("Loading buff from " + scriptLoc);

            _buffScript.lua.DoString("package.path = 'LuaLib/?.lua;' .. package.path");
            _buffScript.lua["me"] = _attachedTo;
            _buffScript.lua["source"] = _attacker;
            _buffScript.lua["buff"] = this;
            _buffScript.lua["BUFF_Internal"] = BuffType.Internal;
            _buffScript.lua["BUFF_Aura"] = BuffType.Aura;
            _buffScript.lua["BUFF_CombatEnchancer"] = BuffType.CombatEnchancer;
            _buffScript.lua["BUFF_CombatDehancer"] = BuffType.CombatDehancer;
            _buffScript.lua["BUFF_SpellShield"] = BuffType.SpellShield;
            _buffScript.lua["BUFF_Stun"] = BuffType.Stun;
            _buffScript.lua["BUFF_Invisibility"] = BuffType.Invisibility;
            _buffScript.lua["BUFF_Silence"] = BuffType.Silence;
            _buffScript.lua["BUFF_Taunt"] = BuffType.Taunt;
            _buffScript.lua["BUFF_Polymorph"] = BuffType.Polymorph;
            _buffScript.lua["BUFF_Slow"] = BuffType.Slow;
            _buffScript.lua["BUFF_Snare"] = BuffType.Snare;
            _buffScript.lua["BUFF_Damage"] = BuffType.Damage;
            _buffScript.lua["BUFF_Heal"] = BuffType.Heal;
            _buffScript.lua["BUFF_Haste"] = BuffType.Haste;
            _buffScript.lua["BUFF_SpellImmunity"] = BuffType.SpellImmunity;
            _buffScript.lua["BUFF_PhysicalImmunity"] = BuffType.PhysicalImmunity;
            _buffScript.lua["BUFF_Invulnerability"] = BuffType.Invulnerability;
            _buffScript.lua["BUFF_Sleep"] = BuffType.Sleep;
            _buffScript.lua["BUFF_NearSight"] = BuffType.NearSight;
            _buffScript.lua["BUFF_Frenzy"] = BuffType.Frenzy;
            _buffScript.lua["BUFF_Fear"] = BuffType.Fear;
            _buffScript.lua["BUFF_Charm"] = BuffType.Charm;
            _buffScript.lua["BUFF_Poison"] = BuffType.Poison;
            _buffScript.lua["BUFF_Suppression"] = BuffType.Suppression;
            _buffScript.lua["BUFF_Blind"] = BuffType.Blind;
            _buffScript.lua["BUFF_Counter"] = BuffType.Counter;
            _buffScript.lua["BUFF_Shred"] = BuffType.Shred;
            _buffScript.lua["BUFF_Flee"] = BuffType.Flee;
            _buffScript.lua["BUFF_Knockup"] = BuffType.Knockup;
            _buffScript.lua["BUFF_Knockback"] = BuffType.Knockback;
            _buffScript.lua["BUFF_Disarm"] = BuffType.Disarm;

            ApiFunctionManager.AddBaseFunctionToLuaScript(_buffScript);
            try
            {
                _buffScript.loadScript(scriptLoc);
            }catch(LuaException e)
            {
                Logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }

        public string GetName()
        {
            return _name;
        }


        public void SetType(BuffType type)
        {
            _buffType = type;
        }
        
        public void Update(long diff)
        {
            _timeElapsed += (float)diff / 1000.0f;

            OnUpdateBuff(diff);

            if (_duration != 0.0f)
            {
                if (_timeElapsed >= _duration)
                {
                    Remove();
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
        
        public void Remove()
        {
            OnEndBuff();
            if (AddNew != null)
            {
                foreach (EventHandler handler in AddNew.GetInvocationList())
                {
                    AddNew -= handler;
                }
            }
            if (UpdateBuff != null)
            {
                foreach (EventHandler<long> handler in UpdateBuff.GetInvocationList())
                {
                    UpdateBuff -= handler;
                }
            }
            if (EndBuff != null)
            {
                foreach (EventHandler handler in EndBuff.GetInvocationList())
                {
                    EndBuff -= handler;
                }
            }
            //_buffScript.removeEvents();
            _remove = true;
        }

        public void SetMovementSpeedPercentModifier(float percent)
        {
            _movementSpeedPercentModifier = percent;
        }
    }
}
