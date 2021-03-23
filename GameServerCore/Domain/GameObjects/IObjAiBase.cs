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
        /// Unit this AI will dash to (assuming they are performing a targeted dash).
        /// </summary>
        IAttackableUnit DashTarget { get; }

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
        /// Forces this AI unit to perform a dash which follows the specified AttackableUnit.
        /// </summary>
        /// <param name="target">Unit to follow.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">How much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        void DashToTarget(IAttackableUnit target, float dashSpeed, string animation, float leapGravity, bool keepFacingLastDirection, float followTargetMaxDistance, float backDistance, float travelTime);
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
