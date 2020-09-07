using GameServerCore.Maps;
using GameServerCore.Domain;
using GameServerCore.Packets.Interfaces;

namespace GameServerCore
{
    public interface IGame: IUpdate
    {
        IMap Map { get; }
        IObjectManager ObjectManager { get; }
        IPlayerManager PlayerManager { get; }
        IPacketNotifier PacketNotifier { get; }
        bool IsRunning { get;}
        bool IsPaused { get; set; }

        bool SetToExit { get; set; }
    }
}
