using GameServerCore.Domain;
using GameServerCore.Handlers;
using GameServerCore.Packets.Interfaces;

namespace GameServerCore
{
    /// <summary>
    /// Class that contains and manages all qualities of the game such as managers for networking and game mechanics as well as the starting, pausing, and stopping of the game.
    /// </summary>
    public interface IGame: IUpdate
    {
        /// <summary>
        /// Whether the server is running or not. Usually true after the network loop has started via GameServerLauncher.
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// Whether or not the game has been paused (via a chat command usually).
        /// </summary>
        bool IsPaused { get; set; }
        /// <summary>
        /// Whether or not the game is set as finished (and thus whether the server should close).
        /// </summary>
        bool SetToExit { get; set; }

        /// <summary>
        /// Contains all map related game settings such as collision handler, navigation grid, announcer events, and map properties. Doubles as a Handler/Manager for all MapScripts.
        /// </summary>
        IMapScriptHandler Map { get; }
        /// <summary>
        /// Interface containing all (public) functions used by ObjectManager. ObjectManager manages GameObjects, their properties, and their interactions such as being added, removed, colliding with other objects or terrain, vision, teams, etc.
        /// </summary>
        IObjectManager ObjectManager { get; }
        /// <summary>
        /// Interface of functions used to identify players or their properties (such as their champion).
        /// </summary>
        IPlayerManager PlayerManager { get; }
        /// <summary>
        /// Interface of functions containing value assignments for packets sent from the server to clients.
        /// </summary>
        IPacketNotifier PacketNotifier { get; }
    }
}
