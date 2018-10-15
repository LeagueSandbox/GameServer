using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Items
{
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
                LifeSteal =
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

}