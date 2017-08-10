using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleMoveConfirm : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_MoveConfirm;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            return true;
        }
    }
}
