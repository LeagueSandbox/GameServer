using System;

namespace GameServerCore.Enums
{
    /// <summary>
    /// Enum of all animation flags that can be applied to an animation. Used for packets.
    /// *NOTE*: Descriptions may not be accurate as the flags have yet to be fully explored.
    /// </summary>
    /// TODO: Finish this.
    [Flags]
    public enum AnimationFlags : byte
    {
        /// <summary>
        /// Seems to prevent animations of the same name from playing until the first one with this flag is finished.
        /// Also seems to override automatic animations such as RUN.
        /// </summary>
        UniqueOverride = 1 << 0,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown2 = 1 << 1,
        /// <summary>
        /// Seems to override automatic animations such as RUN.
        /// </summary>
        Override = 1 << 2,
        /// <summary>
        /// Seems to lock animations at the end of an animation.
        /// </summary>
        Lock = 1 << 3,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown5 = 1 << 4,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown6 = 1 << 5,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown7 = 1 << 6,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown8 = 1 << 7
    }
}