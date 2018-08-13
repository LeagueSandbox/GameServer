using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class HeartbeatRequest
    {
        public int NetId { get; }
        public float ReceiveTime { get; }
        public float AckTime { get; }

        public HeartbeatRequest(int netId, float receiveTime, float ackTime)
        {
            NetId = netId;
            ReceiveTime = receiveTime;
            AckTime = ackTime;
        }
    }
}
