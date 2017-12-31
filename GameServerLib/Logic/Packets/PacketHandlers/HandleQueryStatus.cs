﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleQueryStatus : PacketHandlerBase<EmptyClientPacket>
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QueryStatusReq;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQueryStatus(Game game)
        {
            _game = game;
        }

        public override bool HandlePacketInternal(Peer peer, EmptyClientPacket data)
        {
            var response = new QueryStatus();
            return _game.PacketHandlerManager.sendPacket(peer, response, Channel.CHL_S2C);
        }
    }
}
