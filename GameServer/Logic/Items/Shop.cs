﻿using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class Shop
    {
        private Champion _owner;
        private InventoryManager _inventory;

        private Shop(Champion owner)
        {
            _owner = owner;
            _inventory = _owner.Inventory;
        }

        //public bool ItemBuyRequest(int itemId)
        //{
        //    var item = Item.Instantiate(itemId);
        //    if (item == null) return false;

        //    var recipeParts = _inventory.GetAvailableRecipeParts(item);
        //    var price = item.TotalPrice;
        //    Item i;

        //    if (recipeParts.Length == 0)
        //    {
        //        if (_owner.getStats().getGold() < price)
        //        {
        //            return true;
        //        }

        //        i = _owner.getInventory().AddItem(item);

        //        if (i == null)
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        foreach (var instance in recipeParts)
        //            price -= instance.TotalPrice;

        //        if (_owner.getStats().getGold() < price)
        //            return false;


        //        foreach (var instance in recipeParts)
        //        {
        //            _owner.getStats().unapplyStatMods(instance.getTemplate().getStatMods());
        //            PacketNotifier.notifyRemoveItem(_owner, instance.getSlot(), 0);
        //            _owner.getInventory().RemoveItem(instance.getSlot());
        //        }

        //        i = game.getPeerInfo(peer).getChampion().getInventory().addItem(itemTemplate);
        //    }

        //    _owner.getStats().setGold(_owner.getStats().getGold() - price);
        //    _owner.getStats().applyStatMods(itemTemplate.getStatMods());
        //    PacketNotifier.notifyItemBought(game.getPeerInfo(peer).getChampion(), i);

        //    return true;
        //}

        public static Shop CreateShop(Champion owner)
        {
            return new Shop(owner);
        }
    }
}
