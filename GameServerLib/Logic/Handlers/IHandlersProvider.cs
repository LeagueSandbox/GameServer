using System.Collections.Generic;
using System.Reflection;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Handlers
{
    public interface IHandlersProvider
    {
        Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllPacketHandlers(IEnumerable<Assembly> loadFrom);
        SortedDictionary<string, IChatCommand> GetAllChatCommandHandlers(IEnumerable<Assembly> loadFrom);
    }
}
