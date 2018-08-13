using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class CastSpellRequest
    {
        public int NetId { get; }
        public byte SpellSlot { get; }
        public float X { get; }
        public float Y { get; }
        public float X2 { get; }
        public float Y2 { get; }
        public uint TargetNetId { get; }

        public CastSpellRequest(int netId, byte spellSlot, float x, float y, float x2, float y2, uint targetNetId)
        {
            NetId = netId;
            SpellSlot = spellSlot;
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            TargetNetId = targetNetId;
        }
    }
}
