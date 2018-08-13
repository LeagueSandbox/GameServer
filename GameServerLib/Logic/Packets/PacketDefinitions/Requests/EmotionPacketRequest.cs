using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.Enums;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class EmotionPacketRequest
    {
        public uint NetId;
        public Emotions Id;

        public EmotionPacketRequest(uint netId, Emotions id)
        {
            NetId = netId;
            Id = id;
        }
    }
}
