using GameServerCore.Enums;

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
        /// Stats used purely in networking the accompishments or status of units and their gameplay affecting stats.
        /// </summary>
        IReplication Replication { get; }
        /// <summary>
        /// Variable housing all of this Unit's stats such as health, mana, armor, magic resist, ActionState, etc.
        /// Currently these are only initialized manually by ObjAIBase and ObjBuilding.
        /// </summary>
        IStats Stats { get; }

        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
        void Die(IAttackableUnit killer);
        /// <summary>
        /// Sets whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to change.</param>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetableToTeam(TeamId team, bool targetable);
        /// <summary>
        /// Sets whether or not this unit should be targetable.
        /// </summary>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetable(bool targetable);
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
        /// Sets this unit's current model to the specified internally named model. *NOTE*: If the model is not present in the client files, all connected players will crash.
        /// </summary>
        /// <param name="model">Internally named model to set.</param>
        /// <returns></returns>
        /// TODO: Implement model verification so that clients don't crash if a model which doesn't exist in client files is given.
        bool ChangeModel(string model);
    }
}
