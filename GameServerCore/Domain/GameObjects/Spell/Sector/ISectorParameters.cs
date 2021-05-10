using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects.Spell.Sector
{
    /// <summary>
    /// Parameters which determine how a sector behaves.
    /// </summary>
    public interface ISectorParameters
    {
        /// <summary>
        /// Area around the sector to check for collisions.
        /// </summary>
        float Radius { get; set; }
        /// <summary>
        /// Maximum amount of time this spell sector should last before being automatically removed.
        /// Setting to -1 will cause the spell sector to last until manually removed.
        /// </summary>
        float Lifetime { get; set; }
        /// <summary>
        /// How many times a second the spell sector should check for hitbox collisions.
        /// </summary>
        int Tickrate { get; set; }
        /// <summary>
        /// Whether or not the spell sector should be able to hit something multiple times.
        /// Will only hit again if the unit hit re-enters the hitbox (constant per-collision hitbox).
        /// Is overridden by CanHitSameTargetConsecutively.
        /// </summary>
        bool CanHitSameTarget { get; set; }
        /// <summary>
        /// Whether or not the spell sector should be able to hit something multiple times in a row,
        /// regardless of if it has left and re-entered the hitbox (costant hitbox).
        /// Overrides CanHitSameTarget.
        /// </summary>
        bool CanHitSameTargetConsecutively { get; set; }

        /// <summary>
        /// Maximum number of times the spell sector can hit something before being removed.
        /// </summary>
        int MaximumHits { get; set; }

        /// <summary>
        /// Optional spell flags which determine what units this spell sector affects.
        /// If 0, the sector will use the SpellOrigin's spell flags.
        /// </summary>
        SpellDataFlags OverrideFlags { get; set; }

        /// <summary>
        /// What kind of shape the sector has.
        /// </summary>
        SectorType Type { get; set; }
    }
}