namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public enum Pings : byte
    {
        PING_DEFAULT = 0,
        PING_DANGER = 2,
        PING_MISSING = 3,
        PING_ON_MY_WAY = 4,
        PING_ASSIST = 6
    }
}