using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Base class for all attackable units.
    /// AttackableUnits normally follow these guidelines of functionality: Death state, Stats (including modifiers and basic replication), Buffs (and their scripts), and Call for Help.
    /// </summary>
    public interface IAttackableUnit : IGameObject
    {
        /// <summary>
        /// Whether or not this Unit is dead. Refer to TakeDamage() and Die().
        /// </summary>
        bool IsDead { get; }
        /// <summary>
        /// Whether or not this Unit's model has been changeds this tick. Resets to False when the next tick update happens in ObjectManager.
        /// </summary>
        bool IsModelUpdated { get; set; }
        /// <summary>
        /// Number of minions this Unit has killed. Unused besides in replication which is used for packets, refer to NotifyOnReplication in PacketNotifier.
        /// </summary>
        /// TODO: Verify if we want to move this to ObjAIBase since AttackableUnits cannot attack or kill anything.
        int MinionCounter { get; }
        /// <summary>
        /// This Unit's current internally named model.
        /// </summary>
        string Model { get; }
        /// <summary>
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        /// TODO: Move this to ObjAIBase, as neither GameObjects nor AttackableUnits should be able to move or target.
        List<Vector2> Waypoints { get; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is currently on.
        /// </summary>
        KeyValuePair<int, Vector2> CurrentWaypoint { get; }
        /// <summary>
        /// Status effects enabled on this unit. Refer to StatusFlags enum.
        /// </summary>
        StatusFlags Status { get; }
        /// <summary>
        /// Parameters of any forced movements (dashes) this unit is performing.
        /// </summary>
        IForceMovementParameters MovementParameters { get; }
        /// <summary>
        /// Stats used purely in networking the accompishments or status of units and their gameplay affecting stats.
        /// </summary>
        IReplication Replication { get; }
        /// <summary>
        /// Variable housing all of this Unit's stats such as health, mana, armor, magic resist, ActionState, etc.
        /// Currently these are only initialized manually by ObjAIBase and ObjBuilding.
        /// </summary>
        IStats Stats { get; }
        /// <summary>
        /// Variable which stores the number of times a unit has teleported. Used purely for networking.
        /// Resets when reaching byte.MaxValue (255).
        /// </summary>
        byte TeleportID { get; set; }

        /// <summary>
        /// Gets the HashString for this unit's model. Used for packets so clients know what data to load.
        /// </summary>
        /// <returns>Hashed string of this unit's model.</returns>
        uint GetObjHash();
        /// <summary>
        /// Sets the server-sided position of this object. Optionally takes into account the AI's current waypoints.
        /// </summary>
        /// <param name="vec">Position to set.</param>
        /// <param name="repath">Whether or not to repath the AI from the given position (assuming it has a path).</param>
        void SetPosition(Vector2 vec, bool repath = true);
        /// <summary>
        /// Returns whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to check for.</param>
        /// <returns>True/False.</returns>
        bool GetIsTargetableToTeam(TeamId team);
        /// <summary>
        /// Sets whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to change.</param>
        /// <param name="targetable">True/False.</param>
        void SetIsTargetableToTeam(TeamId team, bool targetable);
        /// <summary>
        /// Whether or not this unit can move itself.
        /// </summary>
        /// <returns></returns>
        bool CanMove();
        /// <summary>
        /// Whether or not this unit can modify its Waypoints.
        /// </summary>
        bool CanChangeWaypoints();
        /// <summary>
        /// Whether or not this unit can take damage of the given type.
        /// </summary>
        /// <param name="type">Type of damage to check.</param>
        /// <returns>True/False</returns>
        bool CanTakeDamage(DamageType type);
        /// <summary>
        /// Adds a modifier to this unit's stats, ex: Armor, Attack Damage, Movespeed, etc.
        /// </summary>
        /// <param name="statModifier">Modifier to add.</param>
        void AddStatModifier(IStatsModifier statModifier);
        /// <summary>
        /// Removes the given stat modifier instance from this unit.
        /// </summary>
        /// <param name="statModifier">Stat modifier instance to remove.</param>
        void RemoveStatModifier(IStatsModifier statModifier);
        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="damageText">Type of damage the damage text should be.</param>
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, DamageResultType damageText);
        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="isCrit">Whether or not the damage text should be shown as a crit.</param>
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit);
        void TakeDamage(IDamageData damageData, DamageResultType damageText);
        void TakeDamage(IDamageData damageData, bool isCrit);
        /// <summary>
        /// Whether or not this unit is currently calling for help. Unimplemented.
        /// </summary>
        /// <returns>True/False.</returns>
        /// TODO: Implement this.
        bool IsInDistress();
        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="data">Data of the death.</param>
        void Die(IDeathData data);
        /// <summary>
        /// Sets this unit's current model to the specified internally named model. *NOTE*: If the model is not present in the client files, all connected players will crash.
        /// </summary>
        /// <param name="model">Internally named model to set.</param>
        /// <returns></returns>
        /// TODO: Implement model verification so that clients don't crash if a model which doesn't exist in client files is given.
        bool ChangeModel(string model);
        /// <summary>
        /// Adds the given buff instance to this unit.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// TODO: Probably needs a refactor to lessen thread usage. Make sure to stick very closely to the current method; just optimize it.
        void AddBuff(IBuff b);
        /// <summary>
        /// Whether or not this unit has the given buff instance.
        /// </summary>
        /// <param name="buff">Buff instance to check.</param>
        /// <returns>True/False.</returns>
        bool HasBuff(IBuff buff);
        /// <summary>
        /// Whether or not this unit has a buff of the given name.
        /// </summary>
        /// <param name="buffName">Internal buff name to check for.</param>
        /// <returns>True/False.</returns>
        bool HasBuff(string buffName);
        /// <summary>
        /// Whether or not this unit has a buff of the given type.
        /// </summary>
        /// <param name="type">BuffType to check for.</param>
        /// <returns>True/False.</returns>
        bool HasBuffType(BuffType type);
        /// <summary>
        /// Gets a new buff slot for the given buff instance.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// <returns>Byte buff slot of the given buff.</returns>
        byte GetNewBuffSlot(IBuff b);
        /// <summary>
        /// Gets the parent buff instance of the buffs of the given name. Parent buffs control stack count for buffs of the same name.
        /// </summary>
        /// <param name="name">Internal buff name to check.</param>
        /// <returns>Parent buff instance.</returns>
        IBuff GetBuffWithName(string name);
        /// <summary>
        /// Gets a dictionary of all parent buff instances and their names.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, IBuff> GetParentBuffs();
        /// <summary>
        /// Gets a list of all buffs applied to this unit (parent and children).
        /// </summary>
        /// <returns>List of buff instances.</returns>
        List<IBuff> GetBuffs();
        /// <summary>
        /// Gets the number of parent buffs applied to this unit.
        /// </summary>
        /// <returns>Number of parent buffs.</returns>
        public int GetBuffsCount();
        /// <summary>
        /// Gets a list of all buff instances of the given name (parent and children).
        /// </summary>
        /// <param name="buffName">Internal buff name to check.</param>
        /// <returns>List of buff instances.</returns>
        List<IBuff> GetBuffsWithName(string buffName);
        /// <summary>
        /// Removes the given buff from this unit. Called automatically when buff timers have finished.
        /// Buffs with BuffAddType.STACKS_AND_OVERLAPS are removed incrementally, meaning one instance removed per RemoveBuff call.
        /// Other BuffAddTypes are removed entirely, regardless of stacks. DecrementStackCount can be used as an alternative.
        /// </summary>
        /// <param name="b">Buff to remove.</param>
        void RemoveBuff(IBuff b);
        /// <summary>
        /// Removes all buffs of the given internal name from this unit regardless of stack count.
        /// Intended mainly for buffs with BuffAddType.STACKS_AND_OVERLAPS.
        /// </summary>
        /// <param name="buffName">Internal buff name to remove.</param>
        void RemoveBuffsWithName(string buffName);
        /// <summary>
        /// Gets the movement speed stat of this unit (units/sec).
        /// </summary>
        /// <returns>Float units/sec.</returns>
        float GetMoveSpeed();
        /// <summary>
        /// Teleports this unit to the given position, and optionally repaths from the new position.
        /// </summary>
        /// <param name="x">X coordinate to teleport to.</param>
        /// <param name="y">Y coordinate to teleport to.</param>
        /// <param name="repath">Whether or not to repath from the new position.</param>
        void TeleportTo(float x, float y, bool repath = false);
        /// <summary>
        /// Returns the next waypoint. If all waypoints have been reached then this returns a -inf Vector2
        /// </summary>
        Vector2 GetNextWaypoint();
        /// <summary>
        /// Returns whether this unit has reached the last waypoint in its path of waypoints.
        /// </summary>
        bool IsPathEnded();
        /// <summary>
        /// Sets this unit's movement path to the given waypoints. *NOTE*: Requires current position to be prepended.
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        /// <param name="networked">Whether or not clients should be notified of this change in waypoints at the next ObjectManager.Update.</param>
        void SetWaypoints(List<Vector2> newWaypoints, bool networked = true);
        /// <summary>
        /// Forces this unit to stop moving.
        /// </summary>
        void StopMovement();
        /// <summary>
        /// Returns whether this unit's waypoints will be networked to clients the next update. Movement updates do not occur for dash based movements.
        /// </summary>
        /// <returns>True/False</returns>
        /// TODO: Refactor movement update logic so this can be applied to any kind of movement.
        bool IsMovementUpdated();
        /// <summary>
        /// Used each object manager update after this unit has set its waypoints and the server has networked it.
        /// </summary>
        void ClearMovementUpdated();
        /// <summary>
        /// Enables or disables the given status on this unit.
        /// </summary>
        /// <param name="status">StatusFlag to enable/disable.</param>
        /// <param name="enabled">Whether or not to enable the flag.</param>
        void SetStatus(StatusFlags status, bool enabled);
        /// <summary>
        /// Forces this unit to perform a dash which ends at the given position.
        /// </summary>
        /// <param name="endPos">Position to end the dash at.</param>
        /// <param name="dashSpeed">Amount of units the dash should travel in a second (movespeed).</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the AI unit should face the direction they were facing before the dash.</param>
        /// <param name="consideredCC">Whether or not to prevent movement, casting, or attacking during the duration of the movement.</param>
        /// TODO: Find a good way to grab these variables from spell data.
        /// TODO: Verify if we should count Dashing as a form of Crowd Control.
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        void DashToLocation(Vector2 endPos, float dashSpeed, string animation = "", float leapGravity = 0.0f, bool keepFacingLastDirection = true, bool consideredCC = true);
        /// <summary>
        /// Sets this unit's current dash state to the given state.
        /// </summary>
        /// <param name="state">State to set. True = dashing, false = not dashing.</param>
        /// TODO: Implement ForcedMovement methods and enumerators to handle different kinds of dashes.
        void SetDashingState(bool state, MoveStopReason reason = MoveStopReason.Finished);
        /// <summary>
        /// Sets this unit's animation states to the given set of states.
        /// Given state pairs are expected to follow a specific structure:
        /// First string is the animation to override, second string is the animation to play in place of the first.
        /// <param name="animPairs">Dictionary of animations to set.</param>
        void SetAnimStates(Dictionary<string, string> animPairs);
    }
}
