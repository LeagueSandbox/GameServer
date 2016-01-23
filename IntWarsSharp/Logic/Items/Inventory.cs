using IntWarsSharp.Core.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.Items
{
    public class Inventory
    {
        private List<ItemInstance> items;
        public Inventory()
        {
            items = new List<ItemInstance>() { null, null, null, null, null, null, null };
        }

        private List<ItemInstance> _getAvailableRecipeParts(ItemTemplate recipe)
        {
            var toReturn = new List<ItemInstance>();

            foreach (var i in items)
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
        public ItemInstance addItem(ItemTemplate itemTemplate)
        {
            var slot = -1;

            if (itemTemplate.isTrinket())
            {
                if (items[6] == null)
                {
                    items[6] = new ItemInstance(itemTemplate, 6, 1);
                    return items[6];
                }
                return null;
            }

            if (itemTemplate.getMaxStack() > 1)
            {
                for (slot = 0; slot < 6; ++slot)
                {
                    if (items[slot] == null)
                        continue;

                    if (items[slot].getTemplate() == itemTemplate)
                    {
                        if (items[slot].getStacks() < itemTemplate.getMaxStack())
                        {
                            items[slot].incrementStacks();
                            return items[slot];
                        }
                        else if (items[slot].getStacks() == itemTemplate.getMaxStack())
                        {
                            return null;
                        }
                    }
                }
            }

            for (slot = 0; slot < 6; ++slot)
            {
                if (items[slot] == null)
                    break;
            }

            if (slot == 6)
            { // Inventory full
                return null;
            }

            Logger.LogCoreInfo("Adding item " + itemTemplate.getId() + " to slot " + slot);
            items[slot] = new ItemInstance(itemTemplate, (byte)slot, 1);

            return items[slot];
        }
        public void swapItems(byte slotFrom, byte slotTo)
        {
            var to = items[slotTo];
            items[slotTo] = items[slotFrom];
            items[slotFrom] = to;
        }
        public List<ItemInstance> getItems()
        {
            return items;
        }
        public void removeItem(byte slot)
        {
            if (items[slot] == null)
                return;

            items.RemoveAt(slot);
        }
        public ItemInstance getItemSlot(byte slot)
        {
            if (items[slot] == null)
                return null;

            return items[slot];
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

            foreach (var i in items)
                if (i != null)
                    i.setRecipeSearchFlag(false);

            return toReturn;
        }

    }
}
