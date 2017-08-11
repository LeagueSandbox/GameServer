using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServer.Logic.Interfaces
{
    public interface IHandlerProvider
    {
        Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllPacketHandlers(IEnumerable<Assembly> loadFrom);
        SortedDictionary<string, IChatCommand> GetAllChatCommandHandlers(IEnumerable<Assembly> loadFrom);
    }
}
