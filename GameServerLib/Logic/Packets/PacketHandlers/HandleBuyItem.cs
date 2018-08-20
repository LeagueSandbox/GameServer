﻿using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
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
            var itemTemplate = _itemManager.SafeGetItemType(request.ItemId);
            if (itemTemplate == null)
            {
                return false;
            }

            var champion = _playerManager.GetPeerInfo(peer).Champion;
            var stats = champion.Stats;
            var inventory = (InventoryManager)champion.Inventory;
            var recipeParts = inventory.GetAvailableItems(itemTemplate.Recipe);
            var price = itemTemplate.TotalPrice;
            Item i;

            if (recipeParts.Count == 0)
            {
                if (stats.Gold < price)
                {
                    return true;
                }

                i = inventory.AddItem(itemTemplate);

                if (i == null)
                {
                    // Slots full
                    return false;
                }
            }
            else
            {
                foreach (var instance in recipeParts)
                {
                    price -= instance.ItemType.TotalPrice;
                }

                if (stats.Gold < price)
                {
                    return false;
                }

                foreach (var instance in recipeParts)
                {
                    stats.RemoveModifier(instance.ItemType);
                    _game.PacketNotifier.NotifyRemoveItem(champion, inventory.GetItemSlot(instance), 0);
                    inventory.RemoveItem(instance);
                }

                i = inventory.AddItem(itemTemplate);
            }

            stats.Gold -= price;
            stats.AddModifier(itemTemplate);
            _game.PacketNotifier.NotifyItemBought(champion, i);

            return true;
        }
    }
}
