using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class Inventory
    {
        private const int TRINKET_SLOT = 6;
        private ItemInstance[] _items;

        public Inventory()
        {
            _items = new ItemInstance[7];
        }

        public ItemInstance AddItem(ItemInstance item)
        {
            if(item.IsTrinket) return AddTrinketItem(item);
            if (item.MaximumStackSize > 1) return AddStackingItem(item);
            return AddNewItem(item);
        }

        public ItemInstance SetItem(int slot, ItemInstance item)
        {
            if (slot > _items.Length) return null;
            _items[slot] = item;
            return item;
        }

        public ItemInstance GetItem(int slot)
        {
            return _items[slot];
        }

        public void RemoveItem(int slot)
        {
            _items[slot] = null;
        }

        public void SwapItems(int slot1, int slot2)
        {
            var buffer = _items[slot1];
            _items[slot1] = _items[slot2];
            _items[slot2] = buffer;
        }

        private ItemInstance AddTrinketItem(ItemInstance item)
        {
            if (_items[TRINKET_SLOT] != null) return null;
            return SetItem(TRINKET_SLOT, item);
        }

        private ItemInstance AddStackingItem(ItemInstance item)
        {
            for(var i = 0; i < _items.Length; i++)
            {
                if (_items[i] == null) continue;
                if (item.Id != _items[i].Id) continue;
                return _items[i].IncrementCount();
            }
            return AddNewItem(item);
        }

        private ItemInstance AddNewItem(ItemInstance item)
        {
            for(var i = 0; i < _items.Length; i++)
            {
                if (i == TRINKET_SLOT) continue;
                if (_items[i] != null) continue;
                return SetItem(i, item);
            }
            return null;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Shitty methods pasted for compatibility                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private List<ItemInstance> _getAvailableRecipeParts(ItemTemplate recipe)
        {
            var toReturn = new List<ItemInstance>();

            foreach (var i in _items)
            {
                if (i == null)
                    continue;

                if (i.getTemplate().getId() == recipe.getId() && !i.getRecipeSearchFlag())
                {
                    toReturn.Add(i);
                    i.setRecipeSearchFlag(true);
                    return toReturn;
                }
            }

            foreach (var itemId in recipe.getRecipeParts())
            {
                var parts = _getAvailableRecipeParts(ItemManager.getInstance().getItemTemplateById(itemId));
                toReturn.AddRange(parts);
            }

            return toReturn;
        }
        public List<ItemInstance> getAvailableRecipeParts(ItemTemplate recipe)
        {
            var toReturn = new List<ItemInstance>();

            foreach (var itemId in recipe.getRecipeParts())
            {
                var item = ItemManager.getInstance().getItemTemplateById(itemId);
                if (item == null)
                    continue;

                var parts = _getAvailableRecipeParts(item);
                toReturn.AddRange(parts);
            }

            foreach (var i in _items)
                if (i != null)
                    i.setRecipeSearchFlag(false);

            return toReturn;
        }
        public ItemInstance addItem(ItemTemplate itemTemplate)
        {
            var slot = -1;

            if (itemTemplate.isTrinket())
            {
                if (_items[6] == null)
                {
                    _items[6] = new ItemInstance(itemTemplate, 6, 1);
                    return _items[6];
                }
                return null;
            }

            if (itemTemplate.getMaxStack() > 1)
            {
                for (slot = 0; slot < 6; ++slot)
                {
                    if (_items[slot] == null)
                        continue;

                    if (_items[slot].getTemplate() == itemTemplate)
                    {
                        if (_items[slot].getStacks() < itemTemplate.getMaxStack())
                        {
                            _items[slot].incrementStacks();
                            return _items[slot];
                        }
                        else if (_items[slot].getStacks() == itemTemplate.getMaxStack())
                        {
                            return null;
                        }
                    }
                }
            }

            for (slot = 0; slot < 6; ++slot)
            {
                if (_items[slot] == null)
                    break;
            }

            if (slot == 6)
            { // Inventory full
                return null;
            }

            _items[slot] = new ItemInstance(itemTemplate, (byte)slot, 1);

            return _items[slot];
        }
    }

    //public class Item
    //{
    //    public bool IsTrinket { get; private set; }
    //    public int MaximumStackSize { get; private set; }
    //    public int StackSize { get; private set; }
    //    public int Id { get; private set; }
    //    public int TotalPrice { get; private set; }

    //    private Item()
    //    {
    //        var inventory = new Inventory();
    //    }

    //    public Item IncrementCount()
    //    {
    //        if (StackSize >= MaximumStackSize) return null;
    //        StackSize++;
    //        return this;
    //    }

    //    public StatMod[] GetStatMods()
    //    {
    //        throw new Exception("Gtfo");
    //    }

    //    public static Item Instantiate(int id)
    //    {
    //        return new Item();
    //    }
    //}
}
