﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSurrender : PacketHandlerBase<EmptyClientPacket>
    {
        private readonly Game _game;
        private readonly PlayerManager _pm;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_Surrender;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSurrender(Game game, PlayerManager pm)
        {
            _game = game;
            _pm = pm;
        }

        public override bool HandlePacketInternal(Peer peer, EmptyClientPacket data)
        {
            var c = _pm.GetPeerInfo(peer).Champion;
            var surrender = new Surrender(c, 0x03, 1, 0, 5, c.Team, 10.0f);
            _game.PacketHandlerManager.broadcastPacketTeam(TeamId.TEAM_BLUE, surrender, Channel.CHL_S2C);
            return true;
        }
    }
}
