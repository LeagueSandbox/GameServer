using LENet;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using System;
using Channel = GameServerCore.Packets.Enums.Channel;
using Version = LENet.Version;
using LeagueSandbox.GameServer;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class responsible for the server's networking of the game.
    /// </summary>
    public class PacketServer
    {
        private Host _server;
        private readonly uint _serverHost = Address.Any;
        private Game _game;
        protected const int PEER_MTU = 996;

        /// <summary>
        /// Networking encryption method.
        /// </summary>
        public BlowFish Blowfish { get; private set; }
        /// <summary>
        /// Interface of all functions related to sending and receiving packets.
        /// </summary>
        public PacketHandlerManager PacketHandlerManager { get; private set; }

        /// <summary>
        /// Creates and sets up the host for the server as well as the packet handler manager.
        /// </summary>
        /// <param name="port">Port the server will be hosted on.</param>
        /// <param name="blowfishKeys">Unique blowfish keys the server will use for encryption for each player.</param>
        /// <param name="game">Game instance.</param>
        /// <param name="netReq">Network request handler instance.</param>
        /// <param name="netResp">Network response handler instance.</param>
        public void InitServer(ushort port, string[] blowfishKeys, Game game, NetworkHandler<ICoreRequest> netReq, NetworkHandler<ICoreRequest> netResp)
        {
            _game = game;
            _server = new Host(Version.Patch420, new Address(_serverHost, port), 32, 32, 0, 0);

            BlowFish[] blowfishes = new BlowFish[blowfishKeys.Length];
            for(int i = 0; i < blowfishKeys.Length; i++)
            {
                var key = Convert.FromBase64String(blowfishKeys[i]);
                if (key.Length <= 0)
                {
                    throw new Exception($"Invalid blowfish key supplied({ key })");
                }
                blowfishes[i] = new BlowFish(key);
            }

            PacketHandlerManager = new PacketHandlerManager(blowfishes, _server, game, netReq, netResp);
        }

        /// <summary>
        /// The core networking loop which fires for connections, received packets, and disconnects.
        /// </summary>
        public void NetLoop(uint timeout = 0)
        {
            var enetEvent = new Event();
            while (_server.HostService(enetEvent, timeout) > 0)
            {
                switch (enetEvent.Type)
                {
                    case EventType.CONNECT:
                        {
                            // Set some defaults
                            enetEvent.Peer.MTU = PEER_MTU;
                            enetEvent.Data = 0;
                        }
                        break;
                    case EventType.RECEIVE:
                        {
                            var channel = (Channel)enetEvent.ChannelID;
                            PacketHandlerManager.HandlePacket(enetEvent.Peer, enetEvent.Packet, channel);
                            // Clean up the packet now that we're done using it.
                            //enetEvent.Packet.Dispose();
                        }
                        break;
                    case EventType.DISCONNECT:
                        {
                            PacketHandlerManager.HandleDisconnect(enetEvent.Peer);
                        }
                        break;
                }
            }
        }
    }
}
