namespace GameServerCore.Packets.Enums
{
    public enum Pings : byte
    {
        PING_DEFAULT = 0,
        PING_ATTACK = 1,
        PING_DANGER = 2,
        PING_MISSING = 3,
        PING_ON_MY_WAY = 4,
        PING_FALLBACK = 5,
        PING_ASSIST = 6
    }
}