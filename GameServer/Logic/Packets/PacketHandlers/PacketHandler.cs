using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public interface IPacketHandler
    {
        bool HandlePacket(Peer peer, byte[] data);
    }
}
