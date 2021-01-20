using ENet;
using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.Interfaces;
using GameServerCore.Packets.PacketDefinitions;
using PacketDefinitions420.Exceptions;
using System;
using System.Collections.Generic;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class responsible for the server's networking of the game.
    /// </summary>
    public class PacketServer
    {
        private Host _server;
        private uint _serverHost;
        private IGame _game;
        protected const int PEER_MTU = 996;

        /// <summary>
        /// Networking encryption method.
        /// </summary>
        public BlowFish Blowfish { get; private set; }
        /// <summary>
        /// Interface of all functions related to sending and receiving packets.
        /// </summary>
        public IPacketHandlerManager PacketHandlerManager { get; private set; }

        /// <summary>
        /// Creates and sets up the host for the server as well as the packet handler manager.
        /// </summary>
        /// <param name="ipaddr">Port the server will be hosted on.</param>
        /// <param name="port">Port the server will be hosted on.</param>
        /// <param name="blowfishKeys">Unique blowfish keys the server will use for encryption for each player.</param>
        /// <param name="game">Game instance.</param>
        /// <param name="netReq">Network request handler instance.</param>
        /// <param name="netResp">Network response handler instance.</param>
        public void InitServer(string ipaddr, ushort port, Dictionary<ulong, string> blowfishKeys, IGame game, NetworkHandler<ICoreRequest> netReq, NetworkHandler<ICoreResponse> netResp)
        {

            //Convert string IP to uint
            string[] octets = ipaddr.Split('.');
            _serverHost = (uint)(((short.Parse(octets[0]) << 24) | (short.Parse(octets[1]) << 16) | (short.Parse(octets[2]) << 8) | short.Parse(octets[3])) & 0xffffffffL);

            _game = game;
            _server = new Host();
            _server.Create(new Address(_serverHost,port), 32, 32, 0, 0);

            Dictionary<ulong, BlowFish> blowfishes = new Dictionary<ulong, BlowFish>();
            foreach(var rawKey in blowfishKeys)
            {
                var key = Convert.FromBase64String(rawKey.Value);
                if (key.Length <= 0)
                {
                    throw new InvalidKeyException($"Invalid blowfish key supplied ({key})");
                }
                blowfishes.Add(rawKey.Key, new BlowFish(key));
            }

            PacketHandlerManager = new PacketHandlerManager(blowfishes, _server, game, netReq, netResp);
            
        }

        /// <summary>
        /// The core networking loop which fires for connections, received packets, and disconnects.
        /// </summary>
        public void NetLoop()
        {
            while (_server.Service(0, out var enetEvent) > 0)
            {
                switch (enetEvent.Type)
                {
                    case EventType.Connect:
                        {
                            // Set some defaults
                            enetEvent.Peer.Mtu = PEER_MTU;
                            enetEvent.Data = 0;
                        }
                        break;
                    case EventType.Receive:
                        {
                            var channel = (Channel)enetEvent.ChannelID;
                            PacketHandlerManager.HandlePacket(enetEvent.Peer, enetEvent.Packet, channel);
                            // Clean up the packet now that we're done using it.
                            enetEvent.Packet.Dispose();
                        }
                        break;
                    case EventType.Disconnect:
                        {
                            PacketHandlerManager.HandleDisconnect(enetEvent.Peer);
                        }
                        break;
                }
            }
        }
    }
}
