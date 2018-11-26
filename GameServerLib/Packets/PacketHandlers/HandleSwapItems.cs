using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Items;
using GameServerCore.Packets.PacketDefinitions.Requests;


namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase<SwapItemsRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SWAP_ITEMS;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSwapItems(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SwapItemsRequest req)
        {
            if (req.SlotFrom > 6 || req.SlotTo > 6)
            {
                return false;
            }

            var champion = _playerManager.GetPeerInfo(userId).Champion;

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            champion.Inventory.SwapItems(req.SlotFrom, req.SlotTo);
            champion.SwapSpells((byte)(req.SlotFrom + Shop.ITEM_ACTIVE_OFFSET),
                (byte)(req.SlotTo + Shop.ITEM_ACTIVE_OFFSET));
            _game.PacketNotifier.NotifyItemsSwapped(
                champion,
                req.SlotFrom,
                req.SlotTo
            );

            return true;
        }
    }
}
