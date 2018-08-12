namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public enum Pings : byte
    {
        Ping_Default = 0,
        Ping_Danger = 2,
        Ping_Missing = 3,
        Ping_OnMyWay = 4,
        Ping_Assist = 6
    }
}