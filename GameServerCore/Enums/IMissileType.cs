namespace GameServerCore.Enums
{
    public enum MissileType : int
    {
        /// <summary>
        /// Unused. If a MissileGameScript is implemented which controls flight path, use this (possibly rename to "Custom").
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Single target missile.
        /// </summary>
        Target = 0x1,
        /// <summary>
        /// Single target missile which can change target after reaching its current target.
        /// </summary>
        Chained = 0x2,
        /// <summary>
        /// Location targeted missile which moves in a set direction.
        /// </summary>
        Circle = 0x3,
        /// <summary>
        /// Missile which has an angular velocity and can be attached to a unit to orbit around or can end at a target location.
        /// </summary>
        Arc = 0x4,
    }
}