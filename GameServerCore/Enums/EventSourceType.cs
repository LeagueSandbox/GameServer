using System;
namespace GameServerCore.Enums
{
    public enum EventSourceType : byte
    {
        Hero = 0x0,
        Minion = 0x1,
        Tower = 0x2,
        Pet = 0x3,
        Clone = 0x4,
        Unknown = 0x5,
    }
}
