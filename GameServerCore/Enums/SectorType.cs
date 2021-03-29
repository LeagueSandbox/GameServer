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
        Cone = 0x2
    }
}