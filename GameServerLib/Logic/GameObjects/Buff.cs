using System.Collections.Generic;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

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
        protected CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();
        public string Name { get; private set; }
        public int Stacks { get; private set; }
        public byte Slot { get; private set; }
        protected Dictionary<Pair<MasterMask, FieldMask>, float> StatsModified = new Dictionary<Pair<MasterMask, FieldMask>, float>();
        protected Game _game;

        public bool NeedsToRemove()
        {
            return _remove;
        }

        public Buff(Game game, string buffName, float dur, int stacks, Unit onto, Unit from)
        {
            _game = game;
            Duration = dur;
            Stacks = stacks;
            Slot = 0x01;
            Name = buffName;
            TimeElapsed = 0;
            _remove = false;
            TargetUnit = onto;
            SourceUnit = from;
            BuffType = BuffType.Aura;
        }

        public Buff(Game game, string buffName, float dur, int stacks, Unit onto)
               : this(game, buffName, dur, stacks, onto, onto) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
        }
        public void Update(float diff)
        {
            TimeElapsed += (float)diff / 1000.0f;
            if (Duration != 0.0f)
            {
                if (TimeElapsed >= Duration)
                {
                    _remove = true;
                }
            }
        }
    }
}
