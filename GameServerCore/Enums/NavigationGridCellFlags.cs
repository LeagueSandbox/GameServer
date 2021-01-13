namespace GameServerCore.Enums
{
    public enum NavigationGridCellFlags : ushort
    {
        HAS_GRASS = 0x1,
        NOT_PASSABLE = 0x2,
        BUSY = 0x4,
        TARGETTED = 0x8,
        MARKED = 0x10,
        PATHED_ON = 0x20,
        SEE_THROUGH = 0x40,
        OTHER_DIRECTION_END_TO_START = 0x80,
        HAS_GLOBAL_VISION = 0x100,
        // HAS_TRANSPARENT_TERRAIN = 0x42 // (SeeThrough | NotPassable)
    }
}