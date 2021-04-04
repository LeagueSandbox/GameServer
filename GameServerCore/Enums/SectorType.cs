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
        /// </summary>
        Area = 0x1,
        /// <summary>
        /// Location and direction targeted area of effect in the shape of a wedge (circular sector). Shape determined by sector angle.
        /// </summary>
        Cone = 0x2,
        /// <summary>
        /// Location and direction targeted area of effect in the shape of a polygon with edges converging on specified points.
        /// </summary>
        Polygon = 0x3,
        /// <summary>
        /// Location targeted area of effect which follows the path of a wave function and has an area of effect in the shape of the wave's crest/trough.
        /// </summary>
        Wave = 0x4,
    }
}