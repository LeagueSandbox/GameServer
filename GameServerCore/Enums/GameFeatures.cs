using System;

namespace GameServerCore.Enums
{
    /// <summary>
    /// Enumerator detailing all game feature flags. Used for enabling/disabling core features and in networking.
    /// </summary>
    [Flags]
    public enum GameFeatures : ulong
    {
        Equalize = 1 << 0x0,
        FoundryOptions = 1 << 0x1,
        OldOptions = 1 << 0x2,
        FoundryQuicChat = 1 << 0x3,
        EarlyWarningForFOWMissiles = 1 << 0x4,
        AnimatedCursors = 1 << 0x5,
        ItemUndo = 1 << 0x6,
        NewPlayerRecommendedPages = 1 << 0x7,
        HighlightLineMissileTargets = 1 << 0x8,
        ControlledChampionIndicator = 1 << 0x9,
        AlternateBountySystem = 1 << 0xA,
        NewMinionSpawnOrder = 1 << 0xB,
        TurretRangeIndicators = 1 << 0xC,
        GoldSourceInfoLogDump = 1 << 0xD,
        ParticleSinNameTech = 1 << 0xE,
        NetworMetrics_1 = 1 << 0xF,
        HardwareMetrics_1 = 1 << 0x10,
        TruLagMetrics = 1 << 0x11,
        DradisSD = 1 << 0x12,
        ServerIPLogging = 1 << 0x13,
        JungleTimers = 1 << 0x14,
        TraceRouteMetrics = 1 << 0x15,
        IsLolbug19805LoggingEnabled = 1 << 0x16,
        IsLolbug19805HacyTourniquetEnabled = 1 << 0x17,
        TurretMemory = 1 << 0x18,
        TimerSyncForReplay = 1 << 0x19,
        RegisterWithLocalServiceDiscovery = 1 << 0x1A,
        MinionFarmingBounty = 1 << 0x1B,
        TeleportToDestroyedTowers = 1 << 0x1C,
        NonRefCountedCharacterStates = 1 << 0x1D
    }
}