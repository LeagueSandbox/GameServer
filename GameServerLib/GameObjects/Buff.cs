using System;
using GameServerCore;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static GameServerCore.Content.HashFunctions;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Buff : Stackable, IEventSource
    {
        // Crucial Vars.
        private readonly Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();

        // Function Vars.
        private readonly bool _infiniteDuration;
        private bool _remove;

        public BuffAddType BuffAddType { get; }
        public BuffType BuffType { get; } /// TODO: Add comments to BuffType enum.
        public float Duration { get; }
        public bool IsHidden { get; }
        public string Name { get; }
        public Spell OriginSpell { get; }
        public byte Slot { get; private set; }
        public ObjAIBase SourceUnit { get; }
        public AttackableUnit TargetUnit { get; }
        public float TimeElapsed { get; private set; }
        /// <summary>
        /// Script instance for this buff. Casting to a specific buff class gives access its functions and variables.
        /// </summary>
        public IBuffGameScript BuffScript { get; private set; }
        public uint ScriptNameHash { get; private set; }
        public IEventSource ParentScript { get; private set; }

        public StatusFlags StatusEffectsToEnable { get; private set; }
        public StatusFlags StatusEffectsToDisable { get; private set; }
        /// <summary>
        /// Used to update player buff tool tip values.
        /// </summary>
        public ToolTipData ToolTipData { get; protected set; }

        public Buff(Game game, string buffName, float duration, int stacks, Spell originSpell, AttackableUnit onto, ObjAIBase from, bool infiniteDuration = false, IEventSource parent = null)
        {
            if (duration < 0)
            {
                throw new ArgumentException("Error: Duration was set to under 0.");
            }

            _infiniteDuration = infiniteDuration;
            _game = game;
            _remove = false;
            Name = buffName;

            ParentScript = parent;
            LoadScript();
            ScriptNameHash = HashString(Name);

            BuffAddType = BuffScript.BuffMetaData.BuffAddType;
            if (BuffAddType == (BuffAddType.STACKS_AND_RENEWS | BuffAddType.STACKS_AND_CONTINUE | BuffAddType.STACKS_AND_OVERLAPS) && BuffScript.BuffMetaData.MaxStacks < 2)
            {
                throw new ArgumentException("Error: Tried to create Stackable Buff, but MaxStacks was less than 2.");
            }

            BuffType = BuffScript.BuffMetaData.BuffType;
            Duration = duration;
            IsHidden = BuffScript.BuffMetaData.IsHidden;
            if (BuffScript.BuffMetaData.MaxStacks > 254 && BuffType != BuffType.COUNTER)
            {
                MaxStacks = 254;
            }
            else
            {
                MaxStacks = Math.Min(BuffScript.BuffMetaData.MaxStacks, int.MaxValue);
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

            ToolTipData = new ToolTipData(TargetUnit, null, this);
        }

        public void LoadScript()
        {
            ApiEventManager.RemoveAllListenersForOwner(BuffScript);
            BuffScript = CSharpScriptEngine.CreateObjectStatic<IBuffGameScript>("Buffs", Name) ?? new BuffScriptEmpty();
        }

        public void ActivateBuff()
        {
            _remove = false;

            try
            {
                BuffScript.OnActivate(TargetUnit, this, OriginSpell);
            }
            catch(Exception e)
            {
                _logger.Error(null, e);
            }
        }

        public void DeactivateBuff()
        {
            if (_remove)
            {
                return;
            }
            _remove = true; // To prevent infinite loop with OnDeactivate calling events

            try
            {
                BuffScript.OnDeactivate(TargetUnit, this, OriginSpell);
            }
            catch(Exception e)
            {
                _logger.Error(null, e);
            }

            ApiEventManager.RemoveAllListenersForOwner(BuffScript);

            if (BuffScript.StatsModifier != null)
            {
                TargetUnit.RemoveStatModifier(BuffScript.StatsModifier);
            }

            ApiEventManager.OnBuffDeactivated.Publish(this);
            ApiEventManager.OnUnitBuffDeactivated.Publish(TargetUnit, this);
        }

        public bool Elapsed()
        {
            return _remove;
        }

        public StatsModifier GetStatsModifier()
        {
            return BuffScript.StatsModifier;
        }

        public void SetStatusEffect(StatusFlags flag, bool enabled)
        {
            if(enabled)
            {
                StatusEffectsToEnable |= flag;
                StatusEffectsToDisable &= ~flag;
            }
            else
            {
                StatusEffectsToDisable |= flag;
                StatusEffectsToEnable &= ~flag;
            }
        }

        public void SetToolTipVar<T>(int tipIndex, T value) where T : struct
        {
            ToolTipData.Update(tipIndex, value);

            if (TargetUnit is Champion champ)
            {
                champ.AddToolTipChange(ToolTipData);
            }
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
            if (!_infiniteDuration)
            {
                TimeElapsed += diff / 1000.0f;
                if (Math.Abs(Duration) > Extensions.COMPARE_EPSILON)
                {
                    try
                    {
                        BuffScript.OnUpdate(diff);
                    }
                    catch(Exception e)
                    {
                        _logger.Error(null, e);
                    }
                    if (TimeElapsed >= Duration)
                    {
                        DeactivateBuff();
                    }
                }
            }
        }
    }
}
