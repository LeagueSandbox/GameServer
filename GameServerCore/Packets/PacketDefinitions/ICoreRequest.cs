using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Packets.PacketDefinitions
{
    /// <summary>
    /// Interface for packet requests. *NOTE*: Currently empty however refer to Game, PacketHandlerManager, and NetworkHandler for usage.
    /// </summary>
    public interface ICoreRequest : ICoreMessage
    {
    }
}
