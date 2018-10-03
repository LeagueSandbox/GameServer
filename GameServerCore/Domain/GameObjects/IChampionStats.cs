namespace GameServerCore.Domain.GameObjects
{
    public interface IChampionStats
    {
        int Assists { get; set; }
        int Kills { get; set; }
        int DoubleKills { get; set; }
        int UnrealKills { get; set; }
        float GoldEarned { get; set; }
        float GoldSpent { get; set; }
        int CurrentKillingSpree { get; set; }
        float LargestCriticalStrike { get; set; }
        int LargestKillingSpree { get; set; }
        int LargestMultiKill { get; set; }
        float LongestTimeSpentLiving { get; set; }
        float MagicDamageDealt { get; set; }
        float MagicDamageDealtToChampions { get; set; }
        float MagicDamageTaken { get; set; }
        int MinionsKilled { get; set; }
        int NeutralMinionsKilled { get; set; }
        int NeutralMinionsKilledInEnemyJungle { get; set; }
        int NeutralMinionsKilledInTeamJungle { get; set; }
        int Deaths { get; set; }
        int PentaKills { get; set; }
        float PhysicalDamageDealt { get; set; }
        float PhysicalDamageDealtToChampions { get; set; }
        float PhysicalDamageTaken { get; set; }
        int QuadraKills { get; set; }
        int TeamId { get; set; }
        float TotalDamageDealt { get; set; }
        float TotalDamageDealtToChampions { get; set; }
        float TotalDamageTaken { get; set; }
        int TotalHeal { get; set; }
        float TotalTimeCrowdControlDealt { get; set; }
        float TotalTimeSpentDead { get; set; }
        int TotalUnitsHealed { get; set; }
        int TripleKills { get; set; }
        float TrueDamageDealt { get; set; }
        float TrueDamageDealtToChampions { get; set; }
        float TrueDamageTaken { get; set; }
        int TurretsKilled { get; set; }
        int BarracksKilled { get; set; }
        int WardsKilled { get; set; }
        int WardsPlaced { get; set; }

        byte[] GetBytes();
    }
}
