﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SwapItems;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSwapItems(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new SwapItemsRequest(data);
            if (request.slotFrom > 6 || request.slotTo > 6)
                return false;

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            _playerManager.GetPeerInfo(peer).Champion.Inventory.SwapItems(request.slotFrom, request.slotTo);
            _game.PacketNotifier.NotifyItemsSwapped(
                _playerManager.GetPeerInfo(peer).Champion,
                request.slotFrom,
                request.slotTo
            );

            return true;
        }
    }
}
