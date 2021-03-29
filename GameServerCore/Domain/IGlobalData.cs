namespace LeagueSandbox.GameServer.Content
{
    public interface IGlobalData
    {
        float AttackDelay { get; set; }
        float AttackDelayCastPercent { get; set; }
        float AttackMinDelay { get; set; }
        float PercentAttackSpeedModMinimum { get; set; }
        float AttackMaxDelay { get; set; }
        float CooldownMinimum { get; set; }
        float PercentRespawnTimeModMinimum { get; set; }
        float PercentGoldLostOnDeathModMinimum { get; set; }
        float PercentEXPBonusMinimum { get; set; }
        float PercentEXPBonusMaximum { get; set; }
    }
}