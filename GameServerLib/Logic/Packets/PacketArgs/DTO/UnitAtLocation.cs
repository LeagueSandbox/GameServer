using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO
{
    public struct UnitAtLocation
    {
        public uint UnitNetId { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public UnitAtLocation(uint unitNetId, float x, float y, float z)
        {
            UnitNetId = unitNetId;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
