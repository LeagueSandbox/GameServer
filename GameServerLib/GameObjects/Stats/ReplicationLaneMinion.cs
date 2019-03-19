using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class ReplicationLaneMinion : Replication
    {
        public ReplicationLaneMinion(LaneMinion owner) : base(owner)
        {

        }
        public override void Update()
        {
            UpdateFloat(Stats.CurrentHealth, 1, 0); //mHP
            UpdateFloat(Stats.HealthPoints.Total, 1, 1); //mMaxHP
            // UpdateFloat(Stats.LifeTime, 1, 2); //mLifetime
            // UpdateFloat(Stats.MaxLifeTime, 1, 3); //mMaxLifetime
            // UpdateFloat(Stats.LifeTimeTicks, 1, 4); //mLifetimeTicks
            // UpdateFloat(Stats.ManaPoints.Total, 1, 5); //mMaxMP
            // UpdateFloat(Stats.CurrentMana, 1, 6); //mMP
            UpdateUint((uint)Stats.ActionState, 1, 7); //ActionState
            UpdateBool(Stats.IsMagicImmune, 1, 8); //MagicImmune
            UpdateBool(Stats.IsInvulnerable, 1, 9); //IsInvulnerable
            UpdateBool(Stats.IsPhysicalImmune, 1, 10); //IsPhysicalImmune
            UpdateBool(Stats.IsLifestealImmune, 1, 11); //IsLifestealImmune
            UpdateFloat(Stats.AttackDamage.BaseValue, 1, 12); //mBaseAttackDamage
            UpdateFloat(Stats.Armor.Total, 1, 13); //mArmor
            UpdateFloat(Stats.MagicResist.Total, 1, 14); //mSpellBlock
            UpdateFloat(Stats.AttackSpeedMultiplier.Total, 1, 15); //mAttackSpeedMod
            UpdateFloat(Stats.AttackDamage.FlatBonus, 1, 16); //mFlatPhysicalDamageMod
            UpdateFloat(Stats.AttackDamage.PercentBonus, 1, 17); //mPercentPhysicalDamageMod
            UpdateFloat(Stats.AbilityPower.Total, 1, 18); //mFlatMagicDamageMod
            UpdateFloat(Stats.HealthRegeneration.Total, 1, 19); //mHPRegenRate
            UpdateFloat(Stats.ManaRegeneration.Total, 1, 20); //mPARRegenRate
            UpdateFloat(Stats.MagicResist.FlatBonus, 1, 21); //mFlatMagicReduction
            UpdateFloat(Stats.MagicResist.PercentBonus, 1, 22); //mPercentMagicReduction
            // UpdateFloat(Stats.PerceptionRange.FlatBonus, 3, 0); //mFlatBubbleRadiusMod
            // UpdateFloat(Stats.PerceptionRange.PercentBonus, 3, 1); //mPercentBubbleRadiusMod
            UpdateFloat(Stats.MoveSpeed.Total, 3, 2); //mMoveSpeed
            UpdateFloat(Stats.Size.Total, 3, 3); //mSkinScaleCoef(mistyped as mCrit)
            UpdateBool(Stats.IsTargetable, 3, 4); //mIsTargetable
            UpdateUint((uint)Stats.IsTargetableToTeam, 3, 5); //mIsTargetableToTeamFlags
        }
    }
}
