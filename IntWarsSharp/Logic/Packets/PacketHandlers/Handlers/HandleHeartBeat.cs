using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using IntWarsSharp.Logic.Packets;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleHeartBeat : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var heartbeat = new HeartBeat(data);

            float diff = heartbeat.ackTime - heartbeat.receiveTime;
            if (heartbeat.receiveTime > heartbeat.ackTime)
            {
                Logger.LogCoreWarning("Player " + game.peerInfo(peer).userId + " sent an invalid heartbeat - Timestamp error (diff: " + diff);
            }
            else
            {
              //  Logger.LogCoreInfo("Player %d sent heartbeat (diff: %.f)", peerInfo(peer)->userId, diff);
            }

            return true;
        }
    }
}
