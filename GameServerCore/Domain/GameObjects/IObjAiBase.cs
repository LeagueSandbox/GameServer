using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Base class for all moving, attackable, and attacking units.
    /// ObjAIBases normally follow these guidelines of functionality: Movement, Crowd Control, Inventory, Targeting, Attacking, and Spells.
    /// </summary>
    public interface IObjAiBase : IAttackableUnit
    {
        /// <summary>
        /// This AI's current auto attack spell.
        /// </summary>
        ISpell AutoAttackSpell { get; }
        /// <summary>
        /// Spell this AI is currently channeling.
        /// </summary>
        ISpell ChannelSpell { get; }
        /// <summary>
        /// Variable containing all data about the AI's current character such as base health, base mana, whether or not they are melee, base movespeed, per level stats, etc.
        /// </summary>
        /// TODO: Move to AttackableUnit as it relates to stats..
        ICharData CharData { get; }
        /// <summary>
        /// An unit's A.I Script
        /// </summary>
        IAIScript AIScript { get; }
        /// <summary>
        /// The ID of the skin this unit should use for its model.
        /// </summary>
        int SkinID { get; }
        /// <summary>
        /// Whether or not this AI has finished an auto attack.
        /// </summary>
        bool HasAutoAttacked { get; set; }
        /// <summary>
        /// Whether or not this AI has made their first auto attack against their current target. Refreshes after untargeting or targeting another unit.
        /// </summary>
        bool HasMadeInitialAttack { get; set; }
        /// <summary>
        /// Variable housing all variables and functions related to this AI's Inventory, ex: Items.
        /// </summary>
        IInventoryManager Inventory { get; }
        /// <summary>
        /// Whether or not this AI is currently auto attacking.
        /// </summary>
        bool IsAttacking { get; }
        /// <summary>
        /// Spell this unit will cast when in range of its target.
        /// Overrides auto attack spell casting.
        /// </summary>
        ISpell SpellToCast { get; }
        /// <summary>
        /// Whether or not this AI's auto attacks apply damage to their target immediately after their cast time ends.
        /// </summary>
        bool IsMelee { get; set; }
        /// <summary>
        /// Current order this AI is performing.
        /// </summary>
        /// TODO: Rework AI so this enum can be used fully.
        OrderType MoveOrder { get; }
        bool IsNextAutoCrit { get; }
        Dictionary<short, ISpell> Spells { get; }
        ICharScript CharScript { get; }
        /// <summary>
        /// Unit this AI will auto attack or use a spell on when in range.
        /// </summary>
        IAttackableUnit TargetUnit { get; set; }
        
        // TODO: Implement AI Scripting for this (for AI in general).
        bool IsBot { get; }

        void LoadCharScript(ISpell spell = null);
        /// <summary>
        /// Function called by this AI's auto attack projectile when it hits its target.
        /// </summary>
        void AutoAttackHit(IAttackableUnit target);
        /// <summary>
        /// Cancels any auto attacks this AI is performing and resets the time between the next auto attack if specified.
        /// </summary>
        /// <param name="reset">Whether or not to reset the delay between the next auto attack.</param>
        void CancelAutoAttack(bool reset, bool fullCancel = false);
        /// <summary>
        /// Forces this AI unit to perform a dash which follows the specified AttackableUnit.
        /// </summary>
        /// <param name="target">Unit to follow.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">How much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time (in seconds) the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// <param name="consideredCC">Whether or not to prevent movement, casting, or attacking during the duration of the movement.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        void DashToTarget
        (
            IAttackableUnit target,
            float dashSpeed,
            string animation,
            float leapGravity,
            bool keepFacingLastDirection,
            float followTargetMaxDistance,
            float backDistance,
            float travelTime,
            bool consideredCC = true
        );
        /// <summary>
        /// Gets a random auto attack spell from the list of auto attacks available for this AI.
        /// Will only select crit auto attacks if the next auto attack is going to be a crit, otherwise normal auto attacks will be selected.
        /// </summary>
        /// <returns>Random auto attack spell.</returns>
        ISpell GetNewAutoAttack();
        /// <summary>
        /// Whether or not this AI is able to auto attack.
        /// </summary>
        /// <returns></returns>
        bool CanAttack();
        /// <summary>
        /// Whether or not this AI is able to cast spells.
        /// </summary>
        /// <param name="spell">Spell to check.</param>
        bool CanCast(ISpell spell);
        ISpell GetSpell(byte slot);
        ISpell GetSpell(string name);
        /// <summary>
        /// Removes the spell instance from the given slot (replaces it with an empty BaseSpell).
        /// </summary>
        /// <param name="slot">Byte slot of the spell to remove.</param>
        void RemoveSpell(byte slot);
        /// <summary>
        /// Sets this AI's current auto attack to their base auto attack.
        /// </summary>
        void ResetAutoAttackSpell();
        ISpell LevelUpSpell(byte slot);
        bool LevelUp(bool force = true);
        /// <summary>
        /// Sets this unit's auto attack spell that they will use when in range of their target (unless they are going to cast a spell first).
        /// </summary>
        /// <param name="newAutoAttackSpell">ISpell instance to set.</param>
        /// <param name="isReset">Whether or not setting this spell causes auto attacks to be reset (cooldown).</param>
        /// <returns>ISpell set.</returns>
        void SetAutoAttackSpell(ISpell newAutoAttackSpell, bool isReset);
        /// <summary>
        /// Sets this unit's auto attack spell that they will use when in range of their target (unless they are going to cast a spell first).
        /// </summary>
        /// <param name="name">Internal name of the spell to set.</param>
        /// <param name="isReset">Whether or not setting this spell causes auto attacks to be reset (cooldown).</param>
        /// <returns>ISpell set.</returns>
        ISpell SetAutoAttackSpell(string name, bool isReset);
        /// <summary>
        /// Forces this AI to skip its next auto attack. Usually used when spells intend to override the next auto attack with another spell.
        /// </summary>
        void SkipNextAutoAttack();
        /// <summary>
        /// Sets the spell for the given slot to a new spell of the given name.
        /// </summary>
        /// <param name="name">Internal name of the spell to set.</param>
        /// <param name="slot">Slot of the spell to replace.</param>
        /// <param name="enabled">Whether or not the new spell should be enabled.</param>
        /// <param name="networkOld">Whether or not to notify clients of this change using an older packet method.</param>
        /// <returns>Newly created spell set.</returns>
        ISpell SetSpell(string name, byte slot, bool enabled, bool networkOld = false);
        /// <summary>
        /// Sets the spell that this unit will cast when it gets in range of the spell's target.
        /// Overrides auto attack spell casting.
        /// </summary>
        /// <param name="s">Spell that will be cast.</param>
        /// <param name="location">Location to cast the spell on. May set to Vector2.Zero if unit parameter is used.</param>
        /// <param name="unit">Unit to cast the spell on.</param>
        void SetSpellToCast(ISpell s, Vector2 location, IAttackableUnit unit = null);
        /// <summary>
        /// Sets the spell that this unit is currently casting.
        /// </summary>
        /// <param name="s">Spell that is being cast.</param>
        void SetCastSpell(ISpell s);
        /// <summary>
        /// Gets the spell this unit is currently casting.
        /// </summary>
        /// <returns>Spell that is being cast.</returns>
        ISpell GetCastSpell();
        /// <summary>
        /// Forces this unit to stop targeting the given unit.
        /// Applies to attacks, spell casts, spell channels, and any queued spell casts.
        /// </summary>
        /// <param name="target"></param>
        void Untarget(IAttackableUnit target);
        /// <summary>
        /// Sets this AI's current target unit. This relates to both auto attacks as well as general spell targeting.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        /// <param name="networked">Whether or not this change in target should be networked to clients.</param>
        void SetTargetUnit(IAttackableUnit target, bool networked = false);
        /// <summary>
        /// Swaps the spell in the given slot1 with the spell in the given slot2.
        /// </summary>
        /// <param name="slot1">Slot of the spell to put into slot2.</param>
        /// <param name="slot2">Slot of the spell to put into slot1.</param>
        void SwapSpells(byte slot1, byte slot2);
        /// <summary>
        /// Sets the spell that will be channeled by this unit. Used by Spell for manual stopping and networking.
        /// </summary>
        /// <param name="spell">Spell that is being channeled.</param>
        /// <param name="network">Whether or not to send the channeling of this spell to clients.</param>
        void SetChannelSpell(ISpell spell, bool network = true);
        /// <summary>
        /// Forces this AI to stop channeling based on the given condition with the given reason.
        /// </summary>
        /// <param name="condition">Canceled or successful?</param>
        /// <param name="reason">How it should be treated.</param>
        void StopChanneling(ChannelingStopCondition condition, ChannelingStopSource reason);
        /// <summary>
        /// Gets the most recently spawned Pet unit which is owned by this unit.
        /// </summary>
        IPet GetPet();
        /// <summary>
        /// Sets the most recently spawned Pet unit which is owned by this unit.
        /// </summary>
        void SetPet(IPet pet);
        /// <summary>
        /// Sets this unit's move order to the given order type.
        /// </summary>
        /// <param name="order">OrderType to set.</param>
        /// <param name="publish">Whether or not to trigger the move order update event.</param>
        void UpdateMoveOrder(OrderType order, bool publish = true);
        ClassifyUnit ClassifyTarget(IAttackableUnit target, IAttackableUnit victium = null);
        bool RecalculateAttackPosition();
        /// <summary>
        /// Gets the state of this unit's AI.
        /// </summary>
        AIState GetAIState();
        /// <summary>
        /// Sets the state of this unit's AI.
        /// </summary>
        /// <param name="newState">State to set.</param>
        void SetAIState(AIState newState);
        /// <summary>
        /// Whether or not this unit's AI is innactive.
        /// </summary>
        bool IsAiPaused();
        /// <summary>
        /// Forces this unit's AI to pause/unpause.
        /// </summary>
        /// <param name="isPaused">Whether or not to pause.</param>s
        void PauseAi(bool isPaused);
    }
}