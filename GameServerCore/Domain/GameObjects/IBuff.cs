using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System.Collections.Generic;

namespace GameServerCore.Domain.GameObjects
{
    public interface IBuff : IStackable, IUpdate
    {       
        /// <summary>
        /// How this buff should be added and treated when adding new buffs of the same name.
        /// </summary>
        BuffAddType BuffAddType { get; }
        /// <summary>
        /// Type of buff to add. Determines how this buff interacts with mechanics of the game. Refer to BuffType.
        /// </summary>
        BuffType BuffType { get; }
        /// <summary>
        /// Total time this buff should be applied to its target.
        /// </summary>
        float Duration { get; }
        /// <summary>
        /// Whether or not this buff should be shown on clients' buff bar (HUD).
        /// </summary>
        bool IsHidden { get; }
        /// <summary>
        /// Internal name of this buff.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Spell which caused this buff to be applied.
        /// </summary>
        ISpell OriginSpell { get; }
        /// <summary>
        /// Slot of this buff instance. Maximum allowed is 255 due to packets.
        /// </summary>
        byte Slot { get; }
        /// <summary>
        /// Unit which applied this buff to its target.
        /// </summary>
        IObjAiBase SourceUnit { get; }
        /// <summary>
        /// Unit which has this buff applied to it.
        /// </summary>
        IAttackableUnit TargetUnit { get; }
        /// <summary>
        /// Time since this buff's timer started.
        /// </summary>
        float TimeElapsed { get; }
        /// <summary>
        /// Script instance for this buff. Casting to a specific buff class gives access its functions and variables.
        /// </summary>
        IBuffGameScript BuffScript { get; }
        /// <summary>
        /// All status effects applied by this buff.
        /// </summary>
        Dictionary<StatusFlags, bool> StatusEffects { get; }
        /// <summary>
        /// Used to update player buff tool tip values.
        /// </summary>
        IToolTipData ToolTipData { get; }

        /// <summary>
        /// Used to load the script for the buff.
        /// </summary>
        void LoadScript();
        void ActivateBuff();
        void DeactivateBuff();
        bool Elapsed();
        IStatsModifier GetStatsModifier();
        void SetStatusEffect(StatusFlags flag, bool enabled);
        bool IsBuffInfinite();
        bool IsBuffSame(string buffName);
        void ResetTimeElapsed();
        void SetSlot(byte slot);
        void SetToolTipVar<T>(int tipIndex, T value) where T : struct;
    }
}
