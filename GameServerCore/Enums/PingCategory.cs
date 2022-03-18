using System;
namespace GameServerCore.Enums
{
    public enum PingCategory : byte
    {
        Command = 0x0,
        Attack = 0x1,
        Danger = 0x2,
        Missing = 0x3,
        OnMyWay = 0x4,
        Fallback = 0x5,
        RequestHelp = 0x6,
    }
}
