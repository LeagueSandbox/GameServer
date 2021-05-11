using System;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Buff : Stackable, IBuff
    {
        // Crucial Vars.
        private readonly Game _game;
        private readonly CSharpScriptEngine _scriptEngine;
        private IBuffGameScript _buffGameScript;

        // Function Vars.
        private readonly bool _infiniteDuration;
        private bool _remove;

        public BuffAddType BuffAddType { get; }
        public BuffType BuffType { get; } /// TODO: Add comments to BuffType enum.
        public float Duration { get; }
        public bool IsHidden { get; }
        public string Name { get; }
        public ISpell OriginSpell { get; }
        public byte Slot { get; private set; }
        public IObjAiBase SourceUnit { get; }
        public IAttackableUnit TargetUnit { get; }
        public float TimeElapsed { get; private set; }

        public Buff(Game game, string buffName, float duration, int stacks, ISpell originSpell, IAttackableUnit onto, IObjAiBase from, bool infiniteDuration = false)
        {
            if (duration < 0)
            {
                throw new ArgumentException("Error: Duration was set to under 0.");
            }

            _infiniteDuration = infiniteDuration;
            _game = game;
            _remove = false;
            _scriptEngine = game.ScriptEngine;
            Name = buffName;

            LoadScript();

            BuffAddType = _buffGameScript.BuffAddType;
            if (BuffAddType == (BuffAddType.STACKS_AND_RENEWS | BuffAddType.STACKS_AND_CONTINUE | BuffAddType.STACKS_AND_OVERLAPS) && _buffGameScript.MaxStacks < 2)
            {
                throw new ArgumentException("Error: Tried to create Stackable Buff, but MaxStacks was less than 2.");
            }

            BuffType = _buffGameScript.BuffType;
            Duration = duration;
            IsHidden = _buffGameScript.IsHidden;
            if (_buffGameScript.MaxStacks > 254 && BuffType != BuffType.COUNTER)
            {
                MaxStacks = 254;
            }
            else
            {
                MaxStacks = Math.Min(_buffGameScript.MaxStacks, int.MaxValue);
            }
            OriginSpell = originSpell;
            if (onto.HasBuff(Name) && BuffAddType == BuffAddType.STACKS_AND_OVERLAPS)
            {
                // Put parent buff data into children buffs
                StackCount = onto.GetBuffWithName(Name).StackCount;
                Slot = onto.GetBuffWithName(Name).Slot;
            }
            else
            {
                StackCount = stacks;
                Slot = onto.GetNewBuffSlot(this);
            }
            SourceUnit = from;
            TimeElapsed = 0;
            TargetUnit = onto;
        }

        public void LoadScript()
        {
            ApiEventManager.RemoveAllListenersForOwner(_buffGameScript);
            _buffGameScript = _scriptEngine.CreateObject<IBuffGameScript>(Name, Name);
        }

        public void ActivateBuff()
        {
            _buffGameScript.OnActivate(TargetUnit, this, OriginSpell);

            _remove = false;
        }

        public void DeactivateBuff()
        {
            if (_remove)
            {
                return;
            }
            _remove = true; // To prevent infinite loop with OnDeactivate calling events

            _buffGameScript.OnDeactivate(TargetUnit, this, OriginSpell);

            if (_buffGameScript.StatsModifier != null)
            {
                TargetUnit.RemoveStatModifier(_buffGameScript.StatsModifier);
            }
        }

        public bool Elapsed()
        {
            return _remove;
        }

        public IStatsModifier GetStatsModifier()
        {
            return _buffGameScript.StatsModifier;
        }

        public bool IsBuffInfinite()
        {
            return _infiniteDuration;
        }

        public bool IsBuffSame(string buffName)
        {
            return buffName == Name;
        }

        public void ResetTimeElapsed()
        {
            TimeElapsed = 0;
        }

        public void SetSlot(byte slot)
        {
            Slot = slot;
        }

        public void Update(float diff)
        {
            if (_infiniteDuration)
            {
                return;
            }

            TimeElapsed += diff / 1000.0f;
            if (Math.Abs(Duration) > Extensions.COMPARE_EPSILON)
            {
                _buffGameScript?.OnUpdate(diff);
                if (TimeElapsed >= Duration)
                {
                    DeactivateBuff();
                }
            }
        }
    }
}
