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
        public float Duration { get; private set; }
        protected float _movementSpeedPercentModifier;
        public float TimeElapsed { get; private set; }
        protected bool _remove;
        public IObjAiBase TargetUnit { get; private set; }
        public IObjAiBase SourceUnit { get; private set; } // who added this buff to the unit it's attached to
        public BuffType BuffType { get; private set; }
        protected CSharpScriptEngine _scriptEngine;
        public string Name { get; private set; }
        public byte StackCount { get; private set; }
        public byte Slot { get; private set; }
        protected bool _infiniteDuration;

        protected Game _game;

        public bool Elapsed()
        {
            return _remove;
        }

        public Buff(Game game, string buffName, float dur, byte stacks, BuffType buffType, IObjAiBase onto, IObjAiBase from, bool infiniteDuration = false)
        {
            if (dur < 0)
            {
                throw new ArgumentException("Error: Duration was set to under 0.");
            }

            _game = game;
            _scriptEngine = game.ScriptEngine;
            Duration = dur;
            StackCount = stacks;
            Name = buffName;
            TimeElapsed = 0;
            _remove = false;
            TargetUnit = onto;
            SourceUnit = from;
            BuffType = buffType;
            Slot = onto.GetNewBuffSlot(this);
            _infiniteDuration = infiniteDuration;
        }

        public Buff(Game game, string buffName, float dur, byte stacks, BuffType buffType, IObjAiBase onto, bool infiniteDuration = false)
               : this(game, buffName, dur, stacks, buffType, onto, onto, infiniteDuration) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
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
                if (TimeElapsed >= Duration)
                {
                    _remove = true;
                }
            }
        }

        public void ResetDuration()
        {
            Duration = 0;
        }

        public bool IncrementStackCount()
        {
            if (StackCount == byte.MaxValue)
                return false;
            StackCount++;
            return true;
        }

        public bool DecrementStackCount()
        {
            if (StackCount <= 0)
                return false;
            StackCount--;
            return true;
        }

        public void SetStacks(byte newStacks)
        {
            StackCount = newStacks;
        }
    }
}
