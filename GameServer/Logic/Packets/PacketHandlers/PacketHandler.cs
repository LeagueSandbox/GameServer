using static ENet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public interface IPacketHandler
    {
        unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game);
    }
}
