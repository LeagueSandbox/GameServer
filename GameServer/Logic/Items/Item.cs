using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class ItemTemplate
    {
        private int id;
        private int maxStack;
        private int price;
        private bool trinket;
        private float sellBackModifier;
        private List<StatMod> statMods;

        /**
         * Necessary item ids to create this one.
         */
        private List<int> recipes;

        public ItemTemplate(int id, int maxStack, int price, float sellBackModifier, bool trinket, List<StatMod> statMods, List<int> recipes)
        {
            this.id = id;
            this.maxStack = maxStack;
            this.price = price;
            this.sellBackModifier = sellBackModifier;
            this.trinket = trinket;
            this.statMods = statMods;
            this.recipes = recipes;
        }

        public int getId()
        {
            return id;
        }
        public int getMaxStack()
        {
            return maxStack;
        }
        public int getPrice()
        {
            return price;
        }

        /**
         * Returns the total price of an item. This will change for recipes, as they include their parts fees
         */
        public int getTotalPrice()
        {
            var toReturn = price;

            foreach (var itemId in recipes)
            {
                var item = ItemManager.getInstance().getItemTemplateById(itemId);
                if (item == null)
                    continue;
                toReturn += item.getTotalPrice();
            }
            return toReturn;
        }

        public bool isTrinket()
        {
            return trinket;
        }
        public float getSellBackModifier()
        {
            return sellBackModifier;
        }
        public List<StatMod> getStatMods()
        {
            return statMods;
        }

        public bool isRecipe()
        {
            return recipes.Count > 0;
        }

        public List<int> getRecipeParts()
        {
            return recipes;
        }
    }

    public class ItemInstance
    {

        private ItemTemplate itemTemplate;
        private byte slot, stacks;
        private float cooldown;
        private bool recipeSearchFlag;

        // Methods to make old code compatible with already refactored code
        public int Id { get { return itemTemplate.getId(); } }
        public bool IsTrinket { get { return itemTemplate.isTrinket(); } }
        public int MaximumStackSize { get { return itemTemplate.getMaxStack(); } }
        public ItemInstance IncrementCount()
        {
            incrementStacks();
            if (stacks >= itemTemplate.getMaxStack())
            {
                decrementStacks();
                return null;
            }
            return this;
        }
        // End compatibility methods

        public ItemInstance(ItemTemplate itemTemplate, byte slot = 0, byte stacks = 1)
        {
            this.itemTemplate = itemTemplate;
            this.cooldown = 0;
            this.slot = slot;
            this.stacks = stacks;
            this.recipeSearchFlag = false;
        }

        public ItemTemplate getTemplate()
        {
            return itemTemplate;
        }

        public byte getSlot()
        {
            return slot;
        }

        public byte getStacks()
        {
            return stacks;
        }

        public void incrementStacks()
        {
            ++stacks;
        }

        public void decrementStacks()
        {
            --stacks;
        }

        public bool getRecipeSearchFlag()
        {
            return recipeSearchFlag;
        }

        public void setRecipeSearchFlag(bool flag)
        {
            recipeSearchFlag = flag;
        }
    }
}
