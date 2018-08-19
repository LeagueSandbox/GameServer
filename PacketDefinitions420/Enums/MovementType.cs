using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketDefinitions420.Enums
{
    public enum MovementType : byte
    {
        EMOTE = 1,
        MOVE = 2,
        ATTACK = 3,
        ATTACKMOVE = 7,
        STOP = 10
    }
}
