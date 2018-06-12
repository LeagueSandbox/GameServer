using LeagueSandbox.GameServer.Logic.GameObjects;
using System.Collections.Generic;
using System.Linq;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class ItemManager
    {
        private Dictionary<int, ItemType> _itemTypes;

        public ItemManager()
        {
            _itemTypes = new Dictionary<int, ItemType>();
        }

        public ItemType GetItemType(int itemId)
        {
            return _itemTypes[itemId];
        }

        public ItemType SafeGetItemType(int itemId, ItemType defaultValue)
        {
            if (!_itemTypes.ContainsKey(itemId)) return defaultValue;
            return _itemTypes[itemId];
        }

        public ItemType SafeGetItemType(int itemId)
        {
            return SafeGetItemType(itemId, null);
        }

        public void ResetItems()
        {
            _itemTypes.Clear();
        }

        public void LoadItems()
        {
            var itemContentCollection = ItemContentCollection.LoadItemsFrom(
                "Content/Data/LeagueSandbox-Default/Items"
            );

            foreach (var entry in itemContentCollection)
            {
                var itemType = ItemType.Load(this, entry.Value);
                _itemTypes.Add(entry.Key, itemType);
            }
        }
    }

    public class ItemType
    {
        //private ItemManager _owner;
        private ItemContentCollectionEntry _itemInfo;

        // Meta
        public int ItemId { get; private set; }
        public string Name { get; private set; }

        // General
        public int MaxStack { get; private set; }
        public int Price { get; private set; }
        public string ItemGroup { get; private set; }
        public float SellBackModifier { get; private set; }

        // Recipes
        public int RecipeItem1 { get; private set; }
        public int RecipeItem2 { get; private set; }
        public int RecipeItem3 { get; private set; }
        public int RecipeItem4 { get; private set; }

        // Not from data
        public ItemRecipe Recipe { get; private set; }
        public int TotalPrice => Recipe.TotalPrice;

        public float FlatArmorMod { get; set; }
        public float PercentArmorMod { get; set; }
        public float FlatCritChanceMod { get; set; }
        public float FlatCritDamageMod { get; set; }
        public float PercentCritDamageMod { get; set; }
        public float FlatHPPoolMod { get; set; }
        public float PercentHPPoolMod { get; set; }
        public float FlatMPPoolMod { get; set; }
        public float PercentMPPoolMod { get; set; }
        public float FlatMagicDamageMod { get; set; }
        public float PercentMagicDamageMod { get; set; }
        public float FlatMagicPenetrationMod { get; set; }
        public float FlatMovementSpeedMod { get; set; }
        public float PercentMovementSpeedMod { get; set; }
        public float FlatPhysicalDamageMod { get; set; }
        public float PercentPhysicalDamageMod { get; set; }
        public float FlatSpellBlockMod { get; set; }
        public float PercentSpellBlockMod { get; set; }
        public float PercentAttackSpeedMod { get; set; }
        public float PercentBaseHPRegenMod { get; set; }
        public float PercentBaseMPRegenMod { get; set; }

        private ItemType(ItemContentCollectionEntry itemInfo)
        {
            _itemInfo = itemInfo;
        }

        private void CreateRecipe(ItemManager manager)
        {
            Recipe = ItemRecipe.FromItemType(this, manager);
        }

        public static ItemType Load(ItemManager owner, ItemContentCollectionEntry itemInfo)
        {
            var result = new ItemType(itemInfo)
            {
                ItemId = itemInfo.ItemId,
                Name = itemInfo.Name,
                MaxStack = itemInfo.GetInt("Data", "MaxStack"),
                Price = itemInfo.GetInt("Data", "Price"),
                ItemGroup = itemInfo.GetString("Data", "ItemGroup"),
                SellBackModifier = itemInfo.GetFloat("Data", "SellBackModifier", 0.7f),

                RecipeItem1 = itemInfo.GetInt("Data", "RecipeItem1", -1),
                RecipeItem2 = itemInfo.GetInt("Data", "RecipeItem2", -1),
                RecipeItem3 = itemInfo.GetInt("Data", "RecipeItem3", -1),
                RecipeItem4 = itemInfo.GetInt("Data", "RecipeItem4", -1),
                FlatArmorMod = itemInfo.GetFloat("Data", "FlatArmorMod"),
                PercentArmorMod = itemInfo.GetFloat("Data", "PercentArmorMod"),
                FlatCritChanceMod = itemInfo.GetFloat("Data", "FlatCritChanceMod"),
                FlatCritDamageMod = itemInfo.GetFloat("Data", "FlatCritDamageMod"),
                PercentCritDamageMod = itemInfo.GetFloat("Data", "PercentCritDamageMod"),
                FlatHPPoolMod = itemInfo.GetFloat("Data", "FlatHPPoolMod"),
                PercentHPPoolMod = itemInfo.GetFloat("Data", "PercentHPPoolMod"),
                FlatMPPoolMod = itemInfo.GetFloat("Data", "FlatMPPoolMod"),
                PercentMPPoolMod = itemInfo.GetFloat("Data", "PercentMPPoolMod"),
                FlatMagicDamageMod = itemInfo.GetFloat("Data", "FlatMagicDamageMod"),
                PercentMagicDamageMod = itemInfo.GetFloat("Data", "PercentMagicDamageMod"),
                FlatMagicPenetrationMod = itemInfo.GetFloat("Data", "FlatMagicPenetrationMod"),
                FlatMovementSpeedMod = itemInfo.GetFloat("Data", "FlatMovementSpeedMod"),
                PercentMovementSpeedMod = itemInfo.GetFloat("Data", "PercentMovementSpeedMod"),
                FlatPhysicalDamageMod = itemInfo.GetFloat("Data", "FlatPhysicalDamageMod"),
                PercentPhysicalDamageMod = itemInfo.GetFloat("Data", "PercentPhysicalDamageMod"),
                FlatSpellBlockMod = itemInfo.GetFloat("Data", "FlatSpellBlockMod"),
                PercentSpellBlockMod = itemInfo.GetFloat("Data", "PercentSpellBlockMod"),
                PercentAttackSpeedMod = itemInfo.GetFloat("Data", "PercentAttackSpeedMod"),
                PercentBaseHPRegenMod = itemInfo.GetFloat("Data", "PercentBaseHPRegenMod"),
                PercentBaseMPRegenMod = itemInfo.GetFloat("Data", "PercentBaseMPRegenMod")
            };

            //itemInfo.SafeGetFloat("Data", "PercentEXPBonus"); // TODO

            result.CreateRecipe(owner);
            return result;
        }

        public bool GetIsTrinket()
        {
            return ItemGroup.ToLower() == "relicbase";
        }
    }

    public class ItemRecipe
    {
        private ItemType _owner;
        private ItemType[] _items;
        private int _totalPrice;
        private ItemManager _itemManager;

        public int TotalPrice
        {
            get
            {
                if (_totalPrice <= -1)
                {
                    FindPrice();
                }

                return _totalPrice;
            }
        }

        private ItemRecipe(ItemType owner, ItemManager manager)
        {
            _owner = owner;
            _totalPrice = -1;
            _itemManager = manager;
        }

        public List<ItemType> GetItems()
        {
            if (_items == null)
                FindRecipeItems(_itemManager);
            return _items.ToList();
        }

        private void FindRecipeItems(ItemManager itemManager)
        {
            // TODO: Figure out how to refactor this.
            _items = new[]
            {
                itemManager.SafeGetItemType(_owner.RecipeItem1),
                itemManager.SafeGetItemType(_owner.RecipeItem2),
                itemManager.SafeGetItemType(_owner.RecipeItem3),
                itemManager.SafeGetItemType(_owner.RecipeItem4)
            };
        }

        private void FindPrice()
        {
            _totalPrice = 0;
            foreach (var item in GetItems())
            {
                if (item != null)
                {
                    _totalPrice += item.TotalPrice;
                }
            }

            _totalPrice += _owner.Price;
        }

        public static ItemRecipe FromItemType(ItemType type, ItemManager manager)
        {
            return new ItemRecipe(type, manager);
        }
    }

    public class Item
    {
        public byte StackSize { get; private set; }
        public int TotalPrice { get; private set; }
        public ItemType ItemType { get; private set; }

        private Inventory _owner;

        private Item(Inventory owner, ItemType type)
        {
            _owner = owner;
            ItemType = type;
            StackSize = 1;
        }

        public bool IncrementStackSize()
        {
            if (StackSize >= ItemType.MaxStack)
            {
                return false;
            }

            StackSize++;
            return true;
        }

        public bool DecrementStackSize()
        {
            if (StackSize < 1)
            {
                return false;
            }

            StackSize--;
            return true;
        }

        public static Item CreateFromType(Inventory inventory, ItemType item)
        {
            return new Item(inventory, item);
        }
    }
}
