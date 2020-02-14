using System;
using GameServerCore.Domain;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.GameObjects.Spells
{
    public class Buff : IBuff
    {
        public BuffAddType BuffAddType { get; private set; }
        public BuffType BuffType { get; private set; }
        public float Duration { get; private set; }
        private IBuffGameScript GameScript { get; }
        public bool IsHidden { get; private set; }
        public int MaxStacks { get; private set; }
        public string Name { get; private set; }
        public ISpell OriginSpell { get; private set; }
        public byte Slot { get; private set; }
        public IObjAiBase SourceUnit { get; private set; } // who added this buff to the unit it's attached to
        public byte StackCount { get; private set; }
        public IObjAiBase TargetUnit { get; private set; }
        public float TimeElapsed { get; private set; }

        protected bool _infiniteDuration;
        protected Game _game;
        protected bool _remove;
        protected CSharpScriptEngine _scriptEngine;

        public Buff(Game game, string buffName, float duration, byte stacks, ISpell originspell, IObjAiBase onto, IObjAiBase from, bool infiniteDuration = false)
        {
            if (duration < 0)
            {
                throw new ArgumentException("Error: Duration was set to under 0.");
            }

            _infiniteDuration = infiniteDuration;
            _game = game;
            _remove = false;
            _scriptEngine = game.ScriptEngine;

            GameScript = _scriptEngine.CreateObject<IBuffGameScript>(buffName, buffName);

            BuffAddType = GameScript.BuffAddType;
            BuffType = GameScript.BuffType;
            Duration = duration;
            IsHidden = GameScript.IsHidden;
            IsUnique = GameScript.IsUnique;
            MaxStacks = GameScript.MaxStacks;
            Name = buffName;
            OriginSpell = originspell;
            StackCount = stacks;
            Slot = onto.GetNewBuffSlot(this);
            SourceUnit = from;
            TimeElapsed = 0;
            TargetUnit = onto;
        }

        public void ActivateBuff()
        {
            GameScript.OnActivate(TargetUnit, this, OriginSpell);
            _remove = false;
        }

        public void DeactivateBuff()
        {
            if (_remove)
            {
                return;
            }

            GameScript.OnDeactivate(TargetUnit);
            _remove = true;
        }

        public bool DecrementStackCount()
        {
            if (StackCount <= 0)
                return false;
            StackCount--;
            return true;
        }

        public bool Elapsed()
        {
            return _remove;
        }

        public IStatsModifier GetStatsModifier()
        {
            return GameScript.StatsModifier;
        }

        public bool IncrementStackCount()
        {
            if (StackCount == byte.MaxValue)
                return false;
            StackCount++;
            return true;
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

        public void SetStacks(byte newStacks)
        {
            if (newStacks <= MaxStacks)
            {
                StackCount = newStacks;
            }
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
                if (GameScript != null)
                {
                    GameScript.OnUpdate(diff);
                }
                if (TimeElapsed >= Duration)
                {
                    DeactivateBuff();
                }
            }
        }
    }
}
