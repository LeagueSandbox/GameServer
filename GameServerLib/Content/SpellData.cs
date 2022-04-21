using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Content
{
    public enum SpellTargetType
    {
        TARGET_SELF = 0, // teemo W ; xin Q
        TARGET_UNIT = 1, // Taric E ; Annie Q ; teemo Q ; xin E
        TARGET_LOC_AOE = 2, // Lux E, Ziggs R
        TARGET_CONE = 3, // Annie W, Kass E
        TARGET_SELF_AOE = 4, // sivir R, Gangplanck E
        TARGET_LOC = 6, // Ez Q, W, E, R ; Mundo Q
        TARGET_LOC2 = 7  // Morg Q, Cait's Q -- These don't seem to have Missile inibins, and SpawnProjectile doesn't seem necessary to show the projectiles
    }

    public class SpellData : ISpellData
    {
        private readonly ContentManager _contentManager;
        private readonly ILog _logger;

        public SpellData(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _logger = LoggerProvider.GetLogger();
        }

        public string AfterEffectName { get; set; } = "";
        //AIEndOnly
        //AILifetime
        //AIRadius
        //AIRange
        //AISendEvent
        //AISpeed
        public string AlternateName { get; set; } = "";
        public bool AlwaysSnapFacing { get; set; }
        //AmmoCountHiddenInUI
        public float[] AmmoRechargeTime { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        public int[] AmmoUsed { get; set; } = { 1, 1, 1, 1, 1, 1, 1 };
        public string AnimationLeadOutName { get; set; } = "";
        public string AnimationLoopName { get; set; } = "";
        public string AnimationName { get; set; } = "";
        public string AnimationWinddownName { get; set; } = "";
        public float AttackDamageCoefficient { get; set; }
        //ApplyAttackDamage
        //ApplyAttackEffect
        //ApplyMaterialOnHitSound
        public bool BelongsToAvatar { get; set; }
        public float BounceRadius { get; set; } = 450;
        public bool CanCastWhileDisabled { get; set; }
        public float CancelChargeOnRecastTime { get; set; }
        public bool CanMoveWhileChanneling { get; set; }
        public bool CannotBeSuppressed { get; set; }
        public bool CanOnlyCastWhileDead { get; set; }
        public bool CanOnlyCastWhileDisabled { get; set; }
        public bool CantCancelWhileChanneling { get; set; }
        public bool CantCancelWhileWindingUp { get; set; }
        public bool CantCastWhileRooted { get; set; }
        public float CastConeAngle { get; set; } = 45;
        public float CastConeDistance { get; set; } = 400;
        public float CastFrame { get; set; }
        public float[] CastRadius { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        public float[] CastRadiusSecondary { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        //CastRadiusSecondaryTexture
        //CastRadiusTexture
        public float[] CastRange { get; set; } = { 400, 400, 400, 400, 400, 400, 400 };
        public float CastRangeDisplayOverride { get; set; }
        public float[] CastRangeGrowthDuration { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        public float[] CastRangeGrowthMax { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        //CastRangeTextureOverrideName
        public bool CastRangeUseBoundingBoxes { get; set; }
        public float CastTargetAdditionalUnitsRadius { get; set; }
        public int CastType { get; set; }
        public float[] ChannelDuration { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        public float ChargeUpdateInterval { get; set; }
        public float CircleMissileAngularVelocity { get; set; }
        public float CircleMissileRadialVelocity { get; set; }
        //ClientOnlyMissileTargetBoneName
        public bool ConsideredAsAutoAttack { get; set; }
        public float[] Cooldown { get; set; } = { 10, 10, 10, 10, 10, 10, 10 };
        //CursorChangesInGrass
        //CursorChangesInTerrain
        public int DeathRecapPriority { get; set; }
        public float DelayCastOffsetPercent { get; set; }
        public float DelayTotalTimePercent { get; set; }
        //Description
        //DisableCastBar
        //DisplayName
        public bool DoesntBreakChannels { get; set; }
        public bool DoNotNeedToFaceTarget { get; set; }
        //DrawSecondaryLineIndicator
        //DynamicExtended
        //string DynamicTooltip
        //EffectXLevelYAmmount
        public SpellDataFlags Flags { get; set; }
        //FloatStaticsDecimalsX
        //FloatVarsDecimalsX
        public bool HaveAfterEffect { get; set; }
        public bool HaveHitBone { get; set; }
        public bool HaveHitEffect { get; set; }
        public bool HavePointEffect { get; set; }
        //HideRangeIndicatorWhenCasting
        public string HitBoneName { get; set; } = "";
        public string HitEffectName { get; set; } = "";
        public int HitEffectOrientType { get; set; } = 1;
        //HitEffectPlayerName
        public bool IgnoreAnimContinueUntilCastFrame { get; set; }
        public bool IgnoreRangeCheck { get; set; }
        //InventoryIconX
        public bool IsDisabledWhileDead { get; set; } = true;
        public bool IsToggleSpell { get; set; }
        //KeywordWhenAcquired
        //LevelXDesc
        public float LineDragLength { get; set; }
        public int LineMissileBounces { get; set; }
        public bool LineMissileCollisionFromStartPoint { get; set; }
        public float LineMissileDelayDestroyAtEndSeconds { get; set; }
        public bool LineMissileEndsAtTargetPoint { get; set; }
        public bool LineMissileFollowsTerrainHeight { get; set; }
        public float LineMissileTargetHeightAugment { get; set; } = 100;
        public float LineMissileTimePulseBetweenCollisionSpellHits { get; set; }
        public bool LineMissileTrackUnits { get; set; }
        public bool LineMissileUsesAccelerationForBounce { get; set; }
        //LineTargetingBaseTextureOverrideName
        //LineTargetingBaseTextureOverrideName
        public float LineWidth { get; set; }
        public float[] LocationTargettingLength { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        public float[] LocationTargettingWidth { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        public bool LockConeToPlayer { get; set; }
        //LookAtPolicy
        public float LuaOnMissileUpdateDistanceInterval { get; set; }
        public float MagicDamageCoefficient { get; set; }
        public float[] ManaCost { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        //Map_X_EffectYLevelZAmmount
        public int MaxAmmo { get; set; } = 1;
        //MaxGrowthRangeTextureName
        //MinimapIcon
        //MinimapIconDisplayFlag
        //MinimapIconRotation
        public float MissileAccel { get; set; }
        public string MissileBoneName { get; set; } = "";
        public string MissileEffect { get; set; } = "";
        public string MissileEffectPlayer { get; set; } = "";
        public float MissileFixedTravelTime { get; set; }
        public bool MissileFollowsTerrainHeight { get; set; }
        public float MissileGravity { get; set; }
        public float MissileLifetime { get; set; }
        public float MissileMaxSpeed { get; set; }
        public float MissileMinSpeed { get; set; }
        public float MissileMinTravelTime { get; set; }
        public float MissilePerceptionBubbleRadius { get; set; }
        public bool MissilePerceptionBubbleRevealsStealth { get; set; }
        public float MissileSpeed { get; set; } = 500;
        public float MissileTargetHeightAugment { get; set; } = 100;
        public bool MissileUnblockable { get; set; }
        public bool NoWinddownIfCancelled { get; set; }
        //NumSpellTargeters
        //OrientRadiusTextureFromPlayer
        //OrientRangeIndicatorToCursor
        //OrientRangeIndicatorToFacing
        public float OverrideCastTime { get; set; }
        public Vector3 ParticleStartOffset { get; set; } = new Vector3(0, 0, 0);
        //PlatformEnabled
        public string PointEffectName { get; set; } = "";
        //RangeIndicatorTextureName
        public string RequiredUnitTags { get; set; } = "";
        public string SelectionPreference { get; set; } = "";
        //Sound_CastName
        //Sound_HitName
        //Sound_VOEventCategory
        public float SpellCastTime { get; set; }
        public float SpellRevealsChampion { get; set; }
        public float SpellTotalTime { get; set; }
        public float StartCooldown { get; set; }
        public bool SubjectToGlobalCooldown { get; set; } = true;
        //TargeterConstrainedToRange
        public TargetingType TargetingType { get; set; } = TargetingType.Target;
        public string TextFlags { get; set; } = "";
        public bool TriggersGlobalCooldown { get; set; } = true;
        public bool UpdateRotationWhenCasting { get; set; } = true;
        public bool UseAnimatorFramerate { get; set; }
        public bool UseAutoattackCastTime { get; set; }
        public bool UseChargeChanneling { get; set; }
        public bool UseChargeTargeting { get; set; }
        public bool UseGlobalLineIndicator { get; set; }
        public bool UseMinimapTargeting { get; set; }
        //Version
        //x1,x2,x3,x4,x5

        /// <summary>
        /// Determines whether or not the target unit should be affected by the spell which has this SpellData.
        /// </summary>
        /// <param name="attacker">AI which cast the spell.</param>
        /// <param name="target">Unit which is being affected.</param>
        /// <param name="overrideFlags">SpellDataFlags to use in place of the ones in this SpellData.</param>
        /// <returns>True/False.</returns>
        public bool IsValidTarget(IObjAiBase attacker, IAttackableUnit target, SpellDataFlags overrideFlags = 0)
        {

            bool overrideTargetable = false;
            if (target is IObjAiBase obj && obj.CharData.IsUseable && Flags.HasFlag(SpellDataFlags.AffectUseable))
            {
                //TODO: Verify if we need a check for CharData.UsableByEnemy here too.
                overrideTargetable = true;
            }

            if (!target.Status.HasFlag(StatusFlags.Targetable) && !overrideTargetable)
            {
                return false;
            }

            var useFlags = Flags;

            if (overrideFlags > 0)
            {
                useFlags = overrideFlags;
            }

            if (target.IsDead && !useFlags.HasFlag(SpellDataFlags.AffectDead))
            {
                return false;
            }

            if (target.Team == attacker.Team && !useFlags.HasFlag(SpellDataFlags.AffectFriends))
            {
                return false;
            }

            if (target.Team != attacker.Team && target.Team != TeamId.TEAM_NEUTRAL && !useFlags.HasFlag(SpellDataFlags.AffectEnemies))
            {
                return false;
            }

            bool valid = true;

            // Assuming all of the team-based checks passed, we move onto unit-based checks.
            if (valid)
            {
                if (Flags.HasFlag(SpellDataFlags.AffectAllUnitTypes))
                {
                    valid = true;
                }
                else
                {
                    switch (target)
                    {
                        // TODO: Verify all
                        // Order is important
                        case ILaneMinion _ when useFlags.HasFlag(SpellDataFlags.AffectMinions)
                                        && !useFlags.HasFlag(SpellDataFlags.IgnoreLaneMinion):
                            valid = true;
                            break;
                        case IMinion m when (!(m is IPet) && useFlags.HasFlag(SpellDataFlags.AffectNotPet))
                                    || (m is IPet && useFlags.HasFlag(SpellDataFlags.AffectUseable))
                                    || (m.IsWard && useFlags.HasFlag(SpellDataFlags.AffectWards))
                                    || (!(m is IPet pet && pet.IsClone) && useFlags.HasFlag(SpellDataFlags.IgnoreClones))
                                    || (target.Team == attacker.Team && !useFlags.HasFlag(SpellDataFlags.IgnoreAllyMinion))
                                    || (target.Team != attacker.Team && target.Team != TeamId.TEAM_NEUTRAL && !useFlags.HasFlag(SpellDataFlags.IgnoreEnemyMinion))
                                    || useFlags.HasFlag(SpellDataFlags.AffectMinions):
                            if (!(target is ILaneMinion))
                            {
                                valid = true;
                                break;
                            }
                            // already got checked in ILaneMinion
                            valid = false;
                            break;
                        case IBaseTurret _ when useFlags.HasFlag(SpellDataFlags.AffectTurrets):
                            valid = true;
                            break;
                        case IInhibitor _ when useFlags.HasFlag(SpellDataFlags.AffectBuildings):
                            valid = true;
                            break;
                        case INexus _ when useFlags.HasFlag(SpellDataFlags.AffectBuildings):
                            valid = true;
                            break;
                        case IChampion _ when useFlags.HasFlag(SpellDataFlags.AffectHeroes):
                            valid = true;
                            break;
                        default:
                            valid = false;
                            break;
                    }

                    // TODO: Verify if placing this here is okay.
                    if (target.Team == TeamId.TEAM_NEUTRAL && !useFlags.HasFlag(SpellDataFlags.AffectNeutral))
                    {
                        valid = false;
                    }
                }
            }

            return valid;
        }

        public float GetCastTime()
        {
            return (1.0f + DelayCastOffsetPercent) * 0.5f;
        }

        // TODO: Implement this (where it is verified to be needed)
        public float GetCastTimeTotal()
        {
            return (1.0f + DelayTotalTimePercent) * 2.0f;
        }

        // TODO: read Global Character Data constants from constants.var (gcd_AttackDelay = 1.600f, gcd_AttackDelayCastPercent = 0.300f)
        public float GetCharacterAttackDelay
        (
            float attackSpeedMod,
            float attackDelayOffsetPercent,
            float attackMinimumDelay = 0.4f,
            float attackMaximumDelay = 5.0f
        )
        {
            float result = ((attackDelayOffsetPercent + 1.0f) * 1.600f) / attackSpeedMod;
            return System.Math.Clamp(result, attackMinimumDelay, attackMaximumDelay);
        }

        public float GetCharacterAttackCastDelay
        (
            float attackSpeedMod,
            float attackDelayOffsetPercent,
            float attackDelayCastOffsetPercent,
            float attackDelayCastOffsetPercentAttackSpeedRatio,
            float attackMinimumDelay = 0.4f,
            float attackMaximumDelay = 5.0f
        )
        {
            float castPercent = System.Math.Min(0.300f + attackDelayCastOffsetPercent, 0.0f);
            float percentDelay = GetCharacterAttackDelay(1.0f, attackDelayOffsetPercent, attackMinimumDelay, attackMaximumDelay) * castPercent;
            float attackDelay = GetCharacterAttackDelay(attackSpeedMod, attackDelayCastOffsetPercent, attackMinimumDelay, attackMaximumDelay);
            float result = (((attackDelay * castPercent) - percentDelay) * attackDelayCastOffsetPercentAttackSpeedRatio) + percentDelay;
            return System.Math.Min(result, attackDelay);
        }

        public void SetTargetingType(TargetingType newType)
        {
            TargetingType = newType;
        }

        public void Load(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var file = new ContentFile();
            try
            {
                file = (ContentFile)_contentManager.GetContentFileFromJson("Spells", name);
            }

            catch (ContentNotFoundException exception)
            {
                _logger.Warn(exception.Message);
                return;
            }

            AfterEffectName = file.GetString("SpellData", "AfterEffectName", AfterEffectName);
            //AIEndOnly
            //AILifetime
            //AIRadius
            //AIRange
            //AISendEvent
            //AISpeed
            AlternateName = file.GetString("SpellData", "AlternateName", name);
            AlwaysSnapFacing = file.GetBool("SpellData", "AlwaysSnapFacing", AlwaysSnapFacing);
            //AmmoCountHiddenInUI
            float lastValidTime = 0;
            for (var i = 1; i <= 6 + 1; i++)
            {
                float time = file.GetFloat("SpellData", $"AmmoRechargeTime{i}", 0);

                if(time > 0)
                {
                    AmmoRechargeTime[i - 1] = time;
                    lastValidTime = time;
                }
                else
                {
                    AmmoRechargeTime[i - 1] = lastValidTime;
                }
            }
            AmmoUsed = file.GetMultiInt("SpellData", "AmmoUsed", 6, AmmoUsed[0]);
            AnimationLeadOutName = file.GetString("SpellData", "AnimationLeadOutName", name);
            AnimationLoopName = file.GetString("SpellData", "AnimationLoopName", name);
            AnimationName = file.GetString("SpellData", "AnimationName", name);
            AnimationWinddownName = file.GetString("SpellData", "AnimationWinddownName", name);
            AttackDamageCoefficient = file.GetFloat("SpellData", "Coefficient", AttackDamageCoefficient);
            //ApplyAttackDamage
            //ApplyAttackEffect
            //ApplyMaterialOnHitSound
            BelongsToAvatar = file.GetBool("SpellData", "BelongsToAvatar", BelongsToAvatar);
            BounceRadius = file.GetFloat("SpellData", "BounceRadius", BounceRadius);
            CanCastWhileDisabled = file.GetBool("SpellData", "CanCastWhileDisabled", CanCastWhileDisabled);
            CancelChargeOnRecastTime = file.GetFloat("SpellData", "CancelChargeOnRecastTime", CancelChargeOnRecastTime);
            CanMoveWhileChanneling = file.GetBool("SpellData", "CanMoveWhileChanneling", CanMoveWhileChanneling);
            CannotBeSuppressed = file.GetBool("SpellData", "CannotBeSuppressed", CannotBeSuppressed);
            CanOnlyCastWhileDead = file.GetBool("SpellData", "CanOnlyCastWhileDead", CanOnlyCastWhileDead);
            CanOnlyCastWhileDisabled = file.GetBool("SpellData", "CanOnlyCastWhileDisabled", CanOnlyCastWhileDisabled);
            CantCancelWhileChanneling = file.GetBool("SpellData", "CantCancelWhileChanneling", CantCancelWhileChanneling);
            CantCancelWhileWindingUp = file.GetBool("SpellData", "CantCancelWhileWindingUp", CantCancelWhileWindingUp);
            CantCastWhileRooted = file.GetBool("SpellData", "CantCastWhileRooted", CantCastWhileRooted);
            CastConeAngle = file.GetFloat("SpellData", "CastConeAngle", CastConeAngle);
            CastConeDistance = file.GetFloat("SpellData", "CastConeDistance", CastConeDistance);
            CastFrame = file.GetFloat("SpellData", "CastFrame", CastFrame);
            CastRadius = file.GetMultiFloat("SpellData", "CastRadius", 6, CastRadius[0]);
            CastRadiusSecondary = file.GetMultiFloat("SpellData", "CastRadiusSecondary", 6, CastRadiusSecondary[0]);
            //CastRadiusSecondaryTexture
            //CastRadiusTexture
            CastRange = file.GetMultiFloat("SpellData", "CastRange", 6, CastRange[0]);
            CastRangeDisplayOverride = file.GetFloat("SpellData", "CastRangeDisplayOverride");
            CastRangeGrowthDuration = file.GetMultiFloat("SpellData", "CastRangeGrowthDuration", 6, CastRangeGrowthDuration[0]);
            CastRangeGrowthMax = file.GetMultiFloat("SpellData", "CastRangeGrowthMax", 6, CastRangeGrowthMax[0]);
            //CastRangeTextureOverrideName
            CastRangeUseBoundingBoxes = file.GetBool("SpellData", "CastRangeUseBoundingBoxes", CastRangeUseBoundingBoxes);
            CastTargetAdditionalUnitsRadius = file.GetFloat("SpellData", "CastTargetAdditionalUnitsRadius", CastTargetAdditionalUnitsRadius);
            CastType = file.GetInt("SpellData", "CastType", CastType);
            ChannelDuration = file.GetMultiFloat("SpellData", "ChannelDuration", 6, ChannelDuration[0]);
            ChargeUpdateInterval = file.GetFloat("SpellData", "ChargeUpdateInterval", ChargeUpdateInterval);
            CircleMissileAngularVelocity = file.GetFloat("SpellData", "CircleMissileAngularVelocity", CircleMissileAngularVelocity);
            CircleMissileRadialVelocity = file.GetFloat("SpellData", "CircleMissileRadialVelocity", CircleMissileRadialVelocity);
            //ClientOnlyMissileTargetBoneName
            ConsideredAsAutoAttack = file.GetBool("SpellData", "ConsideredAsAutoAttack", ConsideredAsAutoAttack);
            Cooldown = file.GetMultiFloat("SpellData", "Cooldown", 6, Cooldown[0]);
            //CursorChangesInGrass
            //CursorChangesInTerrain
            DeathRecapPriority = file.GetInt("SpellData", "DeathRecapPriority", DeathRecapPriority);
            DelayCastOffsetPercent = file.GetFloat("SpellData", "DelayCastOffsetPercent", DelayCastOffsetPercent);
            DelayTotalTimePercent = file.GetFloat("SpellData", "DelayTotalTimePercent", DelayTotalTimePercent);
            //Description
            //DisableCastBar
            //DisplayName
            DoesntBreakChannels = file.GetBool("SpellData", "DoesntBreakChannels", DoesntBreakChannels);
            DoNotNeedToFaceTarget = file.GetBool("SpellData", "DoNotNeedToFaceTarget", DoNotNeedToFaceTarget);
            //DrawSecondaryLineIndicator
            //DynamicExtended
            //string DynamicTooltip
            //EffectXLevelYAmmount
            Flags = (SpellDataFlags)file.GetInt("SpellData", "Flags");
            //FloatStaticsDecimalsX
            //FloatVarsDecimalsX
            HaveAfterEffect = file.GetBool("SpellData", "HaveAfterEffect", HaveAfterEffect);
            HaveHitBone = file.GetBool("SpellData", "HaveHitBone", HaveHitBone);
            HaveHitEffect = file.GetBool("SpellData", "HaveHitEffect", HaveHitEffect);
            HavePointEffect = file.GetBool("SpellData", "HavePointEffect", HavePointEffect);
            //HideRangeIndicatorWhenCasting
            HitBoneName = file.GetString("SpellData", "HitBoneName", HitBoneName);
            HitEffectName = file.GetString("SpellData", "HitEffectName", HitEffectName);
            HitEffectOrientType = file.GetInt("SpellData", "HitEffectOrientType", HitEffectOrientType);
            //HitEffectPlayerName
            IgnoreAnimContinueUntilCastFrame = file.GetBool("SpellData", "IgnoreAnimContinueUntilCastFrame", IgnoreAnimContinueUntilCastFrame);
            IgnoreRangeCheck = file.GetBool("SpellData", "IgnoreRangeCheck", IgnoreRangeCheck);
            //InventoryIconX
            IsDisabledWhileDead = file.GetBool("SpellData", "IsDisabledWhileDead", IsDisabledWhileDead);
            IsToggleSpell = file.GetBool("SpellData", "IsToggleSpell", IsToggleSpell);
            //KeywordWhenAcquired
            //LevelXDesc
            LineDragLength = file.GetFloat("SpellData", "LineDragLength", LineDragLength);
            LineMissileBounces = file.GetInt("SpellData", "LineMissileBounces", LineMissileBounces);
            LineMissileCollisionFromStartPoint = file.GetBool("SpellData", "LineMissileCollisionFromStartPoint", LineMissileCollisionFromStartPoint);
            LineMissileDelayDestroyAtEndSeconds = file.GetFloat("SpellData", "LineMissileDelayDestroyAtEndSeconds", LineMissileDelayDestroyAtEndSeconds);
            LineMissileEndsAtTargetPoint = file.GetBool("SpellData", "LineMissileEndsAtTargetPoint", LineMissileEndsAtTargetPoint);
            LineMissileFollowsTerrainHeight = file.GetBool("SpellData", "LineMissileFollowsTerrainHeight", LineMissileFollowsTerrainHeight);
            LineMissileTargetHeightAugment = file.GetFloat("SpellData", "LineMissileTargetHeightAugment", LineMissileTargetHeightAugment);
            LineMissileTimePulseBetweenCollisionSpellHits = file.GetFloat("SpellData", "LineMissileTimePulseBetweenCollisionSpellHits", LineMissileTimePulseBetweenCollisionSpellHits);
            LineMissileTrackUnits = file.GetBool("SpellData", "LineMissileTrackUnits", LineMissileTrackUnits);
            LineMissileUsesAccelerationForBounce = file.GetBool("SpellData", "LineMissileUsesAccelerationForBounce", LineMissileUsesAccelerationForBounce);
            //LineTargetingBaseTextureOverrideName
            //LineTargetingBaseTextureOverrideName
            LineWidth = file.GetFloat("SpellData", "LineWidth", LineWidth);
            LocationTargettingLength = file.GetMultiFloat("SpellData", "LocationTargettingLength", 6, LocationTargettingLength[0]);
            LocationTargettingWidth = file.GetMultiFloat("SpellData", "LocationTargettingWidth", 6, LocationTargettingWidth[0]);
            LockConeToPlayer = file.GetBool("SpellData", "LockConeToPlayer", LockConeToPlayer);
            //LookAtPolicy
            LuaOnMissileUpdateDistanceInterval = file.GetFloat("SpellData", "LuaOnMissileUpdateDistanceInterval", LuaOnMissileUpdateDistanceInterval);
            MagicDamageCoefficient = file.GetFloat("SpellData", "Coefficient2", MagicDamageCoefficient);
            ManaCost = file.GetMultiFloat("SpellData", "ManaCost", 6, ManaCost[0]);
            //Map_X_EffectYLevelZAmmount
            MaxAmmo = file.GetInt("SpellData", "MaxAmmo", MaxAmmo);
            //MaxGrowthRangeTextureName
            //MinimapIcon
            //MinimapIconDisplayFlag
            //MinimapIconRotation
            MissileSpeed = file.GetFloat("SpellData", "MissileSpeed", MissileSpeed);
            MissileAccel = file.GetFloat("SpellData", "MissileAccel", MissileAccel);
            MissileBoneName = file.GetString("SpellData", "MissileBoneName", MissileBoneName);
            MissileEffect = file.GetString("SpellData", "MissileEffect", MissileEffect);
            MissileEffectPlayer = file.GetString("SpellData", "MissileEffectPlayer", MissileEffectPlayer);
            MissileFixedTravelTime = file.GetFloat("SpellData", "MissileFixedTravelTime", MissileFixedTravelTime);
            MissileFollowsTerrainHeight = file.GetBool("SpellData", "MissileFollowsTerrainHeight", MissileFollowsTerrainHeight);
            MissileGravity = file.GetFloat("SpellData", "MissileGravity", MissileGravity);
            MissileLifetime = file.GetFloat("SpellData", "MissileLifetime", MissileLifetime);
            MissileMaxSpeed = file.GetFloat("SpellData", "MissileMaxSpeed", MissileSpeed);
            MissileMinSpeed = file.GetFloat("SpellData", "MissileMinSpeed", MissileSpeed);
            MissileMinTravelTime = file.GetFloat("SpellData", "MissileMinTravelTime", MissileMinTravelTime);
            MissilePerceptionBubbleRadius = file.GetFloat("SpellData", "MissilePerceptionBubbleRadius", MissilePerceptionBubbleRadius);
            MissilePerceptionBubbleRevealsStealth = file.GetBool("SpellData", "MissilePerceptionBubbleRevealsStealth", MissilePerceptionBubbleRevealsStealth);
            //MissileSpeed = file.GetFloat("SpellData", "MissileSpeed", MissileSpeed);
            MissileTargetHeightAugment = file.GetFloat("SpellData", "MissileTargetHeightAugment", MissileTargetHeightAugment);
            MissileUnblockable = file.GetBool("SpellData", "MissileUnblockable", MissileUnblockable);
            NoWinddownIfCancelled = file.GetBool("SpellData", "NoWinddownIfCancelled", NoWinddownIfCancelled);
            //NumSpellTargeters
            //OrientRadiusTextureFromPlayer
            //OrientRangeIndicatorToCursor
            //OrientRangeIndicatorToFacing
            OverrideCastTime = file.GetFloat("SpellData", "OverrideCastTime", OverrideCastTime);
            //public Vector3 ParticleStartOffset { get; set; } = new Vector3(0, 0, 0);
            var particleStartOffset = file.GetFloatArray("SpellData", "ParticleStartOffset", new[] { ParticleStartOffset.X, ParticleStartOffset.Y, ParticleStartOffset.Z });
            ParticleStartOffset = new Vector3(particleStartOffset[0], particleStartOffset[1], particleStartOffset[2]);
            //PlatformEnabled
            PointEffectName = file.GetString("SpellData", "PointEffectName", PointEffectName);
            //RangeIndicatorTextureName
            RequiredUnitTags = file.GetString("SpellData", "RequiredUnitTags", RequiredUnitTags);
            SelectionPreference = file.GetString("SpellData", "SelectionPreference", SelectionPreference);
            //Sound_CastName
            //Sound_HitName
            //Sound_VOEventCategory
            SpellCastTime = file.GetFloat("SpellData", "SpellCastTime", SpellCastTime);
            SpellRevealsChampion = file.GetFloat("SpellData", "SpellRevealsChampion", SpellRevealsChampion);
            SpellTotalTime = file.GetFloat("SpellData", "SpellTotalTime", SpellTotalTime);
            StartCooldown = file.GetFloat("SpellData", "StartCooldown", StartCooldown);
            SubjectToGlobalCooldown = file.GetBool("SpellData", "SubjectToGlobalCooldown", SubjectToGlobalCooldown);
            //TargeterConstrainedToRange
            TargetingType = (TargetingType)file.GetInt("SpellData", "TargettingType", (int)TargetingType);
            TextFlags = file.GetString("SpellData", "TextFlags", TextFlags);
            TriggersGlobalCooldown = file.GetBool("SpellData", "TriggersGlobalCooldown", TriggersGlobalCooldown);
            UpdateRotationWhenCasting = file.GetBool("SpellData", "UpdateRotationWhenCasting", UpdateRotationWhenCasting);
            UseAnimatorFramerate = file.GetBool("SpellData", "UseAnimatorFramerate", UseAnimatorFramerate);
            UseAutoattackCastTime = file.GetBool("SpellData", "UseAutoattackCastTime", UseAutoattackCastTime);
            UseChargeChanneling = file.GetBool("SpellData", "UseChargeChanneling", UseChargeChanneling);
            UseChargeTargeting = file.GetBool("SpellData", "UseChargeTargeting", UseChargeTargeting);
            UseGlobalLineIndicator = file.GetBool("SpellData", "UseGlobalLineIndicator", UseGlobalLineIndicator);
            UseMinimapTargeting = file.GetBool("SpellData", "UseMinimapTargeting", UseMinimapTargeting);
        }
    }
}
