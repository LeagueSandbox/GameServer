using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Inventory;


namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSwapItems : PacketHandlerBase<SwapItemsRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleSwapItems(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SwapItemsRequest req)
        {
            if (req.Source > 6 || req.Destination > 6)
            {
                return false;
            }

            var champion = _playerManager.GetPeerInfo(userId).Champion;

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            champion.Inventory.SwapItems(req.Source, req.Destination);
            _game.PacketNotifier.NotifySwapItemAns(champion, req.Source, req.Destination);
            champion.SwapSpells((byte)(req.Source + Shop.ITEM_ACTIVE_OFFSET),
                (byte)(req.Destination + Shop.ITEM_ACTIVE_OFFSET));
            return true;
        }
    }
}
