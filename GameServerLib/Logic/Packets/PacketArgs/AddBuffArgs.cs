using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AddBuffArgs
    {
        public uint TargetNetId { get; }
        public uint SourceNetId { get; }
        public int Stacks { get; }
        public float Time { get; }
        public BuffType BuffType { get; }
        public string Name { get; }
        public byte Slot { get; }

        public AddBuffArgs(uint targetNetId, uint sourceNetId, int stacks, float time, BuffType buffType, string name,
            byte slot)
        {
            TargetNetId = targetNetId;
            SourceNetId = sourceNetId;
            Stacks = stacks;
            Time = time;
            BuffType = buffType;
            Name = name;
            Slot = slot;
        }
    }
}
