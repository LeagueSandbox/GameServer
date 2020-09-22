using System;
using GameServerCore.Domain;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.GameObjects.Spells
{
    public class Buff : Stackable, IBuff
    {
        public BuffAddType BuffAddType { get; private set; }
        public BuffType BuffType { get; private set; }
        public float Duration { get; private set; }
        private IBuffGameScript _buffGameScript { get; }
        public bool IsHidden { get; private set; }
        public string Name { get; private set; }
        public ISpell OriginSpell { get; private set; }
        public byte Slot { get; private set; }
        public IObjAiBase SourceUnit { get; private set; } // who added this buff to the unit it's attached to
        public IObjAiBase TargetUnit { get; private set; }
        public float TimeElapsed { get; private set; }

        protected bool _infiniteDuration;
        protected Game _game;
        protected bool _remove;
        protected CSharpScriptEngine _scriptEngine;

        public Buff(Game game, string buffName, float duration, int stacks, ISpell originspell, IObjAiBase onto, IObjAiBase from, bool infiniteDuration = false)
        {
            if (duration < 0)
            {
                throw new ArgumentException("Error: Duration was set to under 0.");
            }

            _infiniteDuration = infiniteDuration;
            _game = game;
            _remove = false;
            _scriptEngine = game.ScriptEngine;

            _buffGameScript = _scriptEngine.CreateObject<IBuffGameScript>(buffName, buffName);

            BuffAddType = _buffGameScript.BuffAddType;
            if (BuffAddType == (BuffAddType.STACKS_AND_OVERLAPS | BuffAddType.STACKS_AND_RENEWS) && _buffGameScript.MaxStacks < 2)
            {
                throw new ArgumentException("Error: Tried to create Stackable Buff, but MaxStacks was less than 2.");
            }

            BuffType = _buffGameScript.BuffType;
            Duration = duration;
            IsHidden = _buffGameScript.IsHidden;
            if (_buffGameScript.MaxStacks > 254 && !(BuffType == BuffType.COUNTER))
            {
                MaxStacks = 254;
            }
            else
            {
                MaxStacks = _buffGameScript.MaxStacks;
            }
            Name = buffName;
            OriginSpell = originspell;
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

        public void ActivateBuff()
        {
            _buffGameScript.OnActivate(TargetUnit, this, OriginSpell);
            if (!OriginSpell.SpellData.CantCancelWhileChanneling)
            {
                ApiEventManager.OnChampionDamageTaken.AddListener(this, (IChampion) TargetUnit, DeactivateBuff);
                ApiEventManager.OnChampionMove.AddListener(this, (IChampion) TargetUnit, DeactivateBuff);
                ApiEventManager.OnChampionCrowdControlled.AddListener(this, (IChampion) TargetUnit, DeactivateBuff);
            }

            _remove = false;
        }

        public void DeactivateBuff()
        {
            if (_remove)
            {
                return;
            }
            _remove = true; // To prevent infinite loop with OnDeactivate calling events

            _buffGameScript.OnDeactivate(TargetUnit);
        }

        public bool Elapsed()
        {
            return _remove;
        }

        public IStatsModifier GetStatsModifier()
        {
            return _buffGameScript.StatsModifier;
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
                if (_buffGameScript != null)
                {
                    _buffGameScript.OnUpdate(diff);
                }
                if (TimeElapsed >= Duration)
                {
                    DeactivateBuff();
                }
            }
        }
    }
}
