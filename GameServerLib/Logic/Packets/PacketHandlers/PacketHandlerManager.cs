using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Handlers;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions;
using LeagueSandbox.GameServer.Logic.Players;
using Packet = LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Packet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class PacketHandlerManager
    {
        private readonly Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> _handlerTable;
        private readonly List<TeamId> _teamsEnumerator;
        private readonly Logger _logger;
        private readonly BlowFish _blowfish;
        private readonly Host _server;
        private readonly PlayerManager _playerManager;
        private readonly IHandlersProvider _packetHandlerProvider;

        public PacketHandlerManager(Logger logger, BlowFish blowfish, Host server, PlayerManager playerManager,
            IHandlersProvider handlersProvider)
        {
            _logger = logger;
            _blowfish = blowfish;
            _server = server;
            _playerManager = playerManager;
            _packetHandlerProvider = handlersProvider;
            _teamsEnumerator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            var loadFrom = new[] { ServerLibAssemblyDefiningType.Assembly };
            _handlerTable = _packetHandlerProvider.GetAllPacketHandlers(loadFrom);
        }

        internal IPacketHandler GetHandler(PacketCmd cmd, Channel channelId)
        {
            var game = Program.ResolveDependency<Game>();
            var packetsHandledWhilePaused = new List<PacketCmd>
            {
                PacketCmd.PKT_UNPAUSE_GAME,
                PacketCmd.PKT_C2_S_CHAR_LOADED,
                PacketCmd.PKT_C2_S_CLICK,
                PacketCmd.PKT_C2_S_CLIENT_READY,
                PacketCmd.PKT_C2_S_EXIT,
                PacketCmd.PKT_C2_S_HEART_BEAT,
                PacketCmd.PKT_C2_S_QUERY_STATUS_REQ,
                PacketCmd.PKT_C2_S_START_GAME,
                PacketCmd.PKT_C2_S_WORLD_SEND_GAME_NUMBER,
                PacketCmd.PKT_CHAT_BOX_MESSAGE,
                PacketCmd.PKT_KEY_CHECK
            };
            if (game.IsPaused && !packetsHandledWhilePaused.Contains(cmd))
            {
                return null;
            }
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey(channelId))
                    return handlers[channelId];
            }
            return null;
        }
        public bool SendPacket(Peer peer, Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return SendPacket(peer, packet.GetBytes(), channelNo, flag);
        }

        private IntPtr AllocMemory(byte[] data)
        {
            var unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            return unmanagedPointer;
        }

        private void ReleaseMemory(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public void PrintPacket(byte[] buffer, string str)
        {
            //string hex = BitConverter.ToString(buffer);
            // System.Diagnostics.Debug.WriteLine(str + hex.Replace("-", " "));
            lock (Program.ExecutingDirectory)
            {
                Debug.Write(str);
                foreach (var b in buffer)
                    Debug.Write(b.ToString("X2") + " ");

                Debug.WriteLine("");
                Debug.WriteLine("--------");
            }
        }
        public bool SendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Sending packet:\n");
            //if(length < 300)
            //printPacket(source, "Sent: ");
            byte[] temp;
            if (source.Length >= 8)
                temp = _blowfish.Encrypt(source);
            else
                temp = source;

            return peer.Send((byte)channelNo, temp);
        }

        public bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Broadcast packet:\n");
            //printPacket(data, "Broadcast: ");
            byte[] temp;
            if (data.Length >= 8)
                temp = _blowfish.Encrypt(data);
            else
                temp = data;

            var packet = new ENet.Packet();
            packet.Create(temp);
            _server.Broadcast((byte)channelNo, ref packet);
            return true;
        }

        public bool BroadcastPacket(Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return BroadcastPacket(packet.GetBytes(), channelNo, flag);
        }


        public bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in _playerManager.GetPlayers())
                if (ci.Item2.Peer != null && ci.Item2.Team == team)
                    SendPacket(ci.Item2.Peer, data, channelNo, flag);
            return true;
        }

        public bool BroadcastPacketTeam(TeamId team, Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return BroadcastPacketTeam(team, packet.GetBytes(), channelNo, flag);
        }

        public bool BroadcastPacketVision(GameObject o, Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return BroadcastPacketVision(o, packet.GetBytes(), channelNo, flag);
        }

        public bool BroadcastPacketVision(GameObject o, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            var game = Program.ResolveDependency<Game>();
            foreach (var team in _teamsEnumerator)
            {
                if (team == TeamId.TEAM_NEUTRAL)
                    continue;

                if (game.ObjectManager.TeamHasVisionOn(team, o))
                {
                    BroadcastPacketTeam(team, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool HandlePacket(Peer peer, byte[] data, Channel channelId)
        {
            var header = new PacketHeader(data);
            var handler = GetHandler(header.Cmd, channelId);

            switch (header.Cmd)
            {
                case PacketCmd.PKT_C2_S_STATS_CONFIRM:
                case PacketCmd.PKT_C2_S_MOVE_CONFIRM:
                case PacketCmd.PKT_C2_S_VIEW_REQ:
                    break;
            }

            if (handler != null)
            {
                if (!handler.HandlePacket(peer, data))
                {
                    return false;
                }

                return true;
            }
            PrintPacket(data, "Error: ");
            return false;
        }

        public bool HandlePacket(Peer peer, ENet.Packet packet, Channel channelId)
        {
            var data = new byte[packet.Length];
            Marshal.Copy(packet.Data, data, 0, data.Length);

            if (data.Length >= 8)
                if (_playerManager.GetPeerInfo(peer) != null)
                    data = _blowfish.Decrypt(data);

            return HandlePacket(peer, data, channelId);
        }
    }
}
