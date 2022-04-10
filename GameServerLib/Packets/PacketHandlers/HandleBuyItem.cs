using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleBuyItem : PacketHandlerBase<BuyItemReq>
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

        public override bool HandlePacket(int userId, BuyItemReq req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemBuyRequest((int)req.ItemID);
        }
    }
}
