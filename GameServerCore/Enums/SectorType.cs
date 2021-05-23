namespace GameServerCore.Enums
{
    public enum SectorType : int
    {
        /// <summary>
        /// Unused. If a SectorGameScript is implemented which controls sector behavior, use this (possibly rename to "Custom").
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Location targeted area of effect in the shape of a circle.
        /// If the spell which created the sector is targeted, the sector will originate at the target.
        /// </summary>
        Area = 0x1,
        /// <summary>
        /// Location and direction targeted area of effect in the shape of a wedge (circular sector). Shape determined by sector angle.
        /// If the spell which created the sector is targeted, the sector will originate at the target, and will use the target's facing direction.
        /// </summary>
        Cone = 0x2,
        /// <summary>
        /// Location and direction targeted area of effect in the shape of a polygon with edges converging on specified points.
        /// If the spell which created the sector is targeted, the sector will originate at the target, and will use the target's facing direction.
        /// </summary>
        Polygon = 0x3,
        /// <summary>
        /// Location and direction targeted area of effect in the shape of a ring (annulus).
        /// </summary>
        Ring = 0x4,
    }
}