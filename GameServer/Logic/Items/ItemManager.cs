using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Items
{
    class ItemManager
    {
        private static ItemManager _instance;

        private Dictionary<int, ItemTemplate> itemTemplates;

        public int GetItemPrice(int itemId)
        {
            throw new NotImplementedException("Fuck yo");
        }

        public void init()
        {
            itemTemplates = new Dictionary<int, ItemTemplate>();
            var itemCollection = ItemCollection.LoadItemsFrom("Content/Data/LeagueSandbox-Default/Items");
            foreach(var itemEntry in itemCollection)
            {
                var itemData = itemEntry.Value;


                var maxStack = itemData.SafeGetValue("Data", "MaxStack", 0);
                var price = itemData.SafeGetValue("Data", "Price", 0);

                var itemGroup = itemData.SafeGetValue("Data", "ItemGroup", "");
                bool trinket = itemGroup.ToLower() == "relicbase";

                float sellBack = itemData.SafeGetValue("Data", "SellBackModifier", 0.7f);

                var statMods = new List<StatMod>();

                // "Another part that's just completely ass" - Mythic, 14th April 2016
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Bonus_Ad_Flat, value = itemData.SafeGetValue("Data", "FlatPhysicalDamageMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Bonus_Ad_Pct, value = itemData.SafeGetValue("Data", "PercentPhysicalDamageMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Bonus_Ap_Flat, value = itemData.SafeGetValue("Data", "FlatMagicDamageMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Hp5, value = itemData.SafeGetValue("Data", "FlatHPRegenMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Crit_Chance, value = itemData.SafeGetValue("Data", "FlatCritChanceMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Armor, value = itemData.SafeGetValue("Data", "FlatArmorMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Magic_Armor, value = itemData.SafeGetValue("Data", "FlatSpellBlockMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_Atks_multiplier, value = itemData.SafeGetValue("Data", "PercentAttackSpeedMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Two, mask = FieldMask.FM2_LifeSteal, value = itemData.SafeGetValue("Data", "PercentLifeStealMod", 0f) });

                statMods.Add(new StatMod { blockId = MasterMask.MM_Four, mask = FieldMask.FM4_MaxHp, value = itemData.SafeGetValue("Data", "FlatHPPoolMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Four, mask = FieldMask.FM4_MaxMp, value = itemData.SafeGetValue("Data", "FlatMPPoolMod", 0f) });
                statMods.Add(new StatMod { blockId = MasterMask.MM_Four, mask = FieldMask.FM4_Speed, value = itemData.SafeGetValue("Data", "FlatMovementSpeedMod", 0f) });

                var recipes = new List<int>();

                var index = 1;
                var componentId = itemData.SafeGetValue("Data", string.Format("RecipeItem{0}", index), -1);
                while (componentId > -1)
                {
                    recipes.Add(componentId);
                    index++;
                    componentId = itemData.SafeGetValue("Data", string.Format("RecipeItem{0}", index), -1);
                }

                var itemTemplate = new ItemTemplate(
                    itemData.ItemId,
                    maxStack,
                    price,
                    sellBack,
                    trinket,
                    statMods,
                    recipes
                );
                itemTemplates.Add(itemData.ItemId, itemTemplate);
                Logger.LogCoreInfo("Loaded Item: {0}", itemData.ItemName);
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
