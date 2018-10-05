using System.Numerics;

namespace GameServerCore.Domain
{
    public interface ISpellData
    {
        string AfterEffectName { get; }
        string AlternateName { get; }
        bool AlwaysSnapFacing { get; }
        float[] AmmoRechargeTime { get; }
        int[] AmmoUsed { get; }
        bool BelongsToAvatar { get; }
        float BounceRadius { get; }
        bool CanCastWhileDisabled { get; }
        float CancelChargeOnRecastTime { get; }
        bool CanMoveWhileChanneling { get; }
        bool CannotBeSuppressed { get; }
        bool CanOnlyCastWhileDead { get; }
        bool CanOnlyCastWhileDisabled { get; }
        bool CantCancelWhileChanneling { get; }
        bool CantCancelWhileWindingUp { get; }
        bool CantCastWhileRooted { get; }
        float CastConeAngle { get; }
        float CastConeDistance { get; }
        float CastFrame { get; }
        float[] CastRadius { get; }
        float[] CastRadiusSecondary { get; }
        float[] CastRange { get; }
        float[] CastRangeDisplayOverride { get; }
        float[] CastRangeGrowthDuration { get; }
        float[] CastRangeGrowthMax { get; }
        bool CastRangeUseBoundingBoxes { get; }
        float CastTargetAdditionalUnitsRadius { get; }
        int CastType { get; }
        float[] ChannelDuration { get; }
        float ChargeUpdateInterval { get; }
        float CircleMissileAngularVelocity { get; }
        float CircleMissileRadialVelocity { get; }
        float[] Cooldown { get; }
        int DeathRecapPriority { get; }
        float DelayCastOffsetPercent { get; }
        float DelayTotalTimePercent { get; }
        bool DoesntBreakChannels { get; }
        bool DoNotNeedToFaceTarget { get; }
        int Flags { get; }
        bool HaveAfterEffect { get; }
        bool HaveHitBone { get; }
        bool HaveHitEffect { get; }
        bool HavePointEffect { get; }
        string HitBoneName { get; }
        string HitEffectName { get; }
        int HitEffectOrientType { get; }
        bool IgnoreAnimContinueUntilCastFrame { get; }
        bool IgnoreRangeCheck { get; }
        bool IsDisabledWhileDead { get; }
        bool IsToggleSpell { get; }
        float LineDragLength { get; }
        int LineMissileBounces { get; }
        bool LineMissileCollisionFromStartPoint { get; }
        float LineMissileDelayDestroyAtEndSeconds { get; }
        bool LineMissileEndsAtTargetPoint { get; }
        bool LineMissileFollowsTerrainHeight { get; }
        float LineMissileTargetHeightAugment { get; }
        float LineMissileTimePulseBetweenCollisionSpellHits { get; }
        bool LineMissileTrackUnits { get; }
        bool LineMissileUsesAccelerationForBounce { get; }
        float LineWidth { get; }
        float[] LocationTargettingLength { get; }
        float[] LocationTargettingWidth { get; }
        bool LockConeToPlayer { get; }
        float LuaOnMissileUpdateDistanceInterval { get; }
        float[] ManaCost { get; }
        int[] MaxAmmo { get; }
        float MissileAccel { get; }
        string MissileBoneName { get; }
        string MissileEffect { get; }
        string MissileEffectPlayer { get; }
        float MissileFixedTravelTime { get; }
        bool MissileFollowsTerrainHeight { get; }
        float MissileGravity { get; }
        float MissileLifetime { get; }
        float MissileMaxSpeed { get; }
        float MissileMinSpeed { get; }
        float MissileMinTravelTime { get; }
        float MissilePerceptionBubbleRadius { get; }
        bool MissilePerceptionBubbleRevealsStealth { get; }
        float MissileSpeed { get; }
        float MissileTargetHeightAugment { get; }
        bool MissileUnblockable { get; }
        bool NoWinddownIfCancelled { get; }
        float OverrideCastTime { get; }
        Vector3 ParticleStartOffset { get; }
        string PointEffectName { get; }
        string RequiredUnitTags { get; }
        string SelectionPreference { get; }
        float SpellCastTime { get; }
        float SpellRevealsChampion { get; }
        float SpellTotalTime { get; }
        float StartCooldown { get; }
        bool SubjectToGlobalCooldown { get; }
        int TargettingType { get; }
        string TextFlags { get; }
        bool TriggersGlobalCooldown { get; }
        bool UpdateRotationWhenCasting { get; }
        bool UseAnimatorFramerate { get; }
        bool UseAutoattackCastTime { get; }
        bool UseChargeChanneling { get; }
        bool UseChargeTargeting { get; }
        bool UseGlobalLineIndicator { get; }
        bool UseMinimapTargeting { get; }

        float GetCastTime();
        void Load(string name);
    }
}
