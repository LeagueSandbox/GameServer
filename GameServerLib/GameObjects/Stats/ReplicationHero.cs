using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class ReplicationHero : Replication
    {
        public ReplicationHero(Champion owner) : base(owner)
        {

        }
        public override void Update()
        {
            UpdateFloat(Stats.Gold, 0, 0); //mGold
            // UpdateFloat(Stats.TotalGold, 0, 1); //mGoldTotal
            UpdateUint((uint)Stats.SpellsEnabled, 0, 2); //mReplicatedSpellCanCastBitsLower1
            UpdateUint((uint)(Stats.SpellsEnabled >> 32), 0, 3); //mReplicatedSpellCanCastBitsUpper1
            UpdateUint((uint)Stats.SummonerSpellsEnabled, 0, 4); //mReplicatedSpellCanCastBitsLower2
            UpdateUint((uint)(Stats.SummonerSpellsEnabled >> 32), 0, 5); //mReplicatedSpellCanCastBitsUpper2
            // UpdateUint(Stats.EvolvePoints, 0, 6); //mEvolvePoints
            // UpdateUint(Stats.EvolveFlags, 0, 7); //mEvolveFlag
            for (var i = 0; i < 4; i++)
            {
                UpdateFloat(Stats.ManaCost[i], 0, 8 + i); //ManaCost_{i}
            }
            for(var i = 0; i < 16; i++)
            {
                UpdateFloat(Stats.ManaCost[45 + i], 0, 12 + i); //ManaCost_Ex{i}
            }
            UpdateUint((uint)Stats.ActionState, 1, 0); //ActionState
            UpdateBool(Stats.IsMagicImmune, 1, 1); //MagicImmune
            UpdateBool(Stats.IsInvulnerable, 1, 2); //IsInvulnerable
            UpdateBool(Stats.IsPhysicalImmune, 1, 3); //IsPhysicalImmune
            UpdateBool(Stats.IsLifestealImmune, 1, 4); //IsLifestealImmune
            UpdateFloat(Stats.AttackDamage.BaseValue, 1, 5); //mBaseAttackDamage
            UpdateFloat(Stats.AbilityPower.BaseValue, 1, 6); //mBaseAbilityDamage
            // UpdateFloat(Stats.DodgeChance, 1, 7); //mDodge
            UpdateFloat(Stats.CriticalChance.Total, 1, 8); //mCrit
            UpdateFloat(Stats.Armor.Total, 1, 9); //mArmor
            UpdateFloat(Stats.MagicResist.Total, 1, 10); //mSpellBlock
            UpdateFloat(Stats.HealthRegeneration.Total, 1, 11); //mHPRegenRate
            UpdateFloat(Stats.ManaRegeneration.Total, 1, 12); //mPARRegenRate
            UpdateFloat(Stats.Range.Total, 1, 13); //mAttackRange
            UpdateFloat(Stats.AttackDamage.FlatBonus, 1, 14); //mFlatPhysicalDamageMod
            UpdateFloat(Stats.AttackDamage.PercentBonus, 1, 15); //mPercentPhysicalDamageMod
            UpdateFloat(Stats.AbilityPower.FlatBonus, 1, 16); //mFlatMagicDamageMod
            // UpdateFloat(Stats.MagicResist.FlatBonus, 1, 17); //mFlatMagicReduction
            // UpdateFloat(Stats.MagicResist.PercentBonus, 1, 18); //mPercentMagicReduction
            UpdateFloat(Stats.AttackSpeedMultiplier.Total, 1, 19); //mAttackSpeedMod
            UpdateFloat(Stats.Range.FlatBonus, 1, 20); //mFlatCastRangeMod
            UpdateFloat(Stats.CooldownReduction.Total, 1, 21); //mPercentCooldownMod
            // UpdateFloat(Stats.PassiveCooldownEndTime, 1, 22); //mPassiveCooldownEndTime
            // UpdateFloat(Stats.PassiveCooldownTotalTime, 1, 23); //mPassiveCooldownTotalTime
            UpdateFloat(Stats.ArmorPenetration.FlatBonus, 1, 24); //mFlatArmorPenetration
            UpdateFloat(Stats.ArmorPenetration.PercentBonus, 1, 25); //mPercentArmorPenetration
            UpdateFloat(Stats.MagicPenetration.FlatBonus, 1, 26); //mFlatMagicPenetration
            UpdateFloat(Stats.MagicPenetration.PercentBonus, 1, 27); //mPercentMagicPenetration
            UpdateFloat(Stats.LifeSteal.Total, 1, 28); //mPercentLifeStealMod
            UpdateFloat(Stats.SpellVamp.Total, 1, 29); //mPercentSpellVampMod
            UpdateFloat(Stats.Tenacity.Total, 1, 30); //mPercentCCReduction
            UpdateFloat(Stats.Armor.PercentBonus, 2, 0); //mPercentBonusArmorPenetration
            UpdateFloat(Stats.MagicPenetration.PercentBonus, 2, 1); //mPercentBonusMagicPenetration
            // UpdateFloat(Stats.HealthRegeneration.BaseValue, 2, 2); //mBaseHPRegenRate
            // UpdateFloat(Stats.ManaRegeneration.BaseValue, 2, 3); //mBasePARRegenRate
            UpdateFloat(Stats.CurrentHealth, 3, 0); //mHP
            UpdateFloat(Stats.CurrentMana, 3, 1); //mMP
            UpdateFloat(Stats.HealthPoints.Total, 3, 2); //mMaxHP
            UpdateFloat(Stats.ManaPoints.Total, 3, 3); //mMaxMP
            UpdateFloat(Stats.Experience, 3, 4); //mExp
            // UpdateFloat(Stats.LifeTime, 3, 5); //mLifetime
            // UpdateFloat(Stats.MaxLifeTime, 3, 6); //mMaxLifetime
            // UpdateFloat(Stats.LifeTimeTicks, 3, 7); //mLifetimeTicks
            // UpdateFloat(Stats.PerceptionRange.FlatMod, 3, 8); //mFlatBubbleRadiusMod
            // UpdateFloat(Stats.PerceptionRange.PercentMod, 3, 9); //mPercentBubbleRadiusMod
            UpdateFloat(Stats.MoveSpeed.Total, 3, 10); //mMoveSpeed
            UpdateFloat(Stats.Size.Total, 3, 11); //mSkinScaleCoef(mistyped as mCrit)
            // UpdateFloat(Stats.FlatPathfindingRadiusMod, 3, 12); //mPathfindingRadiusMod
            UpdateUint(Stats.Level, 3, 13); //mLevelRef
            UpdateUint((uint)Owner.MinionCounter, 3, 14); //mNumNeutralMinionsKilled
            UpdateBool(Stats.IsTargetable, 3, 15); //mIsTargetable
            UpdateUint((uint)Stats.IsTargetableToTeam, 3, 16); //mIsTargetableToTeamFlags
        }
    }
}
