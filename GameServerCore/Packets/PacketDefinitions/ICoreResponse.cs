using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Packets.PacketDefinitions
{
    /// <summary>
    /// Interface for packet responses. *NOTE*: Currently empty as responses are handled independently.
    /// Refer to GameServerCore.Packets.PacketDefinitions.Responses for response definitions. Unused compared to ICoreRequest.
    /// </summary>
    public interface ICoreResponse : ICoreMessage
    {
    }
}
