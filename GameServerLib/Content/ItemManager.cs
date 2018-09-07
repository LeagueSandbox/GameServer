using System.Collections.Generic;
using System.Linq;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.Content
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
            if (!_itemTypes.ContainsKey(itemId))
            {
                return defaultValue;
            }

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

        public void LoadItems(string contentPath)
        {
            var itemContentCollection = ItemContentCollection.LoadItemsFrom(
                $"{contentPath}/LeagueSandbox-Default/Items"
            );

            foreach (var entry in itemContentCollection)
            {
                var itemType = ItemType.Load(this, entry.Value);
                _itemTypes.Add(entry.Key, itemType);
            }
        }
    }

    public class ItemType : StatsModifier, IItemType
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
        public string SpellName { get; private set; }
        public float SellBackModifier { get; private set; }

        // Recipes
        public int RecipeItem1 { get; private set; }
        public int RecipeItem2 { get; private set; }
        public int RecipeItem3 { get; private set; }
        public int RecipeItem4 { get; private set; }

        // Not from data
        public ItemRecipe Recipe { get; private set; }
        public int TotalPrice => Recipe.TotalPrice;

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
            // Because IntelliSense is nice to have
            var result = new ItemType(itemInfo)
            {
                ItemId = itemInfo.ItemId,
                Name = itemInfo.Name,
                MaxStack = itemInfo.GetInt("Data", "MaxStack"),
                Price = itemInfo.GetInt("Data", "Price"),
                ItemGroup = itemInfo.GetString("Data", "ItemGroup"),
                SpellName = itemInfo.GetString("Data", "SpellName"),
                SellBackModifier = itemInfo.GetFloat("Data", "SellBackModifier", 0.7f),

                RecipeItem1 = itemInfo.GetInt("Data", "RecipeItem1", -1),
                RecipeItem2 = itemInfo.GetInt("Data", "RecipeItem2", -1),
                RecipeItem3 = itemInfo.GetInt("Data", "RecipeItem3", -1),
                RecipeItem4 = itemInfo.GetInt("Data", "RecipeItem4", -1),
                Armor =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatArmorMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentArmorMod")
                },
                CriticalChance =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatCritChanceMod")
                },
                HealthPoints =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatHPPoolMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentHPPoolMod")
                },
                ManaPoints =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatMPPoolMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentMPPoolMod")
                },
                AbilityPower =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatMagicDamageMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentMagicDamageMod")
                },
                MagicPenetration =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatMagicPenetrationMod")
                },
                MoveSpeed =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatMovementSpeedMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentMovementSpeedMod")
                },
                AttackDamage =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatPhysicalDamageMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentPhysicalDamageMod")
                },
                MagicResist =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatSpellBlockMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentSpellBlockMod")
                },
                AttackSpeed =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "PercentAttackSpeedMod")
                },
                HealthRegeneration =
                {
                    PercentBonus = itemInfo.GetFloat("Data", "PercentBaseHPRegenMod")
                },
                ManaRegeneration =
                {
                    PercentBonus = itemInfo.GetFloat("Data", "PercentBaseMPRegenMod")
                },
                CriticalDamage =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "FlatCritDamageMod"),
                    PercentBonus = itemInfo.GetFloat("Data", "PercentCritDamageMod")
                },
                LifeSteel =
                {
                    FlatBonus = itemInfo.GetFloat("Data", "PercentLifeStealMod")
                }
            };

            //itemInfo.SafeGetFloat("Data", "PercentEXPBonus"); // TODO

            result.CreateRecipe(owner);
            return result;
        }

        public bool IsTrinket()
        {
            return ItemGroup.ToLower().Equals("relicbase");
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
            {
                FindRecipeItems(_itemManager);
            }

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

    public class Item : IItem
    {
        public byte StackSize { get; private set; }
        public int TotalPrice { get; private set; }
        public ItemType ItemType { get; private set; }

        IItemType IItem.ItemType => ItemType;

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
