#region LICENSE

// Copyright 2014 - 2014 InibinSharp
// Inibin.cs is part of InibinSharp.
// InibinSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// InibinSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with InibinSharp. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using InibinSharp.RAF;

#endregion

namespace InibinSharp
{
    public class Inibin : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly int _stringOffset;
        public readonly Dictionary<uint, object> Values = new Dictionary<uint, object>();

        public Inibin(byte[] data)
            : this(new MemoryStream(data))
        {
        }

        public Inibin(string filePath)
            : this(File.ReadAllBytes(filePath))
        {
        }

        public Inibin(RAFFileListEntry file)
            : this(file.GetContent())
        {
        }

        public Inibin(Stream stream)
        {
            _reader = new BinaryReader(stream);

            var size = (int)_reader.BaseStream.Length;
            var version = ReadValue<byte>();
            var oldLength = ReadValue<UInt16>();
            var bitmask = ReadValue<UInt16>();
            _stringOffset = size - oldLength;

          /*  Debug.WriteLine("Version:" + version);
            Debug.WriteLine("Length:" + size);
            Debug.WriteLine("OldLength:" + oldLength);
            Debug.WriteLine("StringOffset:" + _stringOffset);
            Debug.WriteLine("Bitmask:" + bitmask);
            Debug.WriteLine("");*/

            if (version != 2)
            {
                throw new InvalidDataException("Wrong Ininbin version: " + version);
            }

            if ((bitmask & 0x0001) != 0)
            {
                ParseValues<UInt32>();
            }

            if ((bitmask & 0x0002) != 0)
            {
                ParseValues<float>();
            }

            if ((bitmask & 0x0004) != 0)
            {
                ParseValues<byte>(true);
            }

            if ((bitmask & 0x0008) != 0)
            {
                ParseValues<UInt16>();
            }

            if ((bitmask & 0x0010) != 0)
            {
                ParseValues<byte>();
            }

            if ((bitmask & 0x0020) != 0)
            {
                ParseValues<bool>();
            }

            if ((bitmask & 0x0040) != 0)
            {
                SkipValues(4 + 3);
            }

            if ((bitmask & 0x0080) != 0)
            {
                SkipValues(4 + 12);
            }

            if ((bitmask & 0x0100) != 0)
            {
                ParseValues<UInt16>();
            }

            if ((bitmask & 0x0200) != 0)
            {
                SkipValues(4 + 8);
            }

            if ((bitmask & 0x0400) != 0)
            {
                ParseValues<UInt32>();
            }

            if ((bitmask & 0x0800) != 0)
            {
                SkipValues(4 + 16);
            }

            if ((bitmask & 0x1000) != 0)
            {
                ParseValues<string>();
            }
        }

        public int getIntValue(string v1, string v2)
        {
            return GetValue<int>(v1, v2);
        }
        public bool getBoolValue(int keyHash)
        {
            return GetValue<bool>((uint)keyHash);
        }
        public bool getBoolValue(string v1, string v2)
        {
            return GetValue<bool>(v1, v2);
        }

        public string getStringValue(string v1, string v2)
        {
            return GetValue<string>(v1, v2);
        }

        public float getFloatValue(string v1, string v2)
        {
            return GetValue<float>(v1, v2);
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }
        }

        private void AddValue<T>(UInt32 key, T value)
        {
            if (!Values.ContainsKey(key))
            {
                Values.Add(key, value);
                //Debug.WriteLine("{0} [{1}] = {2}", typeof(T).Name, HashToName(key), value);
            }
        }

        public string HashToName(UInt32 key)
        {
            var data = new Dictionary<string, string>
            {
                #region DATA
                { "3098836810", "AbilityPowerIncPerLevel"},
                { "2874621616", "AcquisitionRange"},
                {    "3901541191", "AllowPetControl"},
                {    "208409411", "AlwaysVisible"},
                {    "2599053023", "Armor"},
                {    "3310611270", "ArmorMaterial"},
                {    "1608827366", "ArmorPerLevel"},
                {    "2539498542", "AssetCategory"},
                {    "3373177976", "AttackDelayCastOffsetPercent"},
                {    "3136453924", "AttackDelayCastOffsetPercentAttackSpeedRatio"},
                {    "2191293239", "AttackDelayOffsetPercent"},
                {    "1387461685", "AttackRange"},
                {    "1276447327", "AttackSpeed"},
                {    "770205030", "AttackSpeedPerLevel"},
                {    "508752583", "BackgroundTexBotU"},
                {    "508752584", "BackgroundTexBotV"},
                {    "2202949562", "BackgroundTexChaosOffset"},
                {    "2403770876", "BackgroundTexU"},
                {    "2403770877", "BackgroundTexV"},
                {    "3959681723", "Badge0TexU"},
                {    "3959681724", "Badge0TexV"},
                {    "86628237", "Badge0_ID"},
                {    "804278716", "Badge1TexU"},
                {    "804278717", "Badge1TexV"},
                {    "867215436", "Badge1_ID"},
                {    "1943843005", "Badge2TexU"},
                {    "1943843006", "Badge2TexV"},
                {    "1647802635", "Badge2_ID"},
                {    "3083407294", "Badge3TexU"},
                {    "3083407295", "Badge3TexV"},
                {    "2428389834", "Badge3_ID"},
                {    "2805314015", "BadgeBorderTexHeight"},
                {    "2446934749", "BadgeBorderTexU"},
                {    "2446934750", "BadgeBorderTexV"},
                {    "2067516814", "BadgeBorderTexWidth"},
                {    "693635755", "BadgeTexHeight"},
                {    "486179266", "BadgeTexWidth"},
                {    "79747866", "BarTextUVs0u1"},
                {    "79747867", "BarTextUVs0u2"},
                {    "79813465", "BarTextUVs0v1"},
                {    "79813466", "BarTextUVs0v2"},
                {    "88009371", "BarTextUVs1u1"},
                {    "88009372", "BarTextUVs1u2"},
                {    "88074970", "BarTextUVs1v1"},
                {    "88074971", "BarTextUVs1v2"},
                {    "96270876", "BarTextUVs2u1"},
                {    "96270877", "BarTextUVs2u2"},
                {    "96336475", "BarTextUVs2v1"},
                {    "96336476", "BarTextUVs2v2"},
                {    "104532381", "BarTextUVs3u1"},
                {    "104532382", "BarTextUVs3u2"},
                {    "104597980", "BarTextUVs3v1"},
                {    "104597981", "BarTextUVs3v2"},
                {    "214741516", "BaseAbilityPower"},
                {    "2157022671", "BaseAttack_Probability"},
                {    "932983135", "BaseCritChance"},
                {    "1880118880", "BaseDamage"},
                {    "2534038438", "BaseDodge"},
                {    "3891320309", "BaseFactorHPRegen"},
                {    "382172794", "BaseFactorMPRegen"},
                {    "742042233", "BaseHP"},
                {    "742370228", "BaseMP"},
                {    "4128291318", "BaseStaticHPRegen"},
                {    "619143803", "BaseStaticMPRegen"},
                {    "26371053", "CanBeDropped"},
                {    "4232026919", "CanBeSold"},
                {    "1703451545", "CardBarHeightFactor"},
                {    "2825652914", "CardBarWidthFactor"},
                {    "1539512836", "CardBarXFactor"},
                {    "837683333", "CardBarYFactor"},
                {    "3185543037", "CardHeightFixed"},
                {    "3591980094", "CardWidthFixed"},
                {    "1624429865", "ChampionBoxHeight"},
                {    "2293184452", "ChampionBoxWidth"},
                {    "4275086102", "ChampionBoxX"},
                {    "4275086103", "ChampionBoxY"},
                {    "2026370171", "ChampionNameHeight"},
                {    "792166683", "ChampionNameHeightForLongString"},
                {    "3527164743", "ChampionNameOffset"},
                {    "3906052348", "ChampionTextBoxHeight"},
                {    "211896113", "ChampionTextBoxWidth"},
                {    "2520399747", "ChampionTextBoxX"},
                {    "2520399748", "ChampionTextBoxY"},
                {    "1190673158", "Classification"},
                {    "2990530837", "ClearUndoHistoryOnActivate"},
                {    "822131234", "Clickable"},
                {    "2656602888", "Consumed"},
                {    "2669481272", "CooldownShowDisabledDuration"},
                {    "1637964898", "CritAttack"},
                {    "2491447989", "CritAttack_AttackDelayCastOffsetPercent"},
                {    "517387892", "CritAttack_AttackDelayOffsetPercent"},
                {    "3607943992", "CritAttack_Probability"},
                {    "4083598422", "CritDamageBonus"},
                {    "1109881825", "CritPerLevel"},
                {    "788422759", "CriticalAttack"},
                {    "1139868982", "DamagePerLevel"},
                {    "1693881287", "DeathEventListeningRadius"},
                {    "32074497", "DeathTime"},
                {    "3587157281", "DebugNumberOnChoas"},
                {    "3502350099", "DebugNumberOnOrder"},
                {    "4030339333", "DebugPing"},
                {    "2378920944", "DelayCastOffsetPercent"},
                {    "2817470807", "DelayTotalTimePercent"},
                {    "3747042364", "Description"},
                {    "1209487289", "DisableAggroIndicator"},
                {    "993917218", "DisappersOnDeath"},
                {    "2120726765", "DisplayName"},
                {    "4232185616", "DrawParLikeHealth"},
                {    "4016718705", "DropsOnDeath"},
                {    "81025252", "DynamicTooltip"},
                {    "1061606915", "EffectRadius"},
                {    "1292300827", "EnemyTooltip"},
                {    "3278952700", "Epicness"},
                {    "2525953397", "ExpGivenOnDeath"},
                {    "845596732", "ExperienceRadius"},
                {    "1578136857", "ExtraAttack1"},
                {    "1229189310", "ExtraAttack1_AttackDelayCastOffsetPercent"},
                {    "1585491695", "ExtraAttack1_Probability"},
                {    "1578136858", "ExtraAttack2"},
                {    "3495925757", "ExtraAttack2_AttackDelayCastOffsetPercent"},
                {    "721996784", "ExtraAttack2_Probability"},
                {    "1578136859", "ExtraAttack3"},
                {    "1467694908", "ExtraAttack3_AttackDelayCastOffsetPercent"},
                {    "4153469169", "ExtraAttack3_Probability"},
                {    "1578136860", "ExtraAttack4"},
                {    "3289974258", "ExtraAttack4_Probability"},
                {    "1578136861", "ExtraAttack5"},
                {    "2426479347", "ExtraAttack5_Probability"},
                {    "1578136862", "ExtraAttack6"},
                {    "1562984436", "ExtraAttack6_Probability"},
                {    "1578136863", "ExtraAttack7"},
                {    "699489525", "ExtraAttack7_Probability"},
                {    "1578136864", "ExtraAttack8"},
                {    "4130961910", "ExtraAttack8_Probability"},
                {    "513832703", "ExtraCritAttack1"},
                {    "456356440", "ExtraCritAttack1_AttackDelayCastOffsetPercent"},
                {    "513832704", "ExtraCritAttack2"},
                {    "513832705", "ExtraCritAttack3"},
                {    "3170278617", "ExtraSpell1"},
                {    "495557015", "ExtraSpell10"},
                {    "3170278618", "ExtraSpell2"},
                {    "3170278619", "ExtraSpell3"},
                {    "3170278620", "ExtraSpell4"},
                {    "3170278621", "ExtraSpell5"},
                {    "3170278622", "ExtraSpell6"},
                {    "3170278623", "ExtraSpell7"},
                {    "3170278624", "ExtraSpell8"},
                {    "3170278625", "ExtraSpell9"},
                {    "2539116757", "FireworksEnabled"},
                {    "2125415132", "FlatArmorMod"},
                {    "2039572956", "FlatAttackSpeedMod"},
                {    "3756168283", "FlatCritChanceMod"},
                {    "921972736", "FlatCritDamageMod"},
                {    "2711130020", "FlatDodgeMod"},
                {    "2138837669", "FlatEnergyPoolMod"},
                {    "4217354982", "FlatEnergyRegenMod"},
                {    "1993475205", "FlatHPPoolMod"},
                {    "3412476166", "FlatHPRegenMod"},
                {    "2522004106", "FlatMPPoolMod"},
                {    "1108872257", "FlatMPRegenMod"},
                {    "3524488927", "FlatMagicDamageMod"},
                {    "345123907", "FlatMovementSpeedMod"},
                {    "899933219", "FlatPhysicalDamageMod"},
                {    "1673461700", "FlatSpellBlockMod"},
                {    "1441402424", "FriendlyTooltip"},
                {    "553829182", "GameplayCollisionRadius"},
                {    "1735633266", "GlobalExpGivenOnDeath"},
                {    "3156874011", "GlobalGoldGivenOnDeath"},
                {    "2816917464", "GoldGivenOnDeath"},
                {    "3306821199", "HPPerLevel"},
                {    "3062102972", "HPRegenPerLevel"},
                {    "1526371557", "HitFxScale"},
                {    "3511130277", "HoverIndicatorRadius"},
                {    "39255091", "HoverIndicatorTextureName"},
                {    "3877008438", "HoverLineIndicatorBaseTextureName"},
                {    "2642185782", "HoverLineIndicatorTargetTextureName"},
                {    "4090549479", "HoverLineIndicatorWidth"},
                {    "3418001179", "Image"},
                {    "1026566370", "ImageHeight"},
                {    "2544133579", "ImageWidth"},
                {    "1169232412", "InStore"},
                {    "4278916085", "InventoryIcon"},
                {    "875316938", "InventorySlotMax"},
                {    "875841720", "InventorySlotMin"},
                {    "2256069577", "IsElite"},
                {    "136069519", "IsEpic"},
                {    "3040737536", "IsImportantBotTarget"},
                {    "1637306730", "IsMelee"},
                {    "600496568", "IsRecipe"},
                {    "339749374", "ItemCalloutPlayer"},
                {    "1893001906", "ItemCalloutSpectator"},
                {    "2703868165", "ItemClass"},
                {    "3471506188", "ItemGroup"},
                {    "1250849294", "ItemId"},
                {    "401384653", "ItemType"},
                {    "1858072851", "LevelDodge"},
                {    "2962400469", "LiveUpdate"},
                {    "2115135178", "LocalExpGivenOnDeath"},
                {    "177384387", "LocalGoldGivenOnDeath"},
                {    "4024919097", "LocalGoldSplitWithLastHitter"},
                {    "3382624560", "LongChampionName"},
                {    "4243215483", "Lore1"},
                {    "1003217290", "MPPerLevel"},
                {    "1248483905", "MPRegenPerLevel"},
                {    "644823045", "MaxGroupOwnable"},
                {    "4250008563", "MaxLevels"},
                {    "4026184356", "MaxStack"},
                {    "3179074558", "MessageOffsetX"},
                {    "3179074559", "MessageOffsetY"},
                {    "1090138575", "Metadata"},
                {    "4126320645", "MonsterDataTableId"},
                {    "1081768566", "MoveSpeed"},
                {    "82690155", "Name"},
                {    "1669208706", "NeverRender"},
                {    "652318104", "NoAutoAttack"},
                {    "3930438518", "NoHealthBar"},
                {    "578740410", "OccludedUnitSelectableDistance"},
                {    "842279458", "PARColor"},
                {    "3601295376", "PARDisplayThroughDeath"},
                {    "2980684070", "PARFadeColor"},
                {    "166202801", "PARHasRegenText"},
                {    "3249029285", "PARIncrements"},
                {    "1792968387", "PARMaxSegments"},
                {    "1791913949", "PARNameString"},
                {    "3248153339", "PARType"},
                {    "743602011", "PassLev1Desc1"},
                {    "743602012", "PassLev1Desc2"},
                {    "743602013", "PassLev1Desc3"},
                {    "743602014", "PassLev1Desc4"},
                {    "743602015", "PassLev1Desc5"},
                {    "4219300475", "Passive1Desc"},
                {    "3810483779", "Passive1Icon"},
                {    "3126432503", "Passive1Level1"},
                {    "3126432504", "Passive1Level2"},
                {    "3126432505", "Passive1Level3"},
                {    "3126432506", "Passive1Level4"},
                {    "3126432507", "Passive1Level5"},
                {    "3126432508", "Passive1Level6"},
                {    "3706924793", "Passive1LuaName"},
                {    "3401798261", "Passive1Name"},
                {    "2424603000", "Passive2Level1"},
                {    "2424603001", "Passive2Level2"},
                {    "2424603002", "Passive2Level3"},
                {    "2424603003", "Passive2Level4"},
                {    "1722773497", "Passive3Level1"},
                {    "1020943994", "Passive4Level1"},
                {    "1692020608", "PathfindingCollisionRadius"},
                {    "2684247432", "PercentArmorMod"},
                {    "4117873288", "PercentAttackSpeedMod"},
                {    "2196017170", "PercentCooldownMod"},
                {    "1761956015", "PercentCritChanceMod"},
                {    "3222727764", "PercentCritDamageMod"},
                {    "1975167751", "PercentEXPBonus"},
                {    "3287651545", "PercentHPPoolMod"},
                {    "1467663794", "PercentHPRegenMod"},
                {    "1581007476", "PercentLifeStealMod"},
                {    "3459027181", "PercentMPRegenMod"},
                {    "1307821963", "PercentMagicDamageMod"},
                {    "1564213279", "PercentMagicPenetrationMod"},
                {    "1249439471", "PercentMovementSpeedMod"},
                {    "1008323703", "PercentPhysicalDamageMod"},
                {    "1822088946", "PercentSlowResistMod"},
                {    "3974216728", "PercentSpellBlockMod"},
                {    "1675658961", "PercentSpellVampMod"},
                {    "3153205083", "PercentTenacityItemMod"},
                {    "2723463672", "PercentageCompleteTextX"},
                {    "2723463673", "PercentageCompleteTextY"},
                {    "1786878869", "PercentageMarginBetweenCardsX"},
                {    "2024367862", "PercentageOfScreenBottomMargin"},
                {    "3215939296", "PercentageOfScreenMiddleMargin"},
                {    "2252855462", "PercentageOfScreenTopMargin"},
                {    "1814941155", "PerceptionBubbleRadius"},
                {    "613264201", "PingBarMarginX"},
                {    "613264202", "PingBarMarginY"},
                {    "1432410575", "PingBoxHeightFactor"},
                {    "611479709", "PingBoxOffsetFactorX"},
                {    "611479710", "PingBoxOffsetFactorY"},
                {    "378724092", "PingBoxWidthFactor"},
                {    "2478814953", "Price"},
                {    "2307580382", "ProgressTextX"},
                {    "2307580383", "ProgressTextY"},
                {    "2987621955", "ProgressbarBorderTexBotU"},
                {    "2987621956", "ProgressbarBorderTexBotV"},
                {    "396567870", "ProgressbarBorderTexChaosOffset"},
                {    "1358625152", "ProgressbarBorderTexU"},
                {    "1358625153", "ProgressbarBorderTexV"},
                {    "3799183", "ProgressbarTexBotU"},
                {    "3799184", "ProgressbarTexBotV"},
                {    "2213736434", "ProgressbarTexChaosOffset"},
                {    "2505590324", "ProgressbarTexU"},
                {    "2505590325", "ProgressbarTexV"},
                {    "398582509", "RankingsTextureAtlas"},
                {    "2487692474", "RankingsTextureX"},
                {    "2487692475", "RankingsTextureY"},
                {    "973644272", "RecipeItem1"},
                {    "973644273", "RecipeItem2"},
                {    "973644274", "RecipeItem3"},
                {    "973644275", "RecipeItem4"},
                {    "3980723528", "RequiredChampion"},
                {    "2525788959", "RequiredItem1"},
                {    "2965529285", "RequiredLevel"},
                {    "9523123", "SelectionHeight"},
                {    "3875712670", "SelectionRadius"},
                {    "3760373072", "SellBackModifier"},
                {    "358249999", "ServerOnly"},
                {    "2600383393", "ShouldFaceTarget"},
                {    "2600205891", "Significance"},
                {    "3685745599", "SkipDrawOutline"},
                {    "3301149543", "SpecialRecipe"},
                {    "404599689", "Spell1"},
                {    "404599690", "Spell2"},
                {    "404599691", "Spell3"},
                {    "404599692", "Spell4"},
                {    "1395891205", "SpellBlock"},
                {    "4032178956", "SpellBlockPerLevel"},
                {    "1796924679", "SpellDelayCastTime"},
                {    "1068117654", "SpellDelayTotalTime"},
                {    "4262394835", "SpellName"},
                {    "4272388092", "SpellSupLevels1"},
                {    "4272388093", "SpellSupLevels2"},
                {    "4272388094", "SpellSupLevels3"},
                {    "4272388095", "SpellSupLevels4"},
                {    "1884215744", "SummonerNameHeight"},
                {    "3385010316", "SummonerNameOffset"},
                {    "2304595374", "SummonorBoxHeight"},
                {    "459490815", "SummonorBoxWidth"},
                {    "930332877", "SummonorSpellBox2X"},
                {    "1047386801", "SummonorSpellBoxX"},
                {    "1047386802", "SummonorSpellBoxY"},
                {    "3458402794", "TeamsTextHeight"},
                {    "1778386361", "TextWidth"},
                {    "2077534274", "TextureHeight"},
                {    "2153672870", "TextureName"},
                {    "3674869355", "TextureWidth"},
                {    "955678904", "Textures"},
                {    "70667385", "Tips1"},
                {    "70667386", "Tips2"},
                {    "3460802284", "TipsTextHeight"},
                {    "297772983", "TowerTargetingPriorityBoost"},
                {    "883270180", "UsableInStore"},
                {    "3582713954", "UseChampionVisibility"},
                {    "2519496612", "UseSkinNames"},
                {    "3803087215", "UseWhenAcquired"},
                {    "3512150540", "VsBorderTexBotU"},
                {    "3512150541", "VsBorderTexBotV"},
                {    "1103632119", "VsBorderTexU"},
                {    "1103632120", "VsBorderTexV"},
                {    "888794131", "VsHeightFactor"},
                {    "1044183992", "VsWidthFactor"},
                {    "3075090915", "WeaponMaterial"},
                {    "1159941902", "WeaponMaterial1"},
                {    "1159941903", "WeaponMaterial2"},
                {    "1159941904", "WeaponMaterial3"},
                {    "3764747859", "blackboxheight"},
                {    "2366481434", "blackboxwidth"},
                {    "3783861612", "blackboxx"},
                {    "3783861613", "blackboxy"},
                {    "1109998426", "championcardheightpercentage"},
                {    "3144725991", "championcardhorizontallayout"},
                {    "4110901169", "chaoscardheightpercentage"},
                {    "4152970224", "chaoscardwidthpercentage"},
                {    "1570053186", "chaoscardxpercentage"},
                {    "3291343299", "chaoscardypercentage"},
                {    "3562272462", "flatBlockMod"},
                {    "1416335451", "flatEXPBonus"},
                {    "1454604501", "hideFromAll"},
                {    "2414666880", "imagePath"},
                {    "3425214783", "ordercardheightpercentage"},
                {    "2268645090", "ordercardwidthpercentage"},
                {    "4043229492", "ordercardxpercentage"},
                {    "1469552309", "ordercardypercentage"},
                {    "1220007731", "passive1range"},
                {    "4121104762", "percentBlockMod"},
                {    "3269962320", "percentDodgeMod"},
                {    "3816180446", "percentMPPoolMod"},
                {    "2466060486", "percentagecompletenopingtexty"},
                {    "2684441592", "profileiconheightfactor"},
                {    "4229529780", "profileiconoffsetfactorx"},
                {    "4229529781", "profileiconoffsetfactory"},
                {    "2346994323", "profileiconwidthfactor"},
                {    "2237961173", "rFlatArmorModPerLevel"},
                {    "1666342795", "rFlatArmorPenetrationMod"},
                {    "1604870802", "rFlatArmorPenetrationModPerLevel"},
                {    "600405488", "rFlatCritChanceModPerLevel"},
                {    "3195832213", "rFlatCritDamageModPerLevel"},
                {    "250160022", "rFlatDodgeMod"},
                {    "651317405", "rFlatDodgeModPerLevel"},
                {    "1919127670", "rFlatEnergyModPerLevel"},
                {    "2827316447", "rFlatEnergyRegenModPerLevel"},
                {    "3596459249", "rFlatGoldPer10Mod"},
                {    "2503536726", "rFlatHPModPerLevel"},
                {    "3470517503", "rFlatHPRegenModPerLevel"},
                {    "2481029467", "rFlatMPModPerLevel"},
                {    "3022420538", "rFlatMPRegenModPerLevel"},
                {    "2904354520", "rFlatMagicDamageModPerLevel"},
                {    "1105291097", "rFlatMagicPenetrationMod"},
                {    "4078980704", "rFlatMagicPenetrationModPerLevel"},
                {    "4241001276", "rFlatMovementSpeedModPerLevel"},
                {    "3408390072", "rFlatPhysicalDamageModPerLevel"},
                {    "1201590105", "rFlatSpellBlockModPerLevel"},
                {    "2152238374", "rFlatTimeDeadMod"},
                {    "1738185261", "rFlatTimeDeadModPerLevel"},
                {    "1489771843", "rPercentArmorPenetrationMod"},
                {    "3739791434", "rPercentArmorPenetrationModPerLevel"},
                {    "1289862173", "rPercentAttackSpeedModPerLevel"},
                {    "331225348", "rPercentCooldownMod"},
                {    "3683445259", "rPercentCooldownModPerLevel"},
                {    "928720145", "rPercentMagicPenetrationMod"},
                {    "1918934040", "rPercentMagicPenetrationModPerLevel"},
                {    "4064342916", "rPercentMovementSpeedModPerLevel"},
                {    "450607838", "rPercentTimeDeadMod"},
                {    "1561614309", "rPercentTimeDeadModPerLevel"}
                #endregion
            };

            if (data.ContainsKey(key.ToString()))
                return "Data/" + data[key.ToString()];

            var buffData = new Dictionary<string, string>
            {
               #region BUFF DATA
               { "1378003864", "AlternateName"},
               { "3966094059", "DynamicExtended"},
               { "2287725201", "DynamicTooltip"},
               { "4279592820", "FloatVarsDecimals1"},
               { "4279592821", "FloatVarsDecimals2"},
               { "4279592822", "FloatVarsDecimals3"},
               { "1456232220", "HideDurationInUI"}
               #endregion
            };

            if (buffData.ContainsKey(key.ToString()))
                return "BuffData/" + buffData[key.ToString()];

            var spellData = new Dictionary<string, string>
            {
                #region SPELL DATA
                {"706575799", "AIBlockLevel"},
                {"3998561799", "AIEndOnly"},
                {"3194240841", "AILifetime"},
                {"1385253074", "AIRadius"},
                {"1898334813", "AIRange"},
                {"1285857298", "AISendEvent"},
                {"1787320455", "AISpeed"},
                {"1201281824", "AfterEffectName"},
                {"3091373677", "AlternateName"},
                {"1543787083", "AlwaysSnapFacing"},
                {"3171254284", "AmmoCountHiddenInUI"},
                {"1279259354", "AmmoRechargeTime"},
                {"253543011", "AmmoUsed"},
                {"2105364273", "AnimationLeadOutName"},
                {"3568019419", "AnimationLoopName"},
                {"1746694807", "AnimationName"},
                {"4021987841", "AnimationWinddownName"},
                {"3690686093", "ApplyAttackDamage"},
                {"34220719", "ApplyAttackEffect"},
                {"3494014840", "ApplyMaterialOnHitSound"},
                {"3776584848", "AttackDelayCastOffsetPercent"},
                {"367590224", "BelongsToAvatar"},
                {"4045688210", "BounceRadius"},
                {"1931495254", "CanCastWhileDisabled"},
                {"3135080359", "CanMoveWhileChanneling"},
                {"3398560978", "CanOnlyCastWhileDead"},
                {"1514199818", "CanOnlyCastWhileDisabled"},
                {"721561220", "CancelChargeOnRecastTime"},
                {"1429106066", "CannotBeSuppressed"},
                {"2152518234", "CantCancelWhileChanneling"},
                {"3109850202", "CantCancelWhileWindingUp"},
                {"2337070487", "CantCastWhileRooted"},
                {"2315647161", "CastConeAngle"},
                {"2745783535", "CastConeDistance"},
                {"3295010166", "CastFrame"},
                {"1133901961", "CastRadius"},
                {"2786735659", "CastRadiusSecondary"},
                {"4156942032", "CastRadiusSecondaryTexture"},
                {"3401378034", "CastRadiusTexture"},
                {"3806805222", "CastRange"},
                {"332266699", "CastRange1"},
                {"332266700", "CastRange2"},
                {"332266701", "CastRange3"},
                {"332266702", "CastRange4"},
                {"332266703", "CastRange5"},
                {"332266704", "CastRange6"},
                {"2530870824", "CastRangeDisplayOverride"},
                {"138180769", "CastRangeGrowthDuration"},
                {"716901527", "CastRangeGrowthMax"},
                {"1552191756", "CastRangeTextureOverrideName"},
                {"3950119348", "CastRangeUseBoundingBoxes"},
                {"2202397586", "CastTargetAdditionalUnitsRadius"},
                {"4166887761", "CastType"},
                {"3891561855", "ChannelDuration"},
                {"2594953842", "ChannelDuration1"},
                {"2594953843", "ChannelDuration2"},
                {"2594953844", "ChannelDuration3"},
                {"2594953845", "ChannelDuration4"},
                {"2594953846", "ChannelDuration5"},
                {"2594953847", "ChannelDuration6"},
                {"460407770", "ChargeUpdateInterval"},
                {"497773249", "CircleMissileAngularVelocity"},
                {"83961306", "CircleMissileRadialVelocity"},
                {"2309212247", "ClientOnlyMissileTargetBoneName"},
                {"844968125", "Coefficient"},
                {"2511077045", "Coefficient2"},
                {"1229951523", "Cooldown"},
                {"2629301966", "Cooldown1"},
                {"2629301967", "Cooldown2"},
                {"2629301968", "Cooldown3"},
                {"2629301969", "Cooldown4"},
                {"2629301970", "Cooldown5"},
                {"2629301971", "Cooldown6"},
                {"4221236252", "CursorChangesInGrass"},
                {"248468475", "CursorChangesInTerrain"},
                {"2694601223", "DeathRecapPriority"},
                {"2965805064", "DelayCastOffsetPercent"},
                {"3144779583", "DelayTotalTimePercent"},
                {"3431853604", "Description"},
                {"1116192868", "DisableCastBar"},
                {"1805538005", "DisplayName"},
                {"3029929007", "DoNotNeedToFaceTarget"},
                {"3501163192", "DoesntBreakChannels"},
                {"3954594563", "DrawSecondaryLineIndicator"},
                {"3331302208", "DynamicExtended"},
                {"2634919164", "DynamicTooltip"},
                {"1168646476", "Effect1Level0Amount"},
                {"466816973", "Effect1Level1Amount"},
                {"4059954766", "Effect1Level2Amount"},
                {"3358125263", "Effect1Level3Amount"},
                {"2656295760", "Effect1Level4Amount"},
                {"1954466257", "Effect1Level5Amount"},
                {"1252636754", "Effect1Level6Amount"},
                {"305151565", "Effect2Level0Amount"},
                {"3898289358", "Effect2Level1Amount"},
                {"3196459855", "Effect2Level2Amount"},
                {"2494630352", "Effect2Level3Amount"},
                {"1792800849", "Effect2Level4Amount"},
                {"1090971346", "Effect2Level5Amount"},
                {"389141843", "Effect2Level6Amount"},
                {"3736623950", "Effect3Level0Amount"},
                {"3034794447", "Effect3Level1Amount"},
                {"2332964944", "Effect3Level2Amount"},
                {"1631135441", "Effect3Level3Amount"},
                {"929305938", "Effect3Level4Amount"},
                {"227476435", "Effect3Level5Amount"},
                {"3820614228", "Effect3Level6Amount"},
                {"2873129039", "Effect4Level0Amount"},
                {"2171299536", "Effect4Level1Amount"},
                {"1469470033", "Effect4Level2Amount"},
                {"767640530", "Effect4Level3Amount"},
                {"65811027", "Effect4Level4Amount"},
                {"3658948820", "Effect4Level5Amount"},
                {"2957119317", "Effect4Level6Amount"},
                {"2009634128", "Effect5Level0Amount"},
                {"1307804625", "Effect5Level1Amount"},
                {"605975122", "Effect5Level2Amount"},
                {"4199112915", "Effect5Level3Amount"},
                {"3497283412", "Effect5Level4Amount"},
                {"2795453909", "Effect5Level5Amount"},
                {"2093624406", "Effect5Level6Amount"},
                {"3036452687", "Flags"},
                {"2964659502", "FloatStaticsDecimals1"},
                {"2964659503", "FloatStaticsDecimals2"},
                {"2964659504", "FloatStaticsDecimals3"},
                {"2964659505", "FloatStaticsDecimals4"},
                {"2964659506", "FloatStaticsDecimals5"},
                {"2964659507", "FloatStaticsDecimals6"},
                {"1183899359", "FloatVarsDecimals1"},
                {"1183899360", "FloatVarsDecimals2"},
                {"1183899361", "FloatVarsDecimals3"},
                {"1183899362", "FloatVarsDecimals4"},
                {"1183899363", "FloatVarsDecimals5"},
                {"1183899364", "FloatVarsDecimals6"},
                {"2256047629", "HaveAfterEffect"},
                {"3498283767", "HaveHitBone"},
                {"2296243972", "HaveHitEffect"},
                {"458448769", "HavePointEffect"},
                {"305076829", "HideRangeIndicatorWhenCasting"},
                {"1114496522", "HitBoneName"},
                {"91820311", "HitEffectName"},
                {"549024747", "HitEffectOrientType"},
                {"266163128", "HitEffectPlayerName"},
                {"3839803762", "IgnoreAnimContinueUntilCastFrame"},
                {"354358773", "IgnoreRangeCheck"},
                {"2059614685", "InventoryIcon"},
                {"1877491092", "InventoryIcon1"},
                {"1877491093", "InventoryIcon2"},
                {"1778900631", "IsDisabledWhileDead"},
                {"3678517266", "IsToggleSpell"},
                {"1994073625", "KeywordWhenAcquired"},
                {"3848571446", "Level1Desc"},
                {"693168439", "Level2Desc"},
                {"1832732728", "Level3Desc"},
                {"2972297017", "Level4Desc"},
                {"4111861306", "Level5Desc"},
                {"956458299", "Level6Desc"},
                {"2037024646", "LineDragLength"},
                {"2390697809", "LineMissileBounces"},
                {"307233808", "LineMissileCollisionFromStartPoint"},
                {"2196893704", "LineMissileDelayDestroyAtEndSeconds"},
                {"2190377914", "LineMissileEndsAtTargetPoint"},
                {"2795922292", "LineMissileFollowsTerrainHeight"},
                {"4058360511", "LineMissileTargetHeightAugment"},
                {"1880583852", "LineMissileTimePulseBetweenCollisionSpellHits"},
                {"4279387294", "LineMissileTrackUnits"},
                {"1402697195", "LineMissileUsesAccelerationForBounce"},
                {"2164699292", "LineTargetingBaseTextureOverrideName"},
                {"2339306908", "LineTargetingTargetTextureOverrideName"},
                {"2933974746", "LineWidth"},
                {"402810463", "LocationTargettingLength1"},
                {"402810464", "LocationTargettingLength2"},
                {"402810465", "LocationTargettingLength3"},
                {"402810466", "LocationTargettingLength4"},
                {"402810467", "LocationTargettingLength5"},
                {"402810468", "LocationTargettingLength6"},
                {"2513139447", "LocationTargettingWidth1"},
                {"2513139448", "LocationTargettingWidth2"},
                {"2513139449", "LocationTargettingWidth3"},
                {"2513139450", "LocationTargettingWidth4"},
                {"2513139451", "LocationTargettingWidth5"},
                {"2513139452", "LocationTargettingWidth6"},
                {"2318589698", "LockConeToPlayer"},
                {"402376892", "LookAtPolicy"},
                {"1771089066", "LuaOnMissileUpdateDistanceInterval"},
                {"3771724453", "ManaCost1"},
                {"3771724454", "ManaCost2"},
                {"3771724455", "ManaCost3"},
                {"3771724456", "ManaCost4"},
                {"3771724457", "ManaCost5"},
                {"3771724458", "ManaCost6"},
                {"1624808058", "MaxAmmo"},
                {"1422229532", "MaxGrowthRangeTextureName"},
                {"4248345958", "MinimapIcon"},
                {"2521094376", "MinimapIconDisplayFlag"},
                {"4284809348", "MinimapIconRotation"},
                {"844925530", "MissileAccel"},
                {"2295071549", "MissileBoneName"},
                {"2669813407", "MissileEffect"},
                {"284255680", "MissileEffectPlayer"},
                {"3772668461", "MissileFixedTravelTime"},
                {"3300480064", "MissileGravity"},
                {"1278325815", "MissileLifetime"},
                {"3353114481", "MissileMaxSpeed"},
                {"2608340739", "MissileMinSpeed"},
                {"1745720811", "MissileMinTravelTime"},
                {"3094849073", "MissilePerceptionBubbleRadius"},
                {"740064540", "MissilePerceptionBubbleRevealsStealth"},
                {"1456468249", "MissileSpeed"},
                {"2104552651", "MissileTargetHeightAugment"},
                {"3897239392", "MissileUnblockable"},
                {"888918147", "Name"},
                {"296189745", "NoWinddownIfCancelled"},
                {"4080107611", "NumSpellTargeters"},
                {"4132718167", "OrientRadiusTextureFromPlayer"},
                {"3797006944", "OrientRangeIndicatorToCursor"},
                {"395025284", "OrientRangeIndicatorToFacing"},
                {"2558459920", "OverrideCastTime"},
                {"143853687", "ParticleStartOffset"},
                {"2199845012", "PointEffectName"},
                {"278926620", "RangeIndicatorTextureName"},
                {"3937459631", "Ranks"},
                {"2587149551", "SelectionPreference"},
                {"2257188466", "Sound_CastName"},
                {"2893618454", "Sound_HitName"},
                {"3088263895", "Sound_VOEventCategory"},
                {"806859196", "SpellCastTime"},
                {"4058255423", "SpellRevealsChampion"},
                {"2257948449", "SpellTotalTime"},
                {"3822003669", "StartCooldown"},
                {"57583389", "SubjectToGlobalCooldown"},
                {"4247601474", "TargeterConstrainedToRange"},
                {"2637063921", "TargettingType"},
                {"1829373218", "TextFlags"},
                {"536890465", "TriggersGlobalCooldown"},
                {"2306799607", "UseAnimatorFramerate"},
                {"2970326610", "UseAutoattackCastTime"},
                {"779692194", "UseChargeChanneling"},
                {"823181198", "UseChargeTargeting"},
                {"1223170345", "UseGlobalLineIndicator"},
                {"1945155259", "UseMinimapTargeting"},
                {"3976640480", "Version"},
                {"616000785", "x1"},
                {"616000786", "x2"},
                {"616000787", "x3"},
                {"616000788", "x4"},
                {"616000789", "x5"}
                #endregion
            };

            if (spellData.ContainsKey(key.ToString()))
                return "SpellData/" + spellData[key.ToString()];

            return key + "";
        }

        private void SkipValues(int size)
        {
            var start = _reader.BaseStream.Position;
            var keys = ReadSegmentKeys();
            _reader.BaseStream.Position += keys.Length * size;
           // Debug.WriteLine("{0} properties skip from {1} to {2}", size, start, _reader.BaseStream.Position);
        }

        private void ParseValues<T>(bool isBase10 = false)
        {
            //Debug.WriteLine("{0} properties start position {1}", typeof(T).Name, _reader.BaseStream.Position);
            var keys = ReadSegmentKeys();

            if (typeof(T) == typeof(bool))
            {
                var index = 0;
                for (var i = 0; i < 1 + ((keys.Length - 1) / 8); ++i)
                {
                    int bits = ReadValue<byte>();
                    for (var b = 0; b < 8; ++b)
                    {
                        var key = keys[index];
                        var val = 0x1 & bits;
                        AddValue(key, val);
                        bits = bits >> 1;
                        if (++index == keys.Length)
                        {
                            break;
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(string))
            {
                foreach (var key in keys)
                {
                    var offset = ReadValue<UInt16>();
                    AddValue(key, ReadValue<string>(_stringOffset + offset));
                }
            }
            else
            {
                foreach (var key in keys)
                {
                    if (isBase10)
                    {
                        AddValue(key, ((byte)(object)ReadValue<T>()) * 0.1f);
                    }
                    else
                    {
                        AddValue(key, ReadValue<T>());
                    }
                }
            }

           // Debug.WriteLine("");
        }

        private T ReadValue<T>(int offset = 0)
        {
            try
            {
                if (typeof(T) == typeof(byte))
                {
                    return (T)(object)_reader.ReadByte();
                }
                if (typeof(T) == typeof(UInt16))
                {
                    return (T)(object)_reader.ReadUInt16();
                }
                if (typeof(T) == typeof(UInt32))
                {
                    return (T)(object)_reader.ReadUInt32();
                }
                if (typeof(T) == typeof(float))
                {
                    return (T)(object)_reader.ReadSingle();
                }
                if (typeof(T) == typeof(string))
                {
                    int c;
                    var sb = new StringBuilder();
                    var oldPos = _reader.BaseStream.Position;
                    _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    while ((c = _reader.ReadByte()) > 0)
                    {
                        sb.Append((char)c);
                    }
                    _reader.BaseStream.Seek(oldPos, SeekOrigin.Begin);

                    return (T)(object)sb.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default(T);
        }

        private UInt32[] ReadSegmentKeys()
        {
            var result = new UInt32[ReadValue<UInt16>()];

            for (var i = 0; i < result.Length; ++i)
            {
                result[i] = ReadValue<UInt32>();
            }

            return result;
        }

        public T GetValue<T>(string section, string name)
        {
            return GetValue<T>(Keys.GetHash(section, name));
        }

        public T GetValue<T>(UInt32 key)
        {
            try
            {
                if (!Values.ContainsKey(key))
                {
                    return default(T);
                   // throw new KeyNotFoundException(key.ToString());
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)Values[key].ToString();
                }

                // integers -> bool
                if (typeof(T) == typeof(bool) && Values[key].GetType().IsInteger())
                {
                    return (T)(object)((int)Values[key] > 0);
                }

                var value = Values[key] as string;
                if (value != null)
                {
                    // string -> bool
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)((value == "1") || (value == "Yes"));
                    }

                    if (typeof(T).IsNumeric())
                    {
                        // string -> byte/short/int
                        int intValue;
                        if (Int32.TryParse(value, out intValue))
                        {
                            return (T)Convert.ChangeType(intValue, typeof(T));
                        }

                        // string -> double/float
                        double doubleValue;
                        if (Double.TryParse(value, out doubleValue))
                        {
                            return (T)Convert.ChangeType(doubleValue, typeof(T));
                        }
                    }
                    if (value == "")
                        return default(T);
                }

                return (T)Convert.ChangeType(Values[key], typeof(T));
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine(key);
                return default(T);
            }
            catch (InvalidCastException)
            {
                Debug.WriteLine(Values[key].GetType().Name + " to " + typeof(T).Name + " @ " + key);
                return default(T);
            }
        }

        public bool KeyExists(string section, string name)
        {
            return Values.ContainsKey(Keys.GetHash(section, name));
        }

        public static class Keys
        {
            public static readonly Dictionary<UInt32, string> KnownKeys = new Dictionary<UInt32, string>();

            #region Sections

            private static readonly string[] PKeys =
            {
                "HealthBar",
                "GeneralDataHero",
                "Info",
                "Useable",
                "MeshSkin",
                "Categories",
                "SpellData",
                "BuffData",
                "Data"
            };

            #endregion

            #region Names

            private static readonly string[] PVark =
            {
                "YOffset",
                "XOffset",
                "DefaultChampionCollisionRadius",
                "GameplayCollisionRadius",
                "IconCircle",
                "IconSquare",
                "IsUseable",
                "GoldRedirectTargetUseableOnly",
                "AllyCanUse",
                "EnemyCanUse",
                "HeroUseSpell",
                "MinionUseSpell",
                "CooldownSpellSlot",
                "MinionUseable",
                "Texture",
                "SkinScale",
                "Skeleton",
                "SimpleSkin",
                "EmissiveTexture",
                "SelfIllumination",
                "Health",
                "SpellBlock",
                "HealthRegen",
                "Armor",
                "Damage",
                "CriticalStrike",
                "AttackSpeed",
                "LifeSteal",
                "SpellDamage",
                "CooldownReduction",
                "Mana",
                "ManaRegen",
                "Movement",
                "Consumable",
                "Internal",
                "Health",
                "SpellBlock",
                "HealthRegen",
                "Armor",
                "Damage",
                "CriticalStrike",
                "AttackSpeed",
                "LifeSteal",
                "SpellDamage",
                "CooldownReduction",
                "Mana",
                "ManaRegen",
                "Movement",
                "Consumable",
                "Internal",
                "AIBlockLevel",
                "AIEndOnly",
                "AILifetime",
                "AIRadius",
                "AIRange",
                "AISendEvent",
                "AISpeed",
                "AfterEffectName",
                "AlternateName",
                "AlwaysSnapFacing",
                "AmmoCountHiddenInUI",
                "AmmoRechargeTime",
                "AmmoUsed",
                "AnimationLeadOutName",
                "AnimationLoopName",
                "AnimationName",
                "AnimationWinddownName",
                "ApplyAttackDamage",
                "ApplyAttackEffect",
                "ApplyMaterialOnHitSound",
                "AttackDelayCastOffsetPercent",
                "BelongsToAvatar",
                "BounceRadius",
                "CanCastWhileDisabled",
                "CanMoveWhileChanneling",
                "CanOnlyCastWhileDead",
                "CanOnlyCastWhileDisabled",
                "CancelChargeOnRecastTime",
                "CannotBeSuppressed",
                "CantCancelWhileChanneling",
                "CantCancelWhileWindingUp",
                "CantCastWhileRooted",
                "CastConeAngle",
                "CastConeDistance",
                "CastFrame",
                "CastRadius",
                "CastRadiusSecondary",
                "CastRadiusSecondaryTexture",
                "CastRadiusTexture",
                "CastRange",
                "CastRange1",
                "CastRange2",
                "CastRange3",
                "CastRange4",
                "CastRange5",
                "CastRange6",
                "CastRangeDisplayOverride",
                "CastRangeGrowthDuration",
                "CastRangeGrowthMax",
                "CastRangeTextureOverrideName",
                "CastRangeUseBoundingBoxes",
                "CastTargetAdditionalUnitsRadius",
                "CastType",
                "ChannelDuration",
                "ChannelDuration1",
                "ChannelDuration2",
                "ChannelDuration3",
                "ChannelDuration4",
                "ChannelDuration5",
                "ChannelDuration6",
                "ChargeUpdateInterval",
                "CircleMissileAngularVelocity",
                "CircleMissileRadialVelocity",
                "ClientOnlyMissileTargetBoneName",
                "Coefficient",
                "Coefficient2",
                "Cooldown",
                "Cooldown1",
                "Cooldown2",
                "Cooldown3",
                "Cooldown4",
                "Cooldown5",
                "Cooldown6",
                "CursorChangesInGrass",
                "CursorChangesInTerrain",
                "DeathRecapPriority",
                "DelayCastOffsetPercent",
                "DelayTotalTimePercent",
                "Description",
                "DisableCastBar",
                "DisplayName",
                "DoNotNeedToFaceTarget",
                "DoesntBreakChannels",
                "DrawSecondaryLineIndicator",
                "DynamicExtended",
                "DynamicTooltip",
                "Effect1Level0Amount",
                "Effect1Level1Amount",
                "Effect1Level2Amount",
                "Effect1Level3Amount",
                "Effect1Level4Amount",
                "Effect1Level5Amount",
                "Effect1Level6Amount",
                "Effect2Level0Amount",
                "Effect2Level1Amount",
                "Effect2Level2Amount",
                "Effect2Level3Amount",
                "Effect2Level4Amount",
                "Effect2Level5Amount",
                "Effect2Level6Amount",
                "Effect3Level0Amount",
                "Effect3Level1Amount",
                "Effect3Level2Amount",
                "Effect3Level3Amount",
                "Effect3Level4Amount",
                "Effect3Level5Amount",
                "Effect3Level6Amount",
                "Effect4Level0Amount",
                "Effect4Level1Amount",
                "Effect4Level2Amount",
                "Effect4Level3Amount",
                "Effect4Level4Amount",
                "Effect4Level5Amount",
                "Effect4Level6Amount",
                "Effect5Level0Amount",
                "Effect5Level1Amount",
                "Effect5Level2Amount",
                "Effect5Level3Amount",
                "Effect5Level4Amount",
                "Effect5Level5Amount",
                "Effect5Level6Amount",
                "Flags",
                "FloatStaticsDecimals1",
                "FloatStaticsDecimals2",
                "FloatStaticsDecimals3",
                "FloatStaticsDecimals4",
                "FloatStaticsDecimals5",
                "FloatStaticsDecimals6",
                "FloatVarsDecimals1",
                "FloatVarsDecimals2",
                "FloatVarsDecimals3",
                "FloatVarsDecimals4",
                "FloatVarsDecimals5",
                "FloatVarsDecimals6",
                "HaveAfterEffect",
                "HaveHitBone",
                "HaveHitEffect",
                "HavePointEffect",
                "HideRangeIndicatorWhenCasting",
                "HitBoneName",
                "HitEffectName",
                "HitEffectOrientType",
                "HitEffectPlayerName",
                "IgnoreAnimContinueUntilCastFrame",
                "IgnoreRangeCheck",
                "InventoryIcon",
                "InventoryIcon1",
                "InventoryIcon2",
                "IsDisabledWhileDead",
                "IsToggleSpell",
                "KeywordWhenAcquired",
                "Level1Desc",
                "Level2Desc",
                "Level3Desc",
                "Level4Desc",
                "Level5Desc",
                "Level6Desc",
                "LineDragLength",
                "LineMissileBounces",
                "LineMissileCollisionFromStartPoint",
                "LineMissileDelayDestroyAtEndSeconds",
                "LineMissileEndsAtTargetPoint",
                "LineMissileFollowsTerrainHeight",
                "LineMissileTargetHeightAugment",
                "LineMissileTimePulseBetweenCollisionSpellHits",
                "LineMissileTrackUnits",
                "LineMissileUsesAccelerationForBounce",
                "LineTargetingBaseTextureOverrideName",
                "LineTargetingTargetTextureOverrideName",
                "LineWidth",
                "LocationTargettingLength1",
                "LocationTargettingLength2",
                "LocationTargettingLength3",
                "LocationTargettingLength4",
                "LocationTargettingLength5",
                "LocationTargettingLength6",
                "LocationTargettingWidth1",
                "LocationTargettingWidth2",
                "LocationTargettingWidth3",
                "LocationTargettingWidth4",
                "LocationTargettingWidth5",
                "LocationTargettingWidth6",
                "LockConeToPlayer",
                "LookAtPolicy",
                "LuaOnMissileUpdateDistanceInterval",
                "ManaCost1",
                "ManaCost2",
                "ManaCost3",
                "ManaCost4",
                "ManaCost5",
                "ManaCost6",
                "MaxAmmo",
                "MaxGrowthRangeTextureName",
                "MinimapIcon",
                "MinimapIconDisplayFlag",
                "MinimapIconRotation",
                "MissileAccel",
                "MissileBoneName",
                "MissileEffect",
                "MissileEffectPlayer",
                "MissileFixedTravelTime",
                "MissileGravity",
                "MissileLifetime",
                "MissileMaxSpeed",
                "MissileMinSpeed",
                "MissileMinTravelTime",
                "MissilePerceptionBubbleRadius",
                "MissilePerceptionBubbleRevealsStealth",
                "MissileSpeed",
                "MissileTargetHeightAugment",
                "MissileUnblockable",
                "Name",
                "NoWinddownIfCancelled",
                "NumSpellTargeters",
                "OrientRadiusTextureFromPlayer",
                "OrientRangeIndicatorToCursor",
                "OrientRangeIndicatorToFacing",
                "OverrideCastTime",
                "ParticleStartOffset",
                "PointEffectName",
                "RangeIndicatorTextureName",
                "Ranks",
                "SelectionPreference",
                "Sound_CastName",
                "Sound_HitName",
                "Sound_VOEventCategory",
                "SpellCastTime",
                "SpellRevealsChampion",
                "SpellTotalTime",
                "StartCooldown",
                "SubjectToGlobalCooldown",
                "TargeterConstrainedToRange",
                "TargettingType",
                "TextFlags",
                "TriggersGlobalCooldown",
                "UseAnimatorFramerate",
                "UseAutoattackCastTime",
                "UseChargeChanneling",
                "UseChargeTargeting",
                "UseGlobalLineIndicator",
                "UseMinimapTargeting",
                "Version",
                "x1",
                "x2",
                "x3",
                "x4",
                "x5",
                "AlternateName",
                "DynamicExtended",
                "DynamicTooltip",
                "FloatVarsDecimals1",
                "FloatVarsDecimals2",
                "FloatVarsDecimals3",
                "HideDurationInUI",
                "AbilityPowerIncPerLevel",
                "AcquisitionRange",
                "AllowPetControl",
                "AlwaysVisible",
                "Armor",
                "ArmorMaterial",
                "ArmorPerLevel",
                "AssetCategory",
                "AttackDelayCastOffsetPercent",
                "AttackDelayCastOffsetPercentAttackSpeedRatio",
                "AttackDelayOffsetPercent",
                "AttackRange",
                "AttackSpeed",
                "AttackSpeedPerLevel",
                "BackgroundTexBotU",
                "BackgroundTexBotV",
                "BackgroundTexChaosOffset",
                "BackgroundTexU",
                "BackgroundTexV",
                "Badge0TexU",
                "Badge0TexV",
                "Badge0_ID",
                "Badge1TexU",
                "Badge1TexV",
                "Badge1_ID",
                "Badge2TexU",
                "Badge2TexV",
                "Badge2_ID",
                "Badge3TexU",
                "Badge3TexV",
                "Badge3_ID",
                "BadgeBorderTexHeight",
                "BadgeBorderTexU",
                "BadgeBorderTexV",
                "BadgeBorderTexWidth",
                "BadgeTexHeight",
                "BadgeTexWidth",
                "BarTextUVs0u1",
                "BarTextUVs0u2",
                "BarTextUVs0v1",
                "BarTextUVs0v2",
                "BarTextUVs1u1",
                "BarTextUVs1u2",
                "BarTextUVs1v1",
                "BarTextUVs1v2",
                "BarTextUVs2u1",
                "BarTextUVs2u2",
                "BarTextUVs2v1",
                "BarTextUVs2v2",
                "BarTextUVs3u1",
                "BarTextUVs3u2",
                "BarTextUVs3v1",
                "BarTextUVs3v2",
                "BaseAbilityPower",
                "BaseAttack_Probability",
                "BaseCritChance",
                "BaseDamage",
                "BaseDodge",
                "BaseFactorHPRegen",
                "BaseFactorMPRegen",
                "BaseHP",
                "BaseMP",
                "BaseStaticHPRegen",
                "BaseStaticMPRegen",
                "CanBeDropped",
                "CanBeSold",
                "CardBarHeightFactor",
                "CardBarWidthFactor",
                "CardBarXFactor",
                "CardBarYFactor",
                "CardHeightFixed",
                "CardWidthFixed",
                "ChampionBoxHeight",
                "ChampionBoxWidth",
                "ChampionBoxX",
                "ChampionBoxY",
                "ChampionNameHeight",
                "ChampionNameHeightForLongString",
                "ChampionNameOffset",
                "ChampionTextBoxHeight",
                "ChampionTextBoxWidth",
                "ChampionTextBoxX",
                "ChampionTextBoxY",
                "Classification",
                "ClearUndoHistoryOnActivate",
                "Clickable",
                "Consumed",
                "CooldownShowDisabledDuration",
                "CritAttack",
                "CritAttack_AttackDelayCastOffsetPercent",
                "CritAttack_AttackDelayOffsetPercent",
                "CritAttack_Probability",
                "CritDamageBonus",
                "CritPerLevel",
                "CriticalAttack",
                "DamagePerLevel",
                "DeathEventListeningRadius",
                "DeathTime",
                "DebugNumberOnChoas",
                "DebugNumberOnOrder",
                "DebugPing",
                "DelayCastOffsetPercent",
                "DelayTotalTimePercent",
                "Description",
                "DisableAggroIndicator",
                "DisappersOnDeath",
                "DisplayName",
                "DrawParLikeHealth",
                "DropsOnDeath",
                "DynamicTooltip",
                "EffectRadius",
                "EnemyTooltip",
                "Epicness",
                "ExpGivenOnDeath",
                "ExperienceRadius",
                "ExtraAttack1",
                "ExtraAttack1_AttackDelayCastOffsetPercent",
                "ExtraAttack1_Probability",
                "ExtraAttack2",
                "ExtraAttack2_AttackDelayCastOffsetPercent",
                "ExtraAttack2_Probability",
                "ExtraAttack3",
                "ExtraAttack3_AttackDelayCastOffsetPercent",
                "ExtraAttack3_Probability",
                "ExtraAttack4",
                "ExtraAttack4_Probability",
                "ExtraAttack5",
                "ExtraAttack5_Probability",
                "ExtraAttack6",
                "ExtraAttack6_Probability",
                "ExtraAttack7",
                "ExtraAttack7_Probability",
                "ExtraAttack8",
                "ExtraAttack8_Probability",
                "ExtraCritAttack1",
                "ExtraCritAttack1_AttackDelayCastOffsetPercent",
                "ExtraCritAttack2",
                "ExtraCritAttack3",
                "ExtraSpell1",
                "ExtraSpell10",
                "ExtraSpell2",
                "ExtraSpell3",
                "ExtraSpell4",
                "ExtraSpell5",
                "ExtraSpell6",
                "ExtraSpell7",
                "ExtraSpell8",
                "ExtraSpell9",
                "FireworksEnabled",
                "FlatArmorMod",
                "FlatAttackSpeedMod",
                "FlatCritChanceMod",
                "FlatCritDamageMod",
                "FlatDodgeMod",
                "FlatEnergyPoolMod",
                "FlatEnergyRegenMod",
                "FlatHPPoolMod",
                "FlatHPRegenMod",
                "FlatMPPoolMod",
                "FlatMPRegenMod",
                "FlatMagicDamageMod",
                "FlatMovementSpeedMod",
                "FlatPhysicalDamageMod",
                "FlatSpellBlockMod",
                "FriendlyTooltip",
                "GameplayCollisionRadius",
                "GlobalExpGivenOnDeath",
                "GlobalGoldGivenOnDeath",
                "GoldGivenOnDeath",
                "HPPerLevel",
                "HPRegenPerLevel",
                "HitFxScale",
                "HoverIndicatorRadius",
                "HoverIndicatorTextureName",
                "HoverLineIndicatorBaseTextureName",
                "HoverLineIndicatorTargetTextureName",
                "HoverLineIndicatorWidth",
                "Image",
                "ImageHeight",
                "ImageWidth",
                "InStore",
                "InventoryIcon",
                "InventorySlotMax",
                "InventorySlotMin",
                "IsElite",
                "IsEpic",
                "IsImportantBotTarget",
                "IsMelee",
                "IsRecipe",
                "ItemCalloutPlayer",
                "ItemCalloutSpectator",
                "ItemClass",
                "ItemGroup",
                "ItemId",
                "ItemType",
                "LevelDodge",
                "LiveUpdate",
                "LocalExpGivenOnDeath",
                "LocalGoldGivenOnDeath",
                "LocalGoldSplitWithLastHitter",
                "LongChampionName",
                "Lore1",
                "MPPerLevel",
                "MPRegenPerLevel",
                "MaxGroupOwnable",
                "MaxLevels",
                "MaxStack",
                "MessageOffsetX",
                "MessageOffsetY",
                "Metadata",
                "MonsterDataTableId",
                "MoveSpeed",
                "Name",
                "NeverRender",
                "NoAutoAttack",
                "NoHealthBar",
                "OccludedUnitSelectableDistance",
                "PARColor",
                "PARDisplayThroughDeath",
                "PARFadeColor",
                "PARHasRegenText",
                "PARIncrements",
                "PARMaxSegments",
                "PARNameString",
                "PARType",
                "PassLev1Desc1",
                "PassLev1Desc2",
                "PassLev1Desc3",
                "PassLev1Desc4",
                "PassLev1Desc5",
                "Passive1Desc",
                "Passive1Icon",
                "Passive1Level1",
                "Passive1Level2",
                "Passive1Level3",
                "Passive1Level4",
                "Passive1Level5",
                "Passive1Level6",
                "Passive1LuaName",
                "Passive1Name",
                "Passive2Level1",
                "Passive2Level2",
                "Passive2Level3",
                "Passive2Level4",
                "Passive3Level1",
                "Passive4Level1",
                "PathfindingCollisionRadius",
                "PercentArmorMod",
                "PercentAttackSpeedMod",
                "PercentCooldownMod",
                "PercentCritChanceMod",
                "PercentCritDamageMod",
                "PercentEXPBonus",
                "PercentHPPoolMod",
                "PercentHPRegenMod",
                "PercentLifeStealMod",
                "PercentMPRegenMod",
                "PercentMagicDamageMod",
                "PercentMagicPenetrationMod",
                "PercentMovementSpeedMod",
                "PercentPhysicalDamageMod",
                "PercentSlowResistMod",
                "PercentSpellBlockMod",
                "PercentSpellVampMod",
                "PercentTenacityItemMod",
                "PercentageCompleteTextX",
                "PercentageCompleteTextY",
                "PercentageMarginBetweenCardsX",
                "PercentageOfScreenBottomMargin",
                "PercentageOfScreenMiddleMargin",
                "PercentageOfScreenTopMargin",
                "PerceptionBubbleRadius",
                "PingBarMarginX",
                "PingBarMarginY",
                "PingBoxHeightFactor",
                "PingBoxOffsetFactorX",
                "PingBoxOffsetFactorY",
                "PingBoxWidthFactor",
                "Price",
                "ProgressTextX",
                "ProgressTextY",
                "ProgressbarBorderTexBotU",
                "ProgressbarBorderTexBotV",
                "ProgressbarBorderTexChaosOffset",
                "ProgressbarBorderTexU",
                "ProgressbarBorderTexV",
                "ProgressbarTexBotU",
                "ProgressbarTexBotV",
                "ProgressbarTexChaosOffset",
                "ProgressbarTexU",
                "ProgressbarTexV",
                "RankingsTextureAtlas",
                "RankingsTextureX",
                "RankingsTextureY",
                "RecipeItem1",
                "RecipeItem2",
                "RecipeItem3",
                "RecipeItem4",
                "RequiredChampion",
                "RequiredItem1",
                "RequiredLevel",
                "SelectionHeight",
                "SelectionRadius",
                "SellBackModifier",
                "ServerOnly",
                "ShouldFaceTarget",
                "Significance",
                "SkipDrawOutline",
                "SpecialRecipe",
                "Spell1",
                "Spell2",
                "Spell3",
                "Spell4",
                "SpellBlock",
                "SpellBlockPerLevel",
                "SpellDelayCastTime",
                "SpellDelayTotalTime",
                "SpellName",
                "SpellSupLevels1",
                "SpellSupLevels2",
                "SpellSupLevels3",
                "SpellSupLevels4",
                "SummonerNameHeight",
                "SummonerNameOffset",
                "SummonorBoxHeight",
                "SummonorBoxWidth",
                "SummonorSpellBox2X",
                "SummonorSpellBoxX",
                "SummonorSpellBoxY",
                "TeamsTextHeight",
                "TextWidth",
                "TextureHeight",
                "TextureName",
                "TextureWidth",
                "Textures",
                "Tips1",
                "Tips2",
                "TipsTextHeight",
                "TowerTargetingPriorityBoost",
                "UsableInStore",
                "UseChampionVisibility",
                "UseSkinNames",
                "UseWhenAcquired",
                "VsBorderTexBotU",
                "VsBorderTexBotV",
                "VsBorderTexU",
                "VsBorderTexV",
                "VsHeightFactor",
                "VsWidthFactor",
                "WeaponMaterial",
                "WeaponMaterial1",
                "WeaponMaterial2",
                "WeaponMaterial3",
                "blackboxheight",
                "blackboxwidth",
                "blackboxx",
                "blackboxy",
                "championcardheightpercentage",
                "championcardhorizontallayout",
                "chaoscardheightpercentage",
                "chaoscardwidthpercentage",
                "chaoscardxpercentage",
                "chaoscardypercentage",
                "flatBlockMod",
                "flatEXPBonus",
                "hideFromAll",
                "imagePath",
                "ordercardheightpercentage",
                "ordercardwidthpercentage",
                "ordercardxpercentage",
                "ordercardypercentage",
                "passive1range",
                "percentBlockMod",
                "percentDodgeMod",
                "percentMPPoolMod",
                "percentagecompletenopingtexty",
                "profileiconheightfactor",
                "profileiconoffsetfactorx",
                "profileiconoffsetfactory",
                "profileiconwidthfactor",
                "rFlatArmorModPerLevel",
                "rFlatArmorPenetrationMod",
                "rFlatArmorPenetrationModPerLevel",
                "rFlatCritChanceModPerLevel",
                "rFlatCritDamageModPerLevel",
                "rFlatDodgeMod",
                "rFlatDodgeModPerLevel",
                "rFlatEnergyModPerLevel",
                "rFlatEnergyRegenModPerLevel",
                "rFlatGoldPer10Mod",
                "rFlatHPModPerLevel",
                "rFlatHPRegenModPerLevel",
                "rFlatMPModPerLevel",
                "rFlatMPRegenModPerLevel",
                "rFlatMagicDamageModPerLevel",
                "rFlatMagicPenetrationMod",
                "rFlatMagicPenetrationModPerLevel",
                "rFlatMovementSpeedModPerLevel",
                "rFlatPhysicalDamageModPerLevel",
                "rFlatSpellBlockModPerLevel",
                "rFlatTimeDeadMod",
                "rFlatTimeDeadModPerLevel",
                "rPercentArmorPenetrationMod",
                "rPercentArmorPenetrationModPerLevel",
                "rPercentAttackSpeedModPerLevel",
                "rPercentCooldownMod",
                "rPercentCooldownModPerLevel",
                "rPercentMagicPenetrationMod",
                "rPercentMagicPenetrationModPerLevel",
                "rPercentMovementSpeedModPerLevel",
                "rPercentTimeDeadMod",
                "rPercentTimeDeadMod"
            };

            #endregion

            static Keys()
            {
                foreach (var section in PKeys)
                {
                    foreach (var name in PVark)
                    {
                        var hash = GetHash(section, name);
                        if (!KnownKeys.ContainsKey(hash))
                            KnownKeys.Add(hash, name);
                    }
                }
            }

            public static string GetKey(UInt32 hash)
            {
                return KnownKeys.ContainsKey(hash) ? KnownKeys[hash] : hash.ToString();
            }

            public static UInt32 GetHash(string section, string name)
            {
                UInt32 hash = 0;

                foreach (var c in section.ToLower())
                {
                    hash = c + 65599 * hash;
                }

                hash = (65599 * hash + 42);

                foreach (var c in name.ToLower())
                {
                    hash = c + 65599 * hash;
                }

                return hash;
            }
        }
    }

    public static class NumericTypeExtension
    {
        public static bool IsNumeric(this Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            return (dataType == typeof(int)
                    || dataType == typeof(double)
                    || dataType == typeof(long)
                    || dataType == typeof(short)
                    || dataType == typeof(float)
                    || dataType == typeof(Int16)
                    || dataType == typeof(Int32)
                    || dataType == typeof(Int64)
                    || dataType == typeof(uint)
                    || dataType == typeof(UInt16)
                    || dataType == typeof(UInt32)
                    || dataType == typeof(UInt64)
                    || dataType == typeof(sbyte)
                    || dataType == typeof(Single)
                );
        }

        public static bool IsInteger(this Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            return (dataType == typeof(int)
                    || dataType == typeof(long)
                    || dataType == typeof(short)
                    || dataType == typeof(Int16)
                    || dataType == typeof(Int32)
                    || dataType == typeof(Int64)
                    || dataType == typeof(uint)
                    || dataType == typeof(UInt16)
                    || dataType == typeof(UInt32)
                    || dataType == typeof(UInt64)
                    || dataType == typeof(sbyte)
                    || dataType == typeof(Single)
                );
        }

        public static bool IsFloatingPoint(this Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            return (dataType == typeof(double) || dataType == typeof(float));
        }
    }
}