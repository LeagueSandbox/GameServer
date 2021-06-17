namespace GameServerCore.Enums
{
    /// <summary>
    /// Enum used to specify the position of a turret on the map.
    /// Used for ObjectManager.
    /// </summary>
    public enum LaneID
    {
        BOTTOM = 0,
        MIDDLE = 1,
        TOP = 2,
        /// <summary>
        /// Used for newly created turrets and Azir turrets
        /// </summary>
        NONE = 3
    }
}