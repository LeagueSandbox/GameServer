using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class ReplicationAiTurret : Replication
    {
        public ReplicationAiTurret(IBaseTurret owner) : base(owner)
        {

        }
        public override void Update()
        {
            // UpdateFloat(Stats.ManaPoints.Total, 1, 0); //mMaxMP
            // UpdateFloat(Stats.CurrentMana, 1, 1); //mMP
            UpdateUint((uint)Stats.ActionState, 1, 2); //ActionState
            UpdateBool(Stats.IsMagicImmune, 1, 3); //MagicImmune
            UpdateBool(Stats.IsInvulnerable, 1, 4); //IsInvulnerable
            UpdateBool(Stats.IsPhysicalImmune, 1, 5); //IsPhysicalImmune
            UpdateBool(Stats.IsLifestealImmune, 1, 6); //IsLifestealImmune
            UpdateFloat(Stats.AttackDamage.BaseValue, 1, 7); //mBaseAttackDamage
            UpdateFloat(Stats.Armor.Total, 1, 8); //mArmor
            UpdateFloat(Stats.MagicResist.Total, 1, 9); //mSpellBlock
            UpdateFloat(Stats.AttackSpeedMultiplier.Total, 1, 10); //mAttackSpeedMod
            UpdateFloat(Stats.AttackDamage.FlatBonus, 1, 11); //mFlatPhysicalDamageMod
            UpdateFloat(Stats.AttackDamage.PercentBonus, 1, 12); //mPercentPhysicalDamageMod
            UpdateFloat(Stats.AbilityPower.Total, 1, 13); //mFlatMagicDamageMod
            UpdateFloat(Stats.HealthRegeneration.Total, 1, 14); //mHPRegenRate
            UpdateFloat(Stats.CurrentHealth, 3, 0); //mHP
            UpdateFloat(Stats.HealthPoints.Total, 3, 1); //mMaxHP
            // UpdateFloat(Stats.PerceptionRange.FlatBonus, 3, 2); //mFlatBubbleRadiusMod
            // UpdateFloat(Stats.PerceptionRange.PercentBonus, 3, 3); //mPercentBubbleRadiusMod
            UpdateFloat(Stats.MoveSpeed.Total, 3, 4); //mMoveSpeed
            UpdateFloat(Stats.Size.Total, 3, 5); //mSkinScaleCoef(mistyped as mCrit)
            UpdateBool(Stats.IsTargetable, 5, 0); //mIsTargetable
            UpdateUint((uint)Stats.IsTargetableToTeam, 5, 1); //mIsTargetableToTeamFlags
        }
    }
}
