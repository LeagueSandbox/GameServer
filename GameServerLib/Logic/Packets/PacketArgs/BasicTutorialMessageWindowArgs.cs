using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct BasicTutorialMessageWindowArgs
    {
        public string Message { get; }

        public BasicTutorialMessageWindowArgs(string message)
        {
            Message = message;
        }
    }
}
