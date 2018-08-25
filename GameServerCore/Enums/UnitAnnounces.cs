namespace GameServerCore.Enums
{
    public enum UnitAnnounces : byte
    {
        DEATH = 0x04,
        INHIBITOR_DESTROYED = 0x1F,
        INHIBITOR_ABOUT_TO_SPAWN = 0x20,
        INHIBITOR_SPAWNED = 0x21,
        TURRET_DESTROYED = 0x24,
        SUMMONER_DISCONNECTED = 0x47,
        SUMMONER_RECONNECTED = 0x48
    }
}