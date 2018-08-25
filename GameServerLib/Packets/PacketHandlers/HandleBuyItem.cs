using ENet;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleBuyItem : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BUY_ITEM_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleBuyItem(Game game)
        {
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadBuyItemRequest(data);
            var champion = (Champion)_playerManager.GetPeerInfo(peer).Champion;
            return champion.Shop.ItemBuyRequest(request.ItemId);
        }
    }
}
