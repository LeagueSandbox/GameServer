using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface ISpell: IUpdate
    {
        /// <summary>
        /// General information about this spell when it is cast. Refer to CastInfo class.
        /// </summary>
        ICastInfo CastInfo { get; }
        int CurrentAmmo { get; }
        float CurrentAmmoCooldown { get; }
        /// <summary>
        /// Current cooldown of this spell.
        /// </summary>
        float CurrentCooldown { get; }
        /// <summary>
        /// Time until casting will end for this spell.
        /// </summary>
        float CurrentCastTime { get; }
        /// <summary>
        /// Time until channeling will finish for this spell.
        /// </summary>
        float CurrentChannelDuration { get; }
        /// <summary>
        /// Time until the same spell can be cast again. Usually only applicable to auto attack spells.
        /// </summary>
        float CurrentDelayTime { get; }
        /// <summary>
        /// The toggle state of this spell.
        /// </summary>
        bool Toggle { get; }
        /// <summary>
        /// Spell data for this spell used for interactions between units, cooldown, channeling time, etc. Refer to SpellData class.
        /// </summary>
        ISpellData SpellData { get; }
        /// <summary>
        /// Internal name of this spell.
        /// </summary>
        string SpellName { get; }
        /// <summary>
        /// State of this spell. Refer to SpellState enum.
        /// </summary>
        SpellState State { get; }
        /// <summary>
        /// Script instance assigned to this spell.
        /// </summary>
        ISpellScript Script { get; }
        /// <summary>
        /// Whether or not the script for this spell is the default empty script.
        /// </summary>
        bool HasEmptyScript { get; }
        /// <summary>
        /// Used to update player ability tool tip values.
        /// </summary>
        IToolTipData ToolTipData { get; }

        /// <returns>spell's unique ID</returns>
        int GetId();

        /// <summary>
        /// Used to load the script for the spell.
        /// </summary>
        void LoadScript();

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        void ApplyEffects(IAttackableUnit u, ISpellMissile p = null, ISpellSector s = null);

        /// <summary>
        /// Whether or not this spell can be cancelled during cast.
        /// </summary>
        /// <returns></returns>
        bool CastCancelCheck();

        /// <summary>
        /// Called when the character casts this spell. Initializes the CastInfo for this spell and begins casting.
        /// </summary>
        bool Cast(Vector2 start, Vector2 end, IAttackableUnit unit = null);

        /// <summary>
        /// Called when a script manually casts this spell.
        /// </summary>
        bool Cast(ICastInfo castInfo, bool cast);

        /// <summary>
        /// Called after the spell has finished casting and is beginning a channel.
        /// </summary>
        void Channel();

        void ChannelCancelCheck();

        /// <summary>
        /// Forces this spell to stop channeling based on the given condition for the given reason.
        /// </summary>
        /// <param name="condition">Canceled or successful?</param>
        /// <param name="reason">How it should be treated.</param>
        void StopChanneling(ChannelingStopCondition condition, ChannelingStopSource reason);

        /// <summary>
        /// Called when the spell is finished casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        void FinishCasting();

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        void FinishChanneling();

        void Deactivate();

        /// <summary>
        /// Creates a spell missile with the given parameters.
        /// </summary>
        /// <param name="parameters">Parameters of the missile.</param>
        ISpellMissile CreateSpellMissile(IMissileParameters parameters);
        /// <summary>
        /// Creates a spell sector with the given parameters.
        /// </summary>
        /// <param name="parameters">Parameters of the sector.</param>
        ISpellSector CreateSpellSector(ISectorParameters parameters);

        float GetCooldown();
        float GetAmmoRechageTime();

        /// <summary>
        /// Gets the cast range for this spell (based on level).
        /// </summary>
        /// <returns>Cast range based on level.</returns>
        float GetCurrentCastRange();

        string GetStringForSlot();

        void LevelUp();

        void LowerCooldown(float lowerValue);

        void ResetSpellCast();

        /// <summary>
        /// Adds the specified unit to the list of targets for this spell.
        /// </summary>
        /// <param name="target">Unit to add.</param>
        void AddTarget(IAttackableUnit target);

        /// <summary>
        /// Removes the specified unit from the list of targets for this spell.
        /// </summary>
        /// <param name="target">Unit to remove.</param>
        void RemoveTarget(IAttackableUnit target);

        /// <summary>
        /// Sets the current target of this spell to the given unit.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        void SetCurrentTarget(IAttackableUnit target);

        /// <summary>
        /// Toggles the auto cast state for this spell.
        /// </summary>
        void SetAutocast();

        /// <summary>
        /// Overrides the normal cast range for this spell. Set to 0 to revert.
        /// </summary>
        /// <param name="newCastRange">Cast range to set.</param>
        void SetOverrideCastRange(float newCastRange);

        /// <summary>
        /// Sets the cooldown of this spell.
        /// </summary>
        /// <param name="newCd">Cooldown to set.</param>
        /// <param name="ignoreCDR">Whether or not to ignore cooldown reduction.</param>
        void SetCooldown(float newCd, bool ignoreCDR = false);

        void SetLevel(byte toLevel);

        /// <summary>
        /// Sets the state of the spell to the specified state. Often used when reseting time between spell casts.
        /// </summary>
        /// <param name="state"></param>
        void SetSpellState(SpellState state);

        /// <summary>
        /// Sets the toggle state of this spell to the specified state. True = usable, false = sealed, unusable.
        /// </summary>
        /// <param name="toggle">True/False.</param>
        void SetSpellToggle(bool toggle);

        void SetToolTipVar<T>(int tipIndex, T value) where T : struct;
    }
}
