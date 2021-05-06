using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface ISpell: IUpdate
    {
        ICastInfo CastInfo { get; }
        float CurrentCooldown { get; }
        float CurrentCastTime { get; }
        float CurrentChannelDuration { get; }
        float CurrentDelayTime { get; }
        bool HasEmptyScript { get; }
        bool Toggle { get; }
        ISpellData SpellData { get; }
        string SpellName { get; }
        SpellState State { get; }

        /// <returns>spell's unique ID</returns>
        int GetId();

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        void ApplyEffects(IAttackableUnit u, ISpellMissile p);

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
        public void Channel();

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
        /// Creates a single-target missile with the specified properties.
        /// </summary>
        ISpellMissile CreateSpellMissile();

        /// <summary>
        /// Creates a single-target bouncing missile with the specified properties.
        /// </summary>
        ISpellMissile CreateSpellChainMissile();

        /// <summary>
        /// Creates a line missile with the specified properties.
        /// </summary>
        ISpellMissile CreateSpellCircleMissile();

        /// <summary>
        /// Creates an arc missile with the specified properties.
        /// </summary>
        ISpellMissile CreateSpellLineMissile();

        /// <summary>
        /// Gets the cast range for this spell (based on level).
        /// </summary>
        /// <returns>Cast range based on level.</returns>
        float GetCurrentCastRange();

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

        /// <summary>
        /// Used to load the script for the spell.
        /// </summary>
        void LoadScript();

        void LevelUp();
        string GetStringForSlot();
        float GetCooldown();
        /// <summary>
        /// Sets the cooldown of this spell.
        /// </summary>
        /// <param name="newCd">Cooldown to set.</param>
        /// <param name="ignoreCDR">Whether or not to ignore cooldown reduction.</param>
        void SetCooldown(float newCd, bool ignoreCDR = false);
        void LowerCooldown(float lowerValue);
        void ResetSpellDelay();
        void SetLevel(byte toLevel);

    }
}
