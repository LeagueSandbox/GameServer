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

        public event EventHandler AddBuff;
        public void OnAddBuff()
        {
            if (AddBuff != null)
            {
                try
                {
                    AddBuff(this, new EventArgs());
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
            OnAddBuff();
        }
        
        public Buff(Game game, string buffName, float dur, int stacks, Unit onto) : this(game, buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }

        public void LoadLua()
        {
            var scriptLoc = _game.Config.ContentManager.GetBuffScriptPath(_name);
            Logger.LogCoreInfo("Loading buff from " + scriptLoc);

            _buffScript.lua.DoString("package.path = 'LuaLib/?.lua;' .. package.path");
            _buffScript.lua["target"] = _attachedTo;
            _buffScript.lua["source"] = _attacker;
            _buffScript.lua["buff"] = this;        
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
            if (AddBuff != null)
            {
                foreach (EventHandler handler in AddBuff.GetInvocationList())
                {
                    AddBuff -= handler;
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
            _buffScript.removeEvents();
            _remove = true;
        }
    }
}
