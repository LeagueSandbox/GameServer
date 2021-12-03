using System;

namespace GameServerCore.Enums
{
    /// <summary>
    /// Enumerator detailing all game feature flags. Used for enabling/disabling core features and in networking.
    /// </summary>
    /// TODO: Finish this.
    [Flags]
    public enum FeatureFlags : uint
    {
        None,
        EnableCooldowns = 1 << 1,
        EnableManaCosts = 1 << 2,
        EnableLaneMinions = 1 << 3
    }
}