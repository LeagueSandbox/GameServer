﻿using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface ICharData
    {
        IGlobalData GlobalCharData { get; }
        float BaseHp { get; }
        float BaseMp { get; }
        float BaseDamage { get; }
        float AttackRange { get; }
        int MoveSpeed { get; }
        float Armor { get; }
        float SpellBlock { get; }
        float BaseStaticHpRegen { get; }
        float BaseStaticMpRegen { get; }
        float[] AttackDelayOffsetPercent { get; }
        float[] AttackDelayCastOffsetPercent { get; }
        float[] AttackDelayCastOffsetPercentAttackSpeedRatio { get; }
        float HpPerLevel { get; }
        float MpPerLevel { get; }
        float DamagePerLevel { get; }
        float ArmorPerLevel { get; }
        float SpellBlockPerLevel { get; }
        float HpRegenPerLevel { get; }
        float MpRegenPerLevel { get; }
        float AttackSpeedPerLevel { get; }
        bool IsMelee { get; } //Yes or no
        float PathfindingCollisionRadius { get; }
        float PerceptionBubbleRadius { get; }
        float GameplayCollisionRadius { get; }
        PrimaryAbilityResourceType ParType { get; }
        string[] SpellNames { get; }
        int[] MaxLevels { get; }
        int[][] SpellsUpLevels { get; }
        string[] AttackNames { get; }
        float[] AttackProbabilities { get; }
        string[] ExtraSpells { get; }
        // TODO: Verify if we want this to be an array.
        IPassiveData PassiveData { get; }
        void Load(string name);
    }
}