using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBuyItem : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private ItemManager _itemManager = Program.ResolveDependency<ItemManager>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new BuyItemReq(data);

            var itemTemplate = _itemManager.SafeGetItemType(request.id);
            if (itemTemplate == null)
                return false;

            var recipeParts = _playerManager.GetPeerInfo(peer).Champion
                .getInventory()
                .GetAvailableItems(itemTemplate.Recipe);
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
                    _playerManager.GetPeerInfo(peer).Champion.GetStats().RemoveBuff(instance.ItemType);
                    var champion = _playerManager.GetPeerInfo(peer).Champion;
                    var inventory = champion.getInventory();
                    _game.PacketNotifier.notifyRemoveItem(champion, inventory.GetItemSlot(instance), 0);
                    inventory.RemoveItem(instance);
                }

                i = _playerManager.GetPeerInfo(peer).Champion.getInventory().AddItem(itemTemplate);
            }

            _playerManager.GetPeerInfo(peer).Champion.GetStats().Gold =
                _playerManager.GetPeerInfo(peer).Champion.GetStats().Gold - price;
            _playerManager.GetPeerInfo(peer).Champion.GetStats().AddBuff(itemTemplate);
            _game.PacketNotifier.notifyItemBought(_playerManager.GetPeerInfo(peer).Champion, i);

            return true;
        }
    }
}
