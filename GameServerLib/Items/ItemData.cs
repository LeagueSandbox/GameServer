using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemData : StatsModifier, IItemData
    {
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

        private void CreateRecipe(ItemManager manager)
        {
            Recipe = ItemRecipe.FromItemType(this, manager);
        }

        public static ItemData Load(ItemManager owner, ContentFile content, int itemId)
        {
            // Because IntelliSense is nice to have
            var result = new ItemData
            {
                ItemId = itemId,
                Name = content.GetString("Data", "DisplayName"),
                MaxStack = content.GetInt("Data", "MaxStack"),
                Price = content.GetInt("Data", "Price"),
                ItemGroup = content.GetString("Data", "ItemGroup"),
                SpellName = content.GetString("Data", "SpellName"),
                SellBackModifier = content.GetFloat("Data", "SellBackModifier", 0.7f),

                RecipeItem1 = content.GetInt("Data", "RecipeItem1", -1),
                RecipeItem2 = content.GetInt("Data", "RecipeItem2", -1),
                RecipeItem3 = content.GetInt("Data", "RecipeItem3", -1),
                RecipeItem4 = content.GetInt("Data", "RecipeItem4", -1),
                Armor =
                {
                    FlatBonus = content.GetFloat("Data", "FlatArmorMod"),
                    PercentBonus = content.GetFloat("Data", "PercentArmorMod")
                },
                CriticalChance =
                {
                    FlatBonus = content.GetFloat("Data", "FlatCritChanceMod")
                },
                HealthPoints =
                {
                    FlatBonus = content.GetFloat("Data", "FlatHPPoolMod"),
                    PercentBonus = content.GetFloat("Data", "PercentHPPoolMod")
                },
                ManaPoints =
                {
                    FlatBonus = content.GetFloat("Data", "FlatMPPoolMod"),
                    PercentBonus = content.GetFloat("Data", "PercentMPPoolMod")
                },
                AbilityPower =
                {
                    FlatBonus = content.GetFloat("Data", "FlatMagicDamageMod"),
                    PercentBonus = content.GetFloat("Data", "PercentMagicDamageMod")
                },
                MagicPenetration =
                {
                    FlatBonus = content.GetFloat("Data", "FlatMagicPenetrationMod")
                },
                MoveSpeed =
                {
                    FlatBonus = content.GetFloat("Data", "FlatMovementSpeedMod"),
                    PercentBonus = content.GetFloat("Data", "PercentMovementSpeedMod")
                },
                AttackDamage =
                {
                    FlatBonus = content.GetFloat("Data", "FlatPhysicalDamageMod"),
                    PercentBonus = content.GetFloat("Data", "PercentPhysicalDamageMod")
                },
                MagicResist =
                {
                    FlatBonus = content.GetFloat("Data", "FlatSpellBlockMod"),
                    PercentBonus = content.GetFloat("Data", "PercentSpellBlockMod")
                },
                AttackSpeed =
                {
                    FlatBonus = content.GetFloat("Data", "PercentAttackSpeedMod")
                },
                HealthRegeneration =
                {
                    PercentBonus = content.GetFloat("Data", "PercentBaseHPRegenMod")
                },
                ManaRegeneration =
                {
                    PercentBonus = content.GetFloat("Data", "PercentBaseMPRegenMod")
                },
                CriticalDamage =
                {
                    FlatBonus = content.GetFloat("Data", "FlatCritDamageMod"),
                    PercentBonus = content.GetFloat("Data", "PercentCritDamageMod")
                },
                LifeSteel =
                {
                    FlatBonus = content.GetFloat("Data", "PercentLifeStealMod")
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