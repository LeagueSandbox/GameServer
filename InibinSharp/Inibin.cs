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

            var size = (int) _reader.BaseStream.Length;
            var version = ReadValue<byte>();
            var oldLength = ReadValue<UInt16>();
            var bitmask = ReadValue<UInt16>();
            _stringOffset = size - oldLength;

            Debug.WriteLine("Version:" + version);
            Debug.WriteLine("Length:" + size);
            Debug.WriteLine("OldLength:" + oldLength);
            Debug.WriteLine("StringOffset:" + _stringOffset);
            Debug.WriteLine("Bitmask:" + bitmask);
            Debug.WriteLine("");

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
                Debug.WriteLine("{0} [{1}] = {2}", typeof (T).Name, key, value);
            }
        }

        private void SkipValues(int size)
        {
            var start = _reader.BaseStream.Position;
            var keys = ReadSegmentKeys();
            _reader.BaseStream.Position += keys.Length*size;
            Debug.WriteLine("{0} properties skip from {1} to {2}", size, start, _reader.BaseStream.Position);
        }

        private void ParseValues<T>(bool isBase10 = false)
        {
            Debug.WriteLine("{0} properties start position {1}", typeof (T).Name, _reader.BaseStream.Position);
            var keys = ReadSegmentKeys();

            if (typeof (T) == typeof (bool))
            {
                var index = 0;
                for (var i = 0; i < 1 + ((keys.Length - 1)/8); ++i)
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
            else if (typeof (T) == typeof (string))
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
                        AddValue(key, ((byte) (object) ReadValue<T>())*0.1f);
                    }
                    else
                    {
                        AddValue(key, ReadValue<T>());
                    }
                }
            }

            Debug.WriteLine("");
        }

        private T ReadValue<T>(int offset = 0)
        {
            try
            {
                if (typeof (T) == typeof (byte))
                {
                    return (T) (object) _reader.ReadByte();
                }
                if (typeof (T) == typeof (UInt16))
                {
                    return (T) (object) _reader.ReadUInt16();
                }
                if (typeof (T) == typeof (UInt32))
                {
                    return (T) (object) _reader.ReadUInt32();
                }
                if (typeof (T) == typeof (float))
                {
                    return (T) (object) _reader.ReadSingle();
                }
                if (typeof (T) == typeof (string))
                {
                    int c;
                    var sb = new StringBuilder();
                    var oldPos = _reader.BaseStream.Position;
                    _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    while ((c = _reader.ReadByte()) > 0)
                    {
                        sb.Append((char) c);
                    }
                    _reader.BaseStream.Seek(oldPos, SeekOrigin.Begin);

                    return (T) (object) sb.ToString();
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
                    throw new KeyNotFoundException(key.ToString());
                }

                if (typeof (T) == typeof (string))
                {
                    return (T) (object) Values[key].ToString();
                }

                // integers -> bool
                if (typeof (T) == typeof (bool) && Values[key].GetType().IsInteger())
                {
                    return (T) (object) ((int) Values[key] > 0);
                }

                var value = Values[key] as string;
                if (value != null)
                {
                    // string -> bool
                    if (typeof (T) == typeof (bool))
                    {
                        return (T) (object) (value == "1");
                    }

                    if (typeof (T).IsNumeric())
                    {
                        // string -> byte/short/int
                        int intValue;
                        if (Int32.TryParse(value, out intValue))
                        {
                            return (T) (object) intValue;
                        }

                        // string -> double/float
                        double doubleValue;
                        if (Double.TryParse(value, out doubleValue))
                        {
                            return (T) (object) doubleValue;
                        }
                    }
                }

                return (T) Values[key];
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine(key);
                return default(T);
            }
            catch (InvalidCastException)
            {
                Debug.WriteLine(Values[key].GetType().Name + " to " + typeof (T).Name + " @ " + key);
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
                    hash = c + 65599*hash;
                }

                hash = (65599*hash + 42);

                foreach (var c in name.ToLower())
                {
                    hash = c + 65599*hash;
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

            return (dataType == typeof (int)
                    || dataType == typeof (double)
                    || dataType == typeof (long)
                    || dataType == typeof (short)
                    || dataType == typeof (float)
                    || dataType == typeof (Int16)
                    || dataType == typeof (Int32)
                    || dataType == typeof (Int64)
                    || dataType == typeof (uint)
                    || dataType == typeof (UInt16)
                    || dataType == typeof (UInt32)
                    || dataType == typeof (UInt64)
                    || dataType == typeof (sbyte)
                    || dataType == typeof (Single)
                );
        }

        public static bool IsInteger(this Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            return (dataType == typeof (int)
                    || dataType == typeof (long)
                    || dataType == typeof (short)
                    || dataType == typeof (Int16)
                    || dataType == typeof (Int32)
                    || dataType == typeof (Int64)
                    || dataType == typeof (uint)
                    || dataType == typeof (UInt16)
                    || dataType == typeof (UInt32)
                    || dataType == typeof (UInt64)
                    || dataType == typeof (sbyte)
                    || dataType == typeof (Single)
                );
        }

        public static bool IsFloatingPoint(this Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            return (dataType == typeof (double) || dataType == typeof (float));
        }
    }
}