using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct HeroSpawn2Args
    {
        public UnitAtLocation Champion { get; }

        public HeroSpawn2Args(UnitAtLocation champion)
        {
            Champion = champion;
        }
    }
}
