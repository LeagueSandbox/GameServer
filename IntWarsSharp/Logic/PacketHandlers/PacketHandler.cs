using ENet.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Core.Logic
{
    public interface PacketHandler
    {
        void HandlePacket(ENetPeer peer, ENetPacket packet);
    }
}
