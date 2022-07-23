using System;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace LeagueSandbox.GameServer.Inventory
{
    public class ItemData : StatsModifier
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
        public ItemRecipe Recipe { get; private set; }
        public int TotalPrice => Recipe.TotalPrice;

        public void CreateRecipe(ItemManager manager)
        {
            Recipe = ItemRecipe.FromItemType(this, manager);
        }

        public ItemData Load(ContentFile file)
        {
            Name = file.Name;

            ItemId = Convert.ToInt32(file.MetaData["Id"]);
            MaxStacks = file.GetInt("Data", "MaxStack");
            Price = file.GetInt("Data", "Price");
            ItemGroup = file.GetString("Data", "ItemGroup");
            Consumed = file.GetBool("Data", "Consumed");
            SpellName = file.GetString("Data", "SpellName");
            SellBackModifier = file.GetFloat("Data", "SellBackModifier", 0.7f);

            RecipeItem1 = file.GetInt("Data", "RecipeItem1", -1);
            RecipeItem2 = file.GetInt("Data", "RecipeItem2", -1);
            RecipeItem3 = file.GetInt("Data", "RecipeItem3", -1);
            RecipeItem4 = file.GetInt("Data", "RecipeItem4", -1);
            Armor.FlatBonus = file.GetFloat("Data", "FlatArmorMod");
            Armor.PercentBonus = file.GetFloat("Data", "PercentArmorMod");
            CriticalChance.FlatBonus = file.GetFloat("Data", "FlatCritChanceMod");
            HealthPoints.FlatBonus = file.GetFloat("Data", "FlatHPPoolMod");
            HealthPoints.PercentBonus = file.GetFloat("Data", "PercentHPPoolMod");
            ManaPoints.FlatBonus = file.GetFloat("Data", "FlatMPPoolMod");
            ManaPoints.PercentBonus = file.GetFloat("Data", "PercentMPPoolMod");
            AbilityPower.FlatBonus = file.GetFloat("Data", "FlatMagicDamageMod");
            AbilityPower.PercentBonus = file.GetFloat("Data", "PercentMagicDamageMod");
            MagicPenetration.FlatBonus = file.GetFloat("Data", "FlatMagicPenetrationMod");
            MoveSpeed.FlatBonus = file.GetFloat("Data", "FlatMovementSpeedMod");
            MoveSpeed.PercentBonus = file.GetFloat("Data", "PercentMovementSpeedMod");
            AttackDamage.FlatBonus = file.GetFloat("Data", "FlatPhysicalDamageMod");
            AttackDamage.PercentBonus = file.GetFloat("Data", "PercentPhysicalDamageMod");
            MagicResist.FlatBonus = file.GetFloat("Data", "FlatSpellBlockMod");
            MagicResist.PercentBonus = file.GetFloat("Data", "PercentSpellBlockMod");
            AttackSpeed.FlatBonus = file.GetFloat("Data", "PercentAttackSpeedMod");
            HealthRegeneration.PercentBonus = file.GetFloat("Data", "PercentBaseHPRegenMod");
            ManaRegeneration.PercentBonus = file.GetFloat("Data", "PercentBaseMPRegenMod");
            CriticalDamage.FlatBonus = file.GetFloat("Data", "FlatCritDamageMod");
            CriticalDamage.PercentBonus = file.GetFloat("Data", "PercentCritDamageMod");
            LifeSteal.FlatBonus = file.GetFloat("Data", "PercentLifeStealMod");

            //itemInfo.SafeGetFloat("Data", "PercentEXPBonus"); // TODO

            return this;
        }
    }

}