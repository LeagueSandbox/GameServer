using ENet;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Items;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SWAP_ITEMS;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSwapItems(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadSwapItemsRequest(data);
            if (request.SlotFrom > 6 || request.SlotTo > 6)
            {
                return false;
            }

            var champion = _playerManager.GetPeerInfo(peer).Champion;

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            champion.Inventory.SwapItems(request.SlotFrom, request.SlotTo);
            champion.SwapSpells((byte)(request.SlotFrom + Shop.ITEM_ACTIVE_OFFSET),
                (byte)(request.SlotTo + Shop.ITEM_ACTIVE_OFFSET));
            _game.PacketNotifier.NotifyItemsSwapped(
                champion,
                request.SlotFrom,
                request.SlotTo
            );

            return true;
        }
    }
}
