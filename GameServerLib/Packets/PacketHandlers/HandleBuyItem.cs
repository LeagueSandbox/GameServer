using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleBuyItem : PacketHandlerBase<BuyItemRequest>
    {
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;

        public HandleBuyItem(Game game)
        {
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, BuyItemRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemBuyRequest((int)req.ItemID);
        }
    }
}
