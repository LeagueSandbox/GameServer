using System;
using GameServerCore.Domain;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.GameObjects.Spells
{
    public class Buff : Stackable, IBuff
    {
        // Crucial Vars.
        protected Game _game;
        private IBuffGameScript _buffGameScript;
        protected CSharpScriptEngine _scriptEngine;

        // Function Vars.
        protected bool _infiniteDuration;
        protected bool _remove;

        /// <summary>
        /// How this buff should be added and treated when adding new buffs of the same name.
        /// </summary>
        public BuffAddType BuffAddType { get; private set; }
        /// <summary>
        /// Type of buff to add. Determines how this buff interacts with mechanics of the game. Refer to BuffType.
        /// </summary>
        /// TODO: Add comments to BuffType enum.
        public BuffType BuffType { get; private set; }
        /// <summary>
        /// Total time this buff should be applied to its target.
        /// </summary>
        public float Duration { get; private set; }
        /// <summary>
        /// Whether or not this buff should be shown on clients' buff bar (HUD).
        /// </summary>
        public bool IsHidden { get; private set; }
        /// <summary>
        /// Internal name of this buff.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Spell which caused this buff to be applied.
        /// </summary>
        public ISpell OriginSpell { get; private set; }
        /// <summary>
        /// Slot of this buff instance. Maximum allowed is 255 due to packets.
        /// </summary>
        public byte Slot { get; private set; }
        /// <summary>
        /// Unit which applied this buff to its target.
        /// </summary>
        public IAttackableUnit SourceUnit { get; private set; }
        /// <summary>
        /// Unit which has this buff applied to it.
        /// </summary>
        public IAttackableUnit TargetUnit { get; private set; }
        /// <summary>
        /// Time since this buff's timer started.
        /// </summary>
        public float TimeElapsed { get; private set; }

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
                if (TargetUnit is IChampion c)
                {
                    ApiEventManager.OnChampionDamageTaken.AddListener(this, c, DeactivateBuff);
                    ApiEventManager.OnChampionMove.AddListener(this, c, DeactivateBuff);
                    ApiEventManager.OnChampionCrowdControlled.AddListener(this, c, DeactivateBuff);
                }
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
