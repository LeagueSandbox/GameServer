namespace GameServerCore.Domain.GameObjects
{
    public interface IChampionStats
    {
        int Assists { get; }
        int Kills { get; }
        int DoubleKills { get; }
        int UnrealKills { get; }
        float GoldEarned { get; }
        float GoldSpent { get; }
        int CurrentKillingSpree { get; }
        float LargestCriticalStrike { get; }
        int LargestKillingSpree { get; }
        int LargestMultiKill { get; }
        float LongestTimeSpentLiving { get; }
        float MagicDamageDealt { get; }
        float MagicDamageDealtToChampions { get; }
        float MagicDamageTaken { get; }
        int MinionsKilled { get; }
        int NeutralMinionsKilled { get; }
        int NeutralMinionsKilledInEnemyJungle { get; }
        int NeutralMinionsKilledInTeamJungle { get; }
        int Deaths { get; }
        int PentaKills { get; }
        float PhysicalDamageDealt { get; }
        float PhysicalDamageDealtToChampions { get; }
        float PhysicalDamageTaken { get; }
        int QuadraKills { get; }
        int TeamId { get; }
        float TotalDamageDealt { get; }
        float TotalDamageDealtToChampions { get; }
        float TotalDamageTaken { get; }
        int TotalHeal { get; }
        float TotalTimeCrowdControlDealt { get; }
        float TotalTimeSpentDead { get; }
        int TotalUnitsHealed { get; }
        int TripleKills { get; }
        float TrueDamageDealt { get; }
        float TrueDamageDealtToChampions { get; }
        float TrueDamageTaken { get; }
        int TurretsKilled { get; }
        int BarracksKilled { get; }
        int WardsKilled { get; }
        int WardsPlaced { get; }

        byte[] GetBytes();
    }
}
