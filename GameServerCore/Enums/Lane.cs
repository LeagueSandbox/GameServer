namespace GameServerCore.Enums
{
    /// <summary>
    /// Enum used to specify the position of a turret on the map.
    /// Used for ObjectManager.
    /// </summary>
    public enum Lane
    {
        LANE_R = 0x0,
        LANE_C = 0x1,
        LANE_L = 0x2,
        LANE_3 = 0x3,
        LANE_4 = 0x4,
        LANE_5 = 0x5,
        LANE_6 = 0x6,
        LANE_7 = 0x7,
        LANE_8 = 0x8,
        LANE_9 = 0x9,
        LANE_Numof = 0xA,
        LANE_Unknown = 0xB,
    }
}