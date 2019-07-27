using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.Content
{
    public interface ICharData
    {
        float BaseHp { get; }
        float BaseMp { get; }
        float BaseDamage { get; }
        float AttackRange { get; }
        int MoveSpeed { get; }
        float Armor { get; }
        float SpellBlock { get; }
        float BaseStaticHpRegen { get; }
        float BaseStaticMpRegen { get; }
        float AttackDelayOffsetPercent { get; }
        float AttackDelayCastOffsetPercent { get; }
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
        float GameplayCollisionRadius { get; }
        PrimaryAbilityResourceType ParType { get; }
        string[] SpellNames { get; }
        int[] MaxLevels { get; }
        int[][] SpellsUpLevels { get; }
        string[] ExtraSpells { get; }
        IPassiveData[] Passives { get; }
        void Load(string name);
    }
}