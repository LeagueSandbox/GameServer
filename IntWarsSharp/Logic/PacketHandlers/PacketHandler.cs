using ENet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Core.Logic
{
    public interface IPacketHandler
    {
        unsafe bool HandlePacket(ENetPeer* peer, ENetPacket* packet);
    }
}
