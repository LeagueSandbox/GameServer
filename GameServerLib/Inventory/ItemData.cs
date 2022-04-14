using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Inventory
{
    public class ItemData : StatsModifier, IItemData
    {
        // Meta
        public int ItemId { get; private set; }
        public string Name { get; private set; }

        // General
        public int MaxStacks { get; private set; }
        public int Price { get; private set; }
        public string ItemGroup { get; private set; }
        public bool Consumed { get; private set; }
        public string SpellName { get; private set; }
        public float SellBackModifier { get; private set; }

        // Recipes
        private int RecipeItem1 { get; set; }
        private int RecipeItem2 { get; set; }
        private int RecipeItem3 { get; set; }
        private int RecipeItem4 { get; set; }

        public int[] RecipeItem
        {
            get
            {
                return new int[4]
                {
                    RecipeItem1,
                    RecipeItem2,
                    RecipeItem3,
                    RecipeItem4,
                };
            }
        }

        // Not from data
        public IItemRecipe Recipe { get; private set; }
        public int TotalPrice => Recipe.TotalPrice;

        private ItemData()
        {
            
        }

        private void CreateRecipe(ItemManager manager)
        {
            Recipe = ItemRecipe.FromItemType(this, manager);
        }

        public static ItemData Load(ItemManager owner, ItemContentCollectionEntry itemInfo)
        {
            // Because IntelliSense is nice to have
            var result = new ItemData()
            {
                ItemId = itemInfo.ItemId,
                Name = itemInfo.Name,
                MaxStacks = itemInfo.GetInt("Data", "MaxStack"),
                Price = itemInfo.GetInt("Data", "Price"),
                ItemGroup = itemInfo.GetString("Data", "ItemGroup"),
                Consumed = itemInfo.GetBool("Data", "Consumed"),
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
    }

}