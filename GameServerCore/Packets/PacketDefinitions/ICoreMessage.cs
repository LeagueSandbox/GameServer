using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Packets.PacketDefinitions
{
    /// <summary>
    /// Interface for all requests and responses. *NOTE*: Currently empty as both requests and responses are handled independently. Refer to NetworkHandler.
    /// </summary>
    public interface ICoreMessage
    {
        // all API requests & responses should implement this
    }
}
