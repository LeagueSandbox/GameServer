using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleBuyItem : PacketHandlerBase<BuyItemRequest>
    {
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly IPlayerManager _playerManager;

        public HandleBuyItem(Game game)
        {
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, BuyItemRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemBuyRequest(req.ItemId);
        }
    }
}
