using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AddXpArgs
    {
        public uint AddToNetId { get; }
        public float Amount { get; }

        public AddXpArgs(uint addToNetId, float amount)
        {
            AddToNetId = addToNetId;
            Amount = amount;
        }
    }
}
