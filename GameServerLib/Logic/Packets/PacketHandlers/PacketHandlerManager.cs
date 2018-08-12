using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BlowFishCS;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Handlers;
using LeagueSandbox.GameServer.Logic.Players;

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

        internal IPacketHandler GetHandler(PacketCmd cmd, Channel channelID)
        {
            var game = Program.ResolveDependency<Game>();
            var packetsHandledWhilePaused = new List<PacketCmd>
            {
                PacketCmd.PKT_UnpauseGame,
                PacketCmd.PKT_C2S_CharLoaded,
                PacketCmd.PKT_C2S_Click,
                PacketCmd.PKT_C2S_ClientReady,
                PacketCmd.PKT_C2S_Exit,
                PacketCmd.PKT_C2S_HeartBeat,
                PacketCmd.PKT_C2S_QueryStatusReq,
                PacketCmd.PKT_C2S_StartGame,
                PacketCmd.PKT_C2S_World_SendGameNumber,
                PacketCmd.PKT_ChatBoxMessage,
                PacketCmd.PKT_KeyCheck
            };
            if (game.IsPaused && !packetsHandledWhilePaused.Contains(cmd))
            {
                return null;
            }
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey(channelID))
                    return handlers[channelID];
            }
            return null;
        }
        public bool sendPacket(Peer peer, GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return sendPacket(peer, packet.GetBytes(), channelNo, flag);
        }

        private IntPtr allocMemory(byte[] data)
        {
            var unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            return unmanagedPointer;
        }

        private void releaseMemory(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public void printPacket(byte[] buffer, string str)
        {
            //string hex = BitConverter.ToString(buffer);
            // System.Diagnostics.Debug.WriteLine(str + hex.Replace("-", " "));
            lock (Program.ExecutingDirectory)
            {
                System.Diagnostics.Debug.Write(str);
                foreach (var b in buffer)
                    System.Diagnostics.Debug.Write(b.ToString("X2") + " ");

                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("--------");
            }
        }
        public bool sendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
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

        public bool broadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
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

        public bool broadcastPacket(GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacket(packet.GetBytes(), channelNo, flag);
        }


        public bool broadcastPacketTeam(TeamId team, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in _playerManager.GetPlayers())
                if (ci.Item2.Peer != null && ci.Item2.Team == team)
                    sendPacket(ci.Item2.Peer, data, channelNo, flag);
            return true;
        }

        public bool broadcastPacketTeam(TeamId team, GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketTeam(team, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketVision(o, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            var game = Program.ResolveDependency<Game>();
            foreach (var team in _teamsEnumerator)
            {
                if (team == TeamId.TEAM_NEUTRAL)
                    continue;

                if (game.ObjectManager.TeamHasVisionOn(team, o))
                {
                    broadcastPacketTeam(team, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool handlePacket(Peer peer, byte[] data, Channel channelID)
        {
            var header = new GameServer.Logic.Packets.PacketHeader(data);
            var handler = GetHandler(header.cmd, channelID);

            switch (header.cmd)
            {
                case PacketCmd.PKT_C2S_StatsConfirm:
                case PacketCmd.PKT_C2S_MoveConfirm:
                case PacketCmd.PKT_C2S_ViewReq:
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
            printPacket(data, "Error: ");
            return false;
        }

        public bool handlePacket(Peer peer, ENet.Packet packet, Channel channelID)
        {
            var data = new byte[(int)packet.Length];
            Marshal.Copy(packet.Data, data, 0, data.Length);

            if (data.Length >= 8)
                if (_playerManager.GetPeerInfo(peer) != null)
                    data = _blowfish.Decrypt(data);

            return handlePacket(peer, data, channelID);
        }
    }
}
