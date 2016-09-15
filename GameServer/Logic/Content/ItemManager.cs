using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

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
            foreach(var entry in itemContentCollection)
            {
                var itemType = ItemType.Load(this, entry.Value);
                _itemTypes.Add(entry.Key, itemType);
            }
        }
    }

    public class ItemType : IBuff
    {
        private ItemManager _owner;
        private ItemContentCollectionEntry _itemInfo;

        // Meta
        public int ItemId { get; private set; }
        public string Name { get; private set; }

        // General
        public int MaxStack { get; private set; }
        public int Price { get; private set; }
        public string ItemGroup { get; private set; }
        public float SellBackModifier { get; private set; }

        // Stats
        public StatModifcator HealthPoints { get; set; }
        public StatModifcator HealthRegeneration { get; set; }
        public StatModifcator AttackDamage { get; set; }
        public StatModifcator AbilityPower { get; set; }
        public StatModifcator CriticalChance { get; set; }
        public StatModifcator Armor { get; set; }
        public StatModifcator MagicResist { get; set; }
        public StatModifcator AttackSpeed { get; set; }
        public StatModifcator ArmorPenetration { get; set; }
        public StatModifcator MagicPenetration { get; set; }
        public StatModifcator ManaPoints { get; set; }
        public StatModifcator ManaRegeneration { get; set; }
        public StatModifcator LifeSteel { get; set; }
        public StatModifcator SpellVamp { get; set; }
        public StatModifcator Tenacity { get; set; }
        public StatModifcator Size { get; set; }
        public StatModifcator Range { get; set; }
        public StatModifcator MoveSpeed { get; set; }
        public StatModifcator GoldPerSecond { get; set; }

        // Recipes
        public int RecipeItem1 { get; private set; }
        public int RecipeItem2 { get; private set; }
        public int RecipeItem3 { get; private set; }
        public int RecipeItem4 { get; private set; }

        // Not from data
        public ItemRecipe Recipe { get; private set; }
        public int TotalPrice { get { return Recipe.TotalPrice; } }

        private ItemType(ItemManager owner, ItemContentCollectionEntry itemInfo)
        {
            _itemInfo = itemInfo;

            HealthPoints = new StatModifcator();
            HealthRegeneration = new StatModifcator();
            AttackDamage = new StatModifcator();
            AbilityPower = new StatModifcator();
            CriticalChance = new StatModifcator();
            Armor = new StatModifcator();
            MagicResist = new StatModifcator();
            AttackSpeed = new StatModifcator();
            ArmorPenetration = new StatModifcator();
            MagicPenetration = new StatModifcator();
            ManaPoints = new StatModifcator();
            ManaRegeneration = new StatModifcator();
            LifeSteel = new StatModifcator();
            SpellVamp = new StatModifcator();
            Tenacity = new StatModifcator();
            Size = new StatModifcator();
            Range = new StatModifcator();
            MoveSpeed = new StatModifcator();
            GoldPerSecond = new StatModifcator();
        }

        private void CreateRecipe(ItemManager manager)
        {
            Recipe = ItemRecipe.FromItemType(this, manager);
        }

        public static ItemType Load(ItemManager owner, ItemContentCollectionEntry itemInfo)
        {
            // Because IntelliSense is nice to have
            var result = new ItemType(owner, itemInfo)
            {
                ItemId = itemInfo.ItemId,
                Name = itemInfo.Name,
                MaxStack = itemInfo.SafeGetInt("Data", "MaxStack"),
                Price = itemInfo.SafeGetInt("Data", "Price"),
                ItemGroup = itemInfo.SafeGetString("Data", "ItemGroup"),
                SellBackModifier = itemInfo.SafeGetFloat("Data", "SellBackModifier", 0.7f),

                RecipeItem1 = itemInfo.SafeGetInt("Data", "RecipeItem1", -1),
                RecipeItem2 = itemInfo.SafeGetInt("Data", "RecipeItem2", -1),
                RecipeItem3 = itemInfo.SafeGetInt("Data", "RecipeItem3", -1),
                RecipeItem4 = itemInfo.SafeGetInt("Data", "RecipeItem4", -1)
            };

            result.Armor.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatArmorMod");
            result.CriticalChance.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatCritChanceMod");
            //itemInfo.SafeGetFloat("Data", "FlatCritDamageMod"); // TODO
            result.HealthPoints.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatHPPoolMod");
            result.ManaPoints.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatMPPoolMod");
            result.AbilityPower.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatMagicDamageMod");
            result.MagicPenetration.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatMagicPenetrationMod");
            result.MoveSpeed.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatMovementSpeedMod");
            result.AttackDamage.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatPhysicalDamageMod");
            result.MagicResist.FlatBonus = itemInfo.SafeGetFloat("Data", "FlatSpellBlockMod");
            result.Armor.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentArmorMod");
            result.AttackSpeed.FlatBonus = itemInfo.SafeGetFloat("Data", "PercentAttackSpeedMod");
            //itemInfo.SafeGetFloat("Data", "PercentCritDamageMod"); // TODO
            //itemInfo.SafeGetFloat("Data", "PercentEXPBonus"); // TODO
            result.HealthPoints.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentHPPoolMod");
            result.HealthRegeneration.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentBaseHPRegenMod");
            result.ManaPoints.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentMPPoolMod");
            result.ManaRegeneration.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentBaseMPRegenMod");
            result.AbilityPower.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentMagicDamageMod");
            result.MoveSpeed.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentMovementSpeedMod");
            result.AttackDamage.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentPhysicalDamageMod");
            result.MagicResist.PercentBonus = itemInfo.SafeGetFloat("Data", "PercentSpellBlockMod");

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
                    FindPrice();
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
            _items = new ItemType[]
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
                if(item != null)
                    _totalPrice += item.TotalPrice;
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
            if (StackSize >= ItemType.MaxStack) return false;
            StackSize++;
            return true;
        }

        public bool DecrementStackSize()
        {
            if (StackSize < 1) return false;
            StackSize--;
            return true;
        }

        public static Item CreateFromType(Inventory inventory, ItemType item)
        {
            return new Item(inventory, item);
        }
    }
}
