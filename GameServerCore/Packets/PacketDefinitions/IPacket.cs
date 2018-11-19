using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Packets.PacketDefinitions
{
    // all packets in PacketDefinitions should implement this
    public interface IPacket
    {
        // for requests
        ICoreMessage Read(byte[] data);
        // for responses
        void Write(ICoreMessage msg);
    }
}
