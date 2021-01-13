using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Base class for all moving, attackable, and attacking units.
    /// ObjAIBases normally follow these guidelines of functionality: Movement, Crowd Control, Inventory, Targeting, Attacking, and Spells.
    /// </summary>
    public interface IObjAiBase : IAttackableUnit
    {
        /// <summary>
        /// Variable storing all the data related to this AI's current auto attack. *NOTE*: Will be deprecated as the spells system gets finished.
        /// </summary>
        ISpellData AaSpellData { get; }
        /// <summary>
        /// Variable for the cast time of this AI's current auto attack.
        /// </summary>
        float AutoAttackCastTime { get; set; }
        /// <summary>
        /// Variable for the projectile speed of this AI's current auto attack projectile.
        /// </summary>
        float AutoAttackProjectileSpeed { get; set; }
        /// <summary>
        /// This AI's current auto attack target. Null if no target.
        /// </summary>
        IAttackableUnit AutoAttackTarget { get; set; }
        /// <summary>
        /// Variable containing all data about the AI's current character such as base health, base mana, whether or not they are melee, base movespeed, per level stats, etc.
        /// </summary>
        /// TODO: Move to AttackableUnit as it relates to stats..
        ICharData CharData { get; }
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
        bool IsAttacking { get; set; }
        /// <summary>
        /// Whether or not this AI is currently casting a spell. *NOTE*: Not to be confused with channeling (which isn't implemented yet).
        /// </summary>
        bool IsCastingSpell { get; set; }
        /// <summary>
        /// Whether or not this AI is currently dashing.
        /// </summary>
        bool IsDashing { get; }
        /// <summary>
        /// Whether or not this AI's auto attacks apply damage to their target immediately after their cast time ends.
        /// </summary>
        bool IsMelee { get; set; }
        /// <summary>
        /// Current order this AI is performing.
        /// </summary>
        /// TODO: Rework AI so this enum can be used fully.
        MoveOrder MoveOrder { get; }
        /// <summary>
        /// Unit this AI will auto attack when it is in auto attack range.
        /// </summary>
        IAttackableUnit TargetUnit { get; set;  }

        /// <summary>
        /// Adds the given buff instance to this AI.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// TODO: Probably needs a refactor to lessen thread usage. Make sure to stick very closely to the current method; just optimize it.
        /// TODO: Move to AttackableUnit.
        void AddBuff(IBuff b);
        /// <summary>
        /// Adds a modifier to this AI's stats, ex: Armor, Attack Damage, Movespeed, etc.
        /// </summary>
        /// <param name="statModifier">Modifier to add.</param>
        /// TODO: Move to AttackableUnit.
        void AddStatModifier(IStatsModifier statModifier);
        /// <summary>
        /// Applies the specified crowd control to this AI, refer to CrowdControlType for examples.
        /// </summary>
        /// <param name="cc">Crowd control to apply.</param>
        /// TODO: Make a CrowdControl class which contains info. about the time the CC is applied, and functions to stop it on command (for scripts).
        void ApplyCrowdControl(ICrowdControl cc);
        /// <summary>
        /// Function called by this AI's auto attack projectile when it hits its target.
        /// </summary>
        void AutoAttackHit(IAttackableUnit target);
        /// <summary>
        /// Sets this AI's current auto attack to the given auto attack. *NOTE*: Will be deprecated when spells are fully implemented.
        /// </summary>
        /// <param name="newAutoAttackSpellDataName">Name of the auto attack to use.</param>
        void ChangeAutoAttackSpellData(string newAutoAttackSpellDataName);
        /// <summary>
        /// Sets this AI's current auto attack to the given auto attack. *NOTE*: Will be deprecated when spells are fully implemented.
        /// </summary>
        /// <param name="newAutoAttackSpellData">Auto attack spell data to use.</param>
        void ChangeAutoAttackSpellData(ISpellData newAutoAttackSpellData);
        /// <summary>
        /// Clears all crowd control from this AI.
        /// </summary>
        void ClearAllCrowdControl();
        /// <summary>
        /// Forces this AI unit to perform a dash which follows the specified GameObject.
        /// </summary>
        /// <param name="target">GameObject to follow.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">How much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        void DashToTarget(IGameObject target, float dashSpeed, string animation, float leapGravity, bool keepFacingLastDirection, float followTargetMaxDistance, float backDistance, float travelTime);
        /// <summary>
        /// Gets the parent buff instance of the buffs of the given name.
        /// </summary>
        /// <param name="name">Internal buff name to check.</param>
        /// <returns>Parent buff instance.</returns>
        /// TODO: Move to AttackableUnit.
        IBuff GetBuffWithName(string name);
        /// <summary>
        /// Gets a list of all buffs applied to this AI (parents and children).
        /// </summary>
        /// <returns>List of buff instances.</returns>
        /// TODO: Move to AttackableUnit.
        List<IBuff> GetBuffs();
        /// <summary>
        /// Gets the number of parent buffs applied to this AI.
        /// </summary>
        /// <returns>Number of parent buffs.</returns>
        /// TODO: Move to AttackableUnit.
        int GetBuffsCount();
        /// <summary>
        /// Gets a list of all buff instances of the given name (parent and children).
        /// </summary>
        /// <param name="buffName">Internal buff name to check.</param>
        /// <returns>List of buff instances.</returns>
        /// TODO: Move to AttackableUnit.
        List<IBuff> GetBuffsWithName(string buffName);
        /// <summary>
        /// Gets a new buff slot for the given buff instance.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// <returns>Byte buff slot of the given buff.</returns>
        /// TODO: Move to AttackableUnit.
        byte GetNewBuffSlot(IBuff b);
        /// <summary>
        /// Gets the HashString for this AI's model. Used for packets so clients know what data to load.
        /// </summary>
        /// <returns>Hashed string of this AI's model.</returns>
        /// TODO: Move to AttackableUnit.
        uint GetObjHash();
        /// <summary>
        /// Whether or not this AI has the given buff instance.
        /// </summary>
        /// <param name="buff">Buff instance to check.</param>
        /// <returns>True/False.</returns>
        /// TODO: Move to AttackableUnit.
        bool HasBuff(IBuff buff);
        /// <summary>
        /// Whether or not this AI has a buff of the given name.
        /// </summary>
        /// <param name="buffName">Internal buff name to check for.</param>
        /// <returns>True/False.</returns>
        /// TODO: Move to AttackableUnit.
        bool HasBuff(string buffName);
        /// <summary>
        /// Whether or not this AI is affected by the given crowd control.
        /// </summary>
        /// <param name="ccType">Crowd control to check for.</param>
        /// <returns>True/False.</returns>
        bool HasCrowdControl(CrowdControlType ccType);
        /// <summary>
        /// Removes the given buff from this AI.
        /// </summary>
        /// <param name="b">Buff to remove.</param>
        /// TODO: Move to AttackableUnit.
        void RemoveBuff(IBuff b);
        /// <summary>
        /// Removes the parent buff of the given internal name from this AI object.
        /// </summary>
        /// <param name="b">Internal buff name to remove.</param>
        /// TODO: Move to AttackableUnit.
        void RemoveBuff(string b);
        /// <summary>
        /// Removes all buffs of the given internal name from this AI object.
        /// Also removes parent buffs.
        /// </summary>
        /// <param name="buffName">Internal buff name to remove.</param>
        /// TODO: Move to AttackableUnit.
        void RemoveBuffsWithName(string buffName);
        /// <summary>
        /// Removes the given crowd control instance from this AI.
        /// </summary>
        /// <param name="cc">Crowd control instance to remove.</param>
        void RemoveCrowdControl(ICrowdControl cc);
        /// <summary>
        /// Removes the given stat modifier instance from this AI.
        /// </summary>
        /// <param name="statModifier">Stat modifier instance to remove.</param>
        /// TODO: Move to AttackableUnit.
        void RemoveStatModifier(IStatsModifier statModifier);
        /// <summary>
        /// Sets this AI's current auto attack to their base auto attack.
        /// *NOTE*: Will be depricated when spell systems are fully implemented.
        /// </summary>
        void ResetAutoAttackSpellData();
        /// <summary>
        /// Sets this AI's current target unit. This relates to both auto attacks as well as general spell targeting.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        /// TODO: Remove Target class.
        void SetTargetUnit(IAttackableUnit target);
        /// <summary>
        /// Sets this unit's move order to the given order.
        /// </summary>
        /// <param name="order">MoveOrder to set.</param>
        void UpdateMoveOrder(MoveOrder order);
        /// <summary>
        /// Sets this AI's current target unit.
        /// </summary>
        /// <param name="unit">Unit to target.</param>
        void UpdateTargetUnit(IAttackableUnit unit);
    }
}
