using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct FogUpdate2Args
    {
        public UnitAtLocation Unit { get; }
        public TeamId UnitTeam { get; }
        public float UnitVisionRadius { get; }
        public uint FogNetId { get; }

        public FogUpdate2Args(UnitAtLocation unit, TeamId unitTeam, float unitVisionRadius, uint fogNetId)
        {
            Unit = unit;
            UnitTeam = unitTeam;
            UnitVisionRadius = unitVisionRadius;
            FogNetId = fogNetId;
        }
    }
}
