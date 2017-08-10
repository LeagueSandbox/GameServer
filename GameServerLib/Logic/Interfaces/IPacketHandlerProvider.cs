using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Interfaces
{
    public interface IPacketHandlerProvider
    {
        Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllHandlers(IEnumerable<Assembly> loadFrom);
    }
}
