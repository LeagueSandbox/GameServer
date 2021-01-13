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
        /// The "score" of this Unit which increases as kills are gained and decreases as deaths are inflicted.
        /// Used in determining kill gold rewards.
        /// </summary>
        int KillDeathCounter { get; set; }
        /// <summary>
        /// Number of minions this Unit has killed. Unused besides in replication which is used for packets, refer to NotifyUpdateStats in PacketNotifier.
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
        /// Whether or not this unit is currently dashing.
        /// </summary>
        bool IsDashing { get; }
        /// <summary>
        /// Speed of the unit's current dash.
        /// </summary>
        /// TODO: Implement a dash class so dash based variables and functions can be separate from units.
        float DashSpeed { get; set; }
        /// <summary>
        /// Amount of time passed since the unit started dashing.
        /// </summary>
        /// TODO: Implement a dash class so dash based variables and functions can be separate from units.
        float DashElapsedTime { get; set; }
        /// <summary>
        /// Total amount of time the unit will dash.
        /// </summary>
        /// TODO: Implement a dash class so dash based variables and functions can be separate from units.
        public float DashTime { get; set; }
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
        /// Returns whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to check for.</param>
        /// <returns>True/False.</returns>
        bool GetIsTargetableToTeam(TeamId team);
        /// <summary>
        /// Sets whether or not this unit should be targetable.
        /// </summary>
        /// <param name="targetable">True/False.</param>
        void SetIsTargetable(bool targetable);
        /// <summary>
        /// Sets whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to change.</param>
        /// <param name="targetable">True/False.</param>
        void SetIsTargetableToTeam(TeamId team, bool targetable);
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
        /// <summary>
        /// Whether or not this unit is currently calling for help. Unimplemented.
        /// </summary>
        /// <returns>True/False.</returns>
        /// TODO: Implement this.
        bool IsInDistress();
        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
        void Die(IAttackableUnit killer);
        /// <summary>
        /// Sets this unit's current model to the specified internally named model. *NOTE*: If the model is not present in the client files, all connected players will crash.
        /// </summary>
        /// <param name="model">Internally named model to set.</param>
        /// <returns></returns>
        /// TODO: Implement model verification so that clients don't crash if a model which doesn't exist in client files is given.
        bool ChangeModel(string model);
        /// <summary>
        /// Gets the movement speed stat of this unit.
        /// </summary>
        /// <returns>Float units/sec.</returns>
        float GetMoveSpeed();
        /// <summary>
        /// Returns the next waypoint. If all waypoints have been reached then this returns a -inf Vector2
        /// </summary>
        Vector2 GetNextWaypoint();
        /// <summary>
        /// Returns whether this unit has reached the last waypoint in its path of waypoints.
        /// </summary>
        bool IsPathEnded();
        /// <summary>
        /// Sets this unit's movement path to the given waypoints.
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        void SetWaypoints(List<Vector2> newWaypoints);
        /// <summary>
        /// Forces this unit to stop moving.
        /// </summary>
        void StopMovement();
        /// <summary>
        /// Returns whether this unit has set its waypoints this update.
        /// </summary>
        /// <returns>True/False</returns>
        bool IsMovementUpdated();
        /// <summary>
        /// Used each object manager update after this unit has set its waypoints and the server has networked it.
        /// </summary>
        void ClearMovementUpdated();
        /// <summary>
        /// Forces this unit to perform a dash which ends at the given position.
        /// </summary>
        /// <param name="endPos">Position to end the dash at.</param>
        /// <param name="dashSpeed">Amount of units the dash should travel in a second (movespeed).</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the AI unit should face the direction they were facing before the dash.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        void DashToLocation(Vector2 endPos, float dashSpeed, string animation, float leapGravity = 0.0f, bool keepFacingLastDirection = true);
        /// <summary>
        /// Sets this unit's current dash state to the given state.
        /// </summary>
        /// <param name="state">State to set. True = dashing, false = not dashing.</param>
        /// TODO: Verify if we want to classify Dashing as a form of Crowd Control.
        void SetDashingState(bool state);
    }
}
