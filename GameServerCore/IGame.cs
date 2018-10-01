using GameServerCore.Maps;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using System.Collections.Generic;
using System.Reflection;

namespace GameServerCore
{
    public interface IGame
    {
        IMap Map { get; }
        IObjectManager ObjectManager { get; }
        IPlayerManager PlayerManager { get; }
        bool IsRunning { get;}
        bool IsPaused { get; set; }

        bool SetToExit { get; set; }

        bool HandleDisconnect(int userId);
        Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllPacketHandlers();
    }
}
