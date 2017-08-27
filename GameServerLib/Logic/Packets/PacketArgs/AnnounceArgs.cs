using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AnnounceArgs
    {
        public Announces Type { get; }
        public int MapId { get; }

        public AnnounceArgs(Announces type) : this()
        {
            Type = type;
        }

        public AnnounceArgs(Announces type, int mapId)
        {
            Type = type;
            MapId = mapId;
        }
    }
}
