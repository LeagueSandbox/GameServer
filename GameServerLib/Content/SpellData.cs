using System.IO;
using System.Numerics;
using GameServerCore.Domain;
using IniParser;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Exceptions;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Content
{
    public enum SpellFlag : uint
    {
        AUTO_CAST = 1u << 1,
        INSTANT_CAST = 1u << 2,
        PERSIST_THROUGH_DEATH = 1u << 3,
        NON_DISPELLABLE = 1u << 4,
        NO_CLICK = 1u << 5,
        AFFECT_IMPORTANT_BOT_TARGETS = 1u << 6,
        ALLOW_WHILE_TAUNTED = 1u << 7,
        NOT_AFFECT_ZOMBIE = 1u << 8,
        AFFECT_UNTARGETABLE = 1u << 9,
        AFFECT_ENEMIES = 1u << 10,
        AFFECT_FRIENDS = 1u << 11,
        AFFECT_BUILDINGS = 1u << 12,
        NOT_AFFECT_SELF = 1u << 13,
        AFFECT_NEUTRAL = 1u << 14,
        AFFECT_ALL_SIDES = AFFECT_ENEMIES | AFFECT_FRIENDS | AFFECT_NEUTRAL,
        AFFECT_MINIONS = 1u << 15,
        AFFECT_HEROES = 1u << 16,
        AFFECT_TURRETS = 1u << 17,
        AFFECT_ALL_UNIT_TYPES = AFFECT_MINIONS | AFFECT_HEROES | AFFECT_TURRETS,
        ALWAYS_SELF = 1u << 18,
        AFFECT_DEAD = 1u << 19,
        AFFECT_NOT_PET = 1u << 20,
        AFFECT_BARRACKS_ONLY = 1u << 21,
        IGNORE_VISIBILITY_CHECK = 1u << 22,
        NON_TARGETABLE_ALLY = 1u << 23,
        NON_TARGETABLE_ENEMY = 1u << 24,
        NON_TARGETABLE_ALL = NON_TARGETABLE_ALLY | NON_TARGETABLE_ENEMY,
        TARGETABLE_TO_ALL = 1u << 25,
        AFFECT_WARDS = 1u << 26,
        AFFECT_USEABLE = 1u << 27,
        IGNORE_ALLY_MINION = 1u << 28,
        IGNORE_ENEMY_MINION = 1u << 29,
        IGNORE_LANE_MINION = 1u << 30,
        IGNORE_CLONES = 1u << 31
    }

    public enum SpellTargetType
    {
        SELF = 0, // teemo W ; xin Q
        UNIT = 1, // Taric E ; Annie Q ; teemo Q ; xin E
        LOC_AOE = 2, // Lux E, Ziggs R
        CONE = 3, // Annie W, Kass E
        SELF_AOE = 4, // sivir R, Gangplanck E
        LOC = 6, // Ez Q, W, E, R ; Mundo Q
        LOC2 = 7  // Morg Q, Cait's Q -- These don't seem to have Missile inibins, and SpawnProjectile doesn't seem necessary to show the projectiles
    }

    public class SpellData : ISpellData
    {
        private readonly Game _game;
        private readonly ILog _logger;

        public SpellData(Game game)
        {
            _game = game;
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
        //AnimationLeadOutName
        //AnimationLoopName
        //AnimationName
        //AnimationWinddownName
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
        public float[] CastRangeDisplayOverride { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
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
        //Coefficient
        //Coefficient2
        //ConsideredAsAutoAttack
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
        public int Flags { get; set; }
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
        public float[] ManaCost { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
        //Map_X_EffectYLevelZAmmount
        public int[] MaxAmmo { get; set; } = { 0, 0, 0, 0, 0, 0, 0 };
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
        public int TargettingType { get; set; } = 1;
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

        public float GetCastTime()
        {
            return (1.0f + DelayCastOffsetPercent) / 2.0f;
        }

        public void Load(string champion, string spell)
        {
            if (string.IsNullOrEmpty(champion) || string.IsNullOrEmpty(spell))
            {
                return;
            }

            ContentFile file;
            try
            {
                var path = _game.Config.ContentManager.GetSpellDataPath(champion, spell);
                _logger.Debug($"Loading spell {spell} data from path: {path}!");
                using (var stream = new StreamReader(_game.Config.ContentManager.Content[path].ReadFile()))
                {
                    var iniParser = new FileIniDataParser();
                    var iniData = iniParser.ReadData(stream);
                    file = new ContentFile(ContentManager.ParseIniFile(iniData));
                }
            }
            catch (ContentNotFoundException)
            {
                _logger.Warn($"Spell data for {spell} was not found.");
                return;
            }

            AfterEffectName = file.GetString("SpellData", "AfterEffectName", AfterEffectName);
            //AIEndOnly
            //AILifetime
            //AIRadius
            //AIRange
            //AISendEvent
            //AISpeed
            AlternateName = file.GetString("SpellData", "AlternateName", spell);
            AlwaysSnapFacing = file.GetBool("SpellData", "AlwaysSnapFacing", AlwaysSnapFacing);
            //AmmoCountHiddenInUI
            AmmoRechargeTime = file.GetMultiFloat("SpellData", "AmmoRechargeTime", 6, AmmoRechargeTime[0]);
            AmmoUsed = file.GetMultiInt("SpellData", "AmmoUsed", 6, AmmoUsed[0]);
            //AnimationLeadOutName
            //AnimationLoopName
            //AnimationName
            //AnimationWinddownName
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
            CastRangeDisplayOverride = file.GetMultiFloat("SpellData", "CastRangeDisplayOverride", 6, CastRangeDisplayOverride[0]);
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
            //Coefficient
            //Coefficient2
            //ConsideredAsAutoAttack
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
            Flags = file.GetInt("SpellData", "Flags", Flags);
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
            ManaCost = file.GetMultiFloat("SpellData", "ManaCost", 6, ManaCost[0]);
            //Map_X_EffectYLevelZAmmount
            MaxAmmo = file.GetMultiInt("SpellData", "MaxAmmo", 6, MaxAmmo[0]);
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
            TargettingType = file.GetInt("SpellData", "TargettingType", TargettingType);
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
