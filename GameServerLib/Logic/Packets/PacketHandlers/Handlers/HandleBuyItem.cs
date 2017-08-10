using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleBuyItem : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BuyItemReq;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleBuyItem(Game game, ItemManager itemManager, PlayerManager playerManager)
        {
            _game = game;
            _itemManager = itemManager;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new BuyItemReq(data);

            var itemTemplate = _itemManager.SafeGetItemType(request.id);
            if (itemTemplate == null)
                return false;

            var recipeParts = _playerManager.GetPeerInfo(peer).Champion.getInventory().GetAvailableItems(itemTemplate.Recipe);
            var price = itemTemplate.TotalPrice;
            Item i;

            if (recipeParts.Count == 0)
            {
                if (_playerManager.GetPeerInfo(peer).Champion.GetStats().Gold < price)
                {
                    return true;
                }

                i = _playerManager.GetPeerInfo(peer).Champion.getInventory().AddItem(itemTemplate);

                if (i == null)
                { // Slots full
                    return false;
                }
            }
            else
            {
                foreach (var instance in recipeParts)
                    price -= instance.ItemType.TotalPrice;

                if (_playerManager.GetPeerInfo(peer).Champion.GetStats().Gold < price)
                    return false;


                foreach (var instance in recipeParts)
                {
                    _playerManager.GetPeerInfo(peer).Champion.GetStats().RemoveModifier(instance.ItemType);
                    var champion = _playerManager.GetPeerInfo(peer).Champion;
                    var inventory = champion.getInventory();
                    _game.PacketNotifier.NotifyRemoveItem(champion, inventory.GetItemSlot(instance), 0);
                    inventory.RemoveItem(instance);
                }

                i = _playerManager.GetPeerInfo(peer).Champion.getInventory().AddItem(itemTemplate);
            }

            _playerManager.GetPeerInfo(peer).Champion.GetStats().Gold = _playerManager.GetPeerInfo(peer).Champion.GetStats().Gold - price;
            _playerManager.GetPeerInfo(peer).Champion.GetStats().AddModifier(itemTemplate);
            _game.PacketNotifier.NotifyItemBought(_playerManager.GetPeerInfo(peer).Champion, i);

            return true;
        }
    }
}
