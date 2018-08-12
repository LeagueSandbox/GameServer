using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Spells
{
    public enum BuffType : byte
    {
        INTERNAL,
        AURA,
        COMBAT_ENCHANCER,
        COMBAT_DEHANCER,
        SPELL_SHIELD,
        STUN,
        INVISIBILITY,
        SILENCE,
        TAUNT,
        POLYMORPH,
        SLOW,
        SNARE,
        DAMAGE,
        HEAL,
        HASTE,
        SPELL_IMMUNITY,
        PHYSICAL_IMMUNITY,
        INVULNERABILITY,
        SLEEP,
        NEAR_SIGHT,
        FRENZY,
        FEAR,
        CHARM,
        POISON,
        SUPPRESSION,
        BLIND,
        COUNTER,
        SHRED,
        FLEE,
        KNOCKUP,
        KNOCKBACK,
        DISARM
    }

    public class Buff
    {
        public float Duration { get; private set; }
        protected float _movementSpeedPercentModifier;
        public float TimeElapsed { get; set; }
        protected bool _remove;
        public ObjAiBase TargetUnit { get; private set; }
        public ObjAiBase SourceUnit { get; private set; } // who added this buff to the unit it's attached to
        public BuffType BuffType { get; private set; }
        protected CSharpScriptEngine _scriptEngine;
        public string Name { get; private set; }
        public int Stacks { get; private set; }
        public byte Slot { get; private set; }
        protected Game _game;

        public bool NeedsToRemove()
        {
            return _remove;
        }

        public Buff(Game game, string buffName, float dur, int stacks, ObjAiBase onto, ObjAiBase from)
        {
            _game = game;
            _scriptEngine = game.ScriptEngine;
            Duration = dur;
            Stacks = stacks;
            Name = buffName;
            TimeElapsed = 0;
            _remove = false;
            TargetUnit = onto;
            SourceUnit = from;
            BuffType = BuffType.AURA;
            Slot = onto.GetNewBuffSlot(this);
        }

        public Buff(Game game, string buffName, float dur, int stacks, ObjAiBase onto)
               : this(game, buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }
        public void Update(float diff)
        {
            TimeElapsed += diff / 1000.0f;
            if (Duration != 0.0f)
            {
                if (TimeElapsed >= Duration)
                {
                    _remove = true;
                }
            }
        }

        public void SetStacks(int newStacks)
        {
            Stacks = newStacks;
        }
    }
}
