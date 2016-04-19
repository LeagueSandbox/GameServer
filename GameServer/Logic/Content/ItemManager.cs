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
        private Game _owner;
        private Dictionary<int, ItemType> _itemTypes;

        private ItemManager(Game owner)
        {
            _owner = owner;
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

        public static ItemManager LoadItems(Game game)
        {
            var result = new ItemManager(game);
            var itemContentCollection = ItemContentCollection.LoadItemsFrom("Content/Data/LeagueSandbox-Default/Items");
            foreach(var entry in itemContentCollection)
            {
                var itemType = ItemType.Load(game, result, entry.Value);
                result._itemTypes.Add(entry.Key, itemType);
            }
            return result;
        }
    }

    public class ItemType
    {
        private Game _game;
        private ItemManager _owner;
        private ItemContentCollectionEntry _itemInfo;
        private StatMod[] _statMods;

        // Meta
        public int ItemId { get; private set; }
        public string Name { get; private set; }

        // General
        public int MaxStack { get; private set; }
        public int Price { get; private set; }
        public string ItemGroup { get; private set; }
        public float SellBackModifier { get; private set; }

        // Stats
        public float FlatPhysicalDamageMod { get; private set; }
        public float PercentPhysicalDamageMod { get; private set; }
        public float FlatMagicDamageMod { get; private set; }
        public float FlatHPRegenMod { get; private set; }
        public float FlatCritChanceMod { get; private set; }
        public float FlatArmorMod { get; private set; }
        public float FlatSpellBlockMod { get; private set; }
        public float PercentAttackSpeedMod { get; private set; }
        public float PercentLifeStealMod { get; private set; }
        public float FlatHPPoolMod { get; private set; }
        public float FlatMPPoolMod { get; private set; }
        public float FlatMovementSpeedMod { get; private set; }

        // Recipes
        public int RecipeItem1 { get; private set; }
        public int RecipeItem2 { get; private set; }
        public int RecipeItem3 { get; private set; }
        public int RecipeItem4 { get; private set; }

        // Not from data
        public ItemRecipe Recipe { get; private set; }
        public int TotalPrice { get { return Recipe.TotalPrice; } }
        public List<StatMod> StatMods { get { if (_statMods == null) CreateStatMods(); return _statMods.ToList(); } }

        private ItemType(Game game, ItemManager owner, ItemContentCollectionEntry itemInfo)
        {
            _game = game;
            _owner = owner;
            _itemInfo = itemInfo;
        }

        public void CreateStatMods()
        {
            _statMods = new StatMod[]
            {
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat, FlatPhysicalDamageMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Pct, PercentPhysicalDamageMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat, FlatMagicDamageMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Hp5, FlatHPRegenMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Crit_Chance, FlatCritChanceMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Armor, FlatArmorMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Magic_Armor, FlatSpellBlockMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier, PercentAttackSpeedMod),
                StatMod.FromValues(MasterMask.MM_Two, FieldMask.FM2_LifeSteal, PercentLifeStealMod),
                StatMod.FromValues(MasterMask.MM_Four, FieldMask.FM4_MaxHp, FlatHPPoolMod),
                StatMod.FromValues(MasterMask.MM_Four, FieldMask.FM4_MaxMp, FlatMPPoolMod),
                StatMod.FromValues(MasterMask.MM_Four, FieldMask.FM4_Speed, FlatMovementSpeedMod)
            };
        }

        private void CreateRecipe()
        {
            Recipe = ItemRecipe.FromItemType(_game, this);
        }

        public static ItemType Load(Game game, ItemManager owner, ItemContentCollectionEntry itemInfo)
        {
            // Because IntelliSense is nice to have
            var result = new ItemType(game, owner, itemInfo)
            {
                ItemId = itemInfo.ItemId,
                Name = itemInfo.Name,
                MaxStack = itemInfo.SafeGetInt("Data", "MaxStack"),
                Price = itemInfo.SafeGetInt("Data", "Price"),
                ItemGroup = itemInfo.SafeGetString("Data", "ItemGroup"),
                SellBackModifier = itemInfo.SafeGetFloat("Data", "SellBackModifier", 0.7f),
                FlatPhysicalDamageMod = itemInfo.SafeGetFloat("Data", "FlatPhysicalDamageMod"),
                PercentPhysicalDamageMod = itemInfo.SafeGetFloat("Data", "PercentPhysicalDamageMod"),
                FlatMagicDamageMod = itemInfo.SafeGetFloat("Data", "FlatMagicDamageMod"),
                FlatHPRegenMod = itemInfo.SafeGetFloat("Data", "FlatHPRegenMod"),
                FlatCritChanceMod = itemInfo.SafeGetFloat("Data", "FlatCritChanceMod"),
                FlatArmorMod = itemInfo.SafeGetFloat("Data", "FlatArmorMod"),
                FlatSpellBlockMod = itemInfo.SafeGetFloat("Data", "FlatSpellBlockMod"),
                PercentAttackSpeedMod = itemInfo.SafeGetFloat("Data", "PercentAttackSpeedMod"),
                PercentLifeStealMod = itemInfo.SafeGetFloat("Data", "PercentLifeStealMod"),
                FlatHPPoolMod = itemInfo.SafeGetFloat("Data", "FlatHPPoolMod"),
                FlatMPPoolMod = itemInfo.SafeGetFloat("Data", "FlatMPPoolMod"),
                FlatMovementSpeedMod = itemInfo.SafeGetFloat("Data", "FlatMovementSpeedMod"),
                RecipeItem1 = itemInfo.SafeGetInt("Data", "RecipeItem1", -1),
                RecipeItem2 = itemInfo.SafeGetInt("Data", "RecipeItem2", -1),
                RecipeItem3 = itemInfo.SafeGetInt("Data", "RecipeItem3", -1),
                RecipeItem4 = itemInfo.SafeGetInt("Data", "RecipeItem4", -1)
            };
            result.CreateRecipe();
            return result;
        }

        public bool GetIsTrinket()
        {
            return ItemGroup.ToLower() == "relicbase";
        }
    }

    public class ItemRecipe
    {
        private Game _game;
        private ItemType _owner;
        private ItemType[] _items;
        private int _totalPrice;

        public List<ItemType> Items { get { if (_items == null) FindRecipeItems(); return _items.ToList(); } }
        public int TotalPrice { get { if (_totalPrice < -1) FindPrice(); return _totalPrice; } }

        private ItemRecipe(Game game, ItemType owner)
        {
            _game = game;
            _owner = owner;
            _totalPrice = -1;
        }

        private void FindRecipeItems()
        {
            _items = new ItemType[]
            {
                _game.ItemManager.SafeGetItemType(_owner.RecipeItem1),
                _game.ItemManager.SafeGetItemType(_owner.RecipeItem2),
                _game.ItemManager.SafeGetItemType(_owner.RecipeItem3),
                _game.ItemManager.SafeGetItemType(_owner.RecipeItem4)
            };
        }

        private void FindPrice()
        {
            _totalPrice = 0;
            foreach (var item in Items)
            {
                _totalPrice += item.TotalPrice;
            }
            _totalPrice += _owner.Price;;
        }

        public static ItemRecipe FromItemType(Game game, ItemType type)
        {
            return new ItemRecipe(game, type);
        }
    }

    public class Item
    {
        public byte Slot { get; private set; }
        public byte StackSize { get; private set; }
        public int TotalPrice { get; private set; }
        public ItemType ItemType { get; private set; }

        private Game _game;
        private Inventory _owner;

        private Item(Game game, Inventory owner, ItemType type, byte slot)
        {
            _game = game;
            _owner = owner;
            ItemType = type;
            StackSize = 1;
            Slot = slot;
        }

        public bool IncrementStackSize()
        {
            if (StackSize >= ItemType.MaxStack) return false;
            StackSize++;
            return true;
        }

        public bool DecrementStackSize()
        {
            if (StackSize <= 1) return false;
            StackSize--;
            return true;
        }

        public void SetSlot(byte newSlot)
        {
            Slot = newSlot;
        }

        public static Item CreateFromType(Game _game, Inventory inventory, ItemType item, byte slot)
        {
            return new Item(_game, inventory, item, slot);
        }
    }
}
