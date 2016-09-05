using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleView : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new ViewRequest(data);
            var answer = new ViewAnswer(request);
            if (request.requestNo == 0xFE)
            {
                answer.setRequestNo(0xFF);
            }
            else
            {
                answer.setRequestNo(request.requestNo);
            }
            _game.PacketHandlerManager.sendPacket(peer, answer, Channel.CHL_S2C, PacketFlags.None);
            return true;
        }
    }
}
