namespace GameServerCore.Domain.GameObjects
{
    public interface IChampionStats
    {
        int Kills { get; set; }
        int DoubleKills { get; set; }
        int TripleKills { get; set; }
        int QuadraKills { get; set; }
        int PentaKills { get; set; }
        int UnrealKills { get; set; }
        int Deaths { get; set; }
        int Assists { get; set; }
        int CurrentKillingSpree { get; set; }
        int LargestKillingSpree { get; set; }
        int LargestMultiKill { get; set; }
        int MinionsKilled { get; set; }
        int NeutralMinionsKilled { get; set; }
        int NeutralMinionsKilledInEnemyJungle { get; set; }
        int NeutralMinionsKilledInTeamJungle { get; set; }
        int TurretsKilled { get; set; }
        int BarracksKilled { get; set; }
        int WardsKilled { get; set; }
        int WardsPlaced { get; set; }
        int TeamId { get; set; }
        int TotalHeal { get; set; }
        int TotalUnitsHealed { get; set; }
        
        float GoldEarned { get; set; }
        float GoldSpent { get; set; }
        float LargestCriticalStrike { get; set; }
        float LongestTimeSpentLiving { get; set; }
        float MagicDamageDealt { get; set; }
        float MagicDamageDealtToChampions { get; set; }
        float MagicDamageTaken { get; set; }
        float PhysicalDamageDealt { get; set; }
        float PhysicalDamageDealtToChampions { get; set; }
        float PhysicalDamageTaken { get; set; }
        float TotalDamageDealt { get; set; }
        float TotalDamageDealtToChampions { get; set; }
        float TotalDamageTaken { get; set; }
        float TotalTimeCrowdControlDealt { get; set; }
        float TotalTimeSpentDead { get; set; }
        float TrueDamageDealt { get; set; }
        float TrueDamageDealtToChampions { get; set; }
        float TrueDamageTaken { get; set; }

        byte[] GetBytes();
    }
}
