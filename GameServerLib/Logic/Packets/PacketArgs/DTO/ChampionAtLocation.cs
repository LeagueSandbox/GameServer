using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO
{
    public struct ChampionAtLocation
    {
        public uint UnitNetId { get; }
        public int UnitHash { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public ChampionAtLocation(uint unitNetId, int unitHash, float x, float y, float z)
        {
            UnitNetId = unitNetId;
            UnitHash = unitHash;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
