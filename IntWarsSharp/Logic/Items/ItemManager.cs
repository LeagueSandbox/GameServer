using InibinSharp;
using IntWarsSharp.Core.Logic;
using IntWarsSharp.Core.Logic.PacketHandlers;
using IntWarsSharp.Core.Logic.RAF;
using IntWarsSharp.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.Items
{
    class ItemManager
    {
        private static ItemManager _instance;

        private Dictionary<int, ItemTemplate> itemTemplates;
        private ItemManager()
        {

        }

        public void init()
        {
            itemTemplates = new Dictionary<int, ItemTemplate>();
            // TODO : this is highly inefficient
            for (var i = 1000; i < 4000; ++i)
            {
                Inibin inibin;
                if (!RAFManager.getInstance().readInibin("DATA/items/" + i + ".inibin", out inibin))
                    continue;


                var maxStack = inibin.getIntValue("DATA", "MaxStack");
                var price = inibin.getIntValue("DATA", "Price");
                bool trinket = inibin.getBoolValue(0x32E2CBC9);

                float sellBack = 0.7f;

                if (inibin.KeyExists("DATA", "SellBackModifier"))
                    sellBack = inibin.getFloatValue("DATA", "SellBackModifier");

                var statMods = new List<StatMod>();

                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Bonus_Ad_Flat, value = inibin.getFloatValue("DATA", "FlatPhysicalDamageMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Bonus_Ad_Pct, value = inibin.getFloatValue("DATA", "PercentPhysicalDamageMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Bonus_Ap_Flat, value = inibin.getFloatValue("DATA", "FlatMagicDamageMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Hp5, value = inibin.getFloatValue("DATA", "FlatHPRegenMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Crit_Chance, value = inibin.getFloatValue("DATA", "FlatCritChanceMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Armor, value = inibin.getFloatValue("DATA", "FlatArmorMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Magic_Armor, value = inibin.getFloatValue("DATA", "FlatSpellBlockMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Atks_multiplier, value = inibin.getFloatValue("DATA", "PercentAttackSpeedMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_LifeSteal, value = inibin.getFloatValue("DATA", "PercentLifeStealMod") });

                statMods.Add(new StatMod { blockId = MasterMask.MM_Four, mask = FieldMask.FM4_MaxHp, value = inibin.getFloatValue("DATA", "FlatHPPoolMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Four, mask = FieldMask.FM4_MaxMp, value = inibin.getFloatValue("DATA", "FlatMPPoolMod") });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Four, mask = FieldMask.FM4_Speed, value = inibin.getFloatValue("DATA", "FlatMovementSpeedMod") });

                var recipes = new List<int>();

                var c = 1;
                while (inibin.KeyExists("DATA", "RecipeItem" + c))
                {
                    var componentId = inibin.getIntValue("DATA", "RecipeItem" + c);
                    if (componentId > 0)
                    { // sometimes there are "0" entries
                        recipes.Add(componentId);
                    }
                    ++c;
                }

                itemTemplates.Add(itemTemplates.Count, new ItemTemplate(i, maxStack, price, sellBack, trinket, statMods, recipes));
            }


            Logger.LogCoreInfo("Loaded " + itemTemplates.Count + " items");
        }
        public ItemTemplate getItemTemplateById(int id)
        {
            if (!itemTemplates.ContainsKey(id))
                return null;

            return itemTemplates[id];
        }


        public static ItemManager getInstance()
        {
            if (_instance == null)
                _instance = new ItemManager();
            return _instance;
        }
    }
}
