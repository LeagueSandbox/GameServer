using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleClick : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var click = new Click(data);
            Logger.LogCoreInfo("Object " + game.getPeerInfo(peer).getChampion().getNetId() + " clicked on " + click.targetNetId);

            return true;
        }
    }
}
