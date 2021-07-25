using System;

namespace GameServerCore.Enums
{
    /// <summary>
    /// Enumerator containing all(?) possible FX flags.
    /// </summary>
    /// TODO: Finish labeling enum and verify if there are other possible values.
    [Flags]
    public enum FXFlags
    {
        /// <summary>
        /// Particle will face the given direction.
        /// </summary>
        GivenDirection = 1 << 4,
        /// <summary>
        /// Particle will face the same direction as the object it is bound to.
        /// </summary>
        BindDirection = 1 << 5,
        Unknown3 = GivenDirection + BindDirection,
        Unknown4 = 1 << 6,
        /// <summary>
        /// Particle will face towards its target.
        /// </summary>
        TargetDirection = 1 << 7,
        Unknown6 = BindDirection + TargetDirection,
        Unknown7 = 1 << 8,
        Unknown8 = GivenDirection + Unknown7,
        Unknown9 = BindDirection + TargetDirection + Unknown7,
        Unknown10 = BindDirection + Unknown4 + TargetDirection + Unknown7
    }
}