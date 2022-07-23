using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace GameServerLib.GameObjects.AttackableUnits
{
    public class DeathData
    {
        /// <summary>
        /// Whether or not the death should result in resurrection as a zombie.
        /// </summary>
        public bool BecomeZombie { get; set; }
        /// <summary>
        /// The type of death. Values unknown.
        /// </summary>
        /// TODO: Create an enum for this.
        public byte DieType { get; set; }
        /// <summary>
        /// Unit which is dying.
        /// </summary>
        public AttackableUnit Unit { get; set; }
        /// <summary>
        /// Unit which is responsible for the death.
        /// </summary>
        public AttackableUnit Killer { get; set; }
        /// <summary>
        /// Type of damage with caused the death.
        /// </summary>
        public DamageType DamageType { get; set; }
        /// <summary>
        /// Source of the damage which caused the death.
        /// </summary>
        public DamageSource DamageSource { get; set; }
        /// <summary>
        /// Time until death finishes (fade-out duration?).
        /// </summary>
        public float DeathDuration { get; set; }
    }
}
