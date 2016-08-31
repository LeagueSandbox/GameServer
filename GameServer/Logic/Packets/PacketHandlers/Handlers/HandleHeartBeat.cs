using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using Ninject;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleHeartBeat : IPacketHandler
    {
        private Logger _logger = Program.Kernel.Get<Logger>();

        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var heartbeat = new HeartBeat(data);

            float diff = heartbeat.ackTime - heartbeat.receiveTime;
            if (heartbeat.receiveTime > heartbeat.ackTime)
            {
                _logger.LogCoreWarning("Player " + game.GetPeerInfo(peer).UserId + " sent an invalid heartbeat - Timestamp error (diff: " + diff);
            }
            else
            {
              //  Logger.LogCoreInfo("Player %d sent heartbeat (diff: %.f)", peerInfo(peer)->userId, diff);
            }

            return true;
        }
    }
}
