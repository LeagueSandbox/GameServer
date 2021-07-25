using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IDeathData
    {
        /// <summary>
        /// Whether or not the death should result in resurrection as a zombie.
        /// </summary>
        bool BecomeZombie { get; }
        /// <summary>
        /// The type of death. Values unknown.
        /// </summary>
        /// TODO: Create an enum for this.
        byte DieType { get; }
        /// <summary>
        /// Unit which is dying.
        /// </summary>
        IAttackableUnit Unit { get; }
        /// <summary>
        /// Unit which is responsible for the death.
        /// </summary>
        IAttackableUnit Killer { get; }
        /// <summary>
        /// Type of damage with caused the death.
        /// </summary>
        DamageType DamageType { get; }
        /// <summary>
        /// Source of the damage which caused the death.
        /// </summary>
        DamageSource DamageSource { get; }
        /// <summary>
        /// Time until death finishes (fade-out duration?).
        /// </summary>
        float DeathDuration { get; }
    }
}