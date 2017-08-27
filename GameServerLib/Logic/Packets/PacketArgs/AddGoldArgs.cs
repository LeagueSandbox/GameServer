using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AddGoldArgs
    {
        public uint AcceptorNetId { get; }
        public uint DonorNetId { get; }
        public float Amount { get; }

        public AddGoldArgs(uint acceptorNetId, uint donorNetId, float amount)
        {
            AcceptorNetId = acceptorNetId;
            DonorNetId = donorNetId;
            Amount = amount;
        }
    }
}
