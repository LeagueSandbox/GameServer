using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleBuyItem : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BUY_ITEM_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleBuyItem(Game game)
        {
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadBuyItemRequest(data);
            var champion = (Champion)_playerManager.GetPeerInfo(userId).Champion;
            return champion.Shop.HandleItemBuyRequest(request.ItemId);
        }
    }
}
