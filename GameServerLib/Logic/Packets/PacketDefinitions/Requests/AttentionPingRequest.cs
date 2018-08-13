using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class AttentionPingRequest
    {
        public float X { get; }
        public float Y { get; }
        public int TargetNetId { get; }
        public Pings Type { get; }

        public AttentionPingRequest(float x, float y, int targetNetId, Pings type)
        {
            X = x;
            Y = y;
            TargetNetId = targetNetId;
            Type = type;
        }
    }
}
