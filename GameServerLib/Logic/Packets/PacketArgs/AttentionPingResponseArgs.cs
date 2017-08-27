using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AttentionPingResponseArgs
    {
        public uint PlayerChampionNetId { get; }
        public float X { get; }
        public float Y { get; }
        public uint TargetNetId { get; }
        public Pings Type { get; }

        public AttentionPingResponseArgs(uint playerChampionNetId, float x, float y, uint targetNetId, Pings type)
        {
            PlayerChampionNetId = playerChampionNetId;
            X = x;
            Y = y;
            TargetNetId = targetNetId;
            Type = type;
        }
    }
}
