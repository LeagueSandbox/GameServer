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
        /// Forces this spell to stop channeling based on the given condition for the given reason.
        /// </summary>
        /// <param name="condition">Canceled or successful?</param>
        /// <param name="reason">How it should be treated.</param>
        void StopChanneling(ChannelingStopCondition condition, ChannelingStopSource reason);
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
        int GetId();
        float GetCooldown();
        /// <summary>
        /// Gets the cast range for this spell (based on level).
        /// </summary>
        /// <returns>Cast range based on level.</returns>
        float GetCurrentCastRange();
        string GetStringForSlot();
        void LevelUp();
        void LowerCooldown(float lowerValue);
        void ResetSpellDelay();
        void SetCooldown(float newCd);
        void SetLevel(byte toLevel);
        void SetSpellState(SpellState state);
        void SetSpellToggle(bool toggle);
    }
}
