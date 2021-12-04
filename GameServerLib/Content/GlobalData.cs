using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Content
{
    public class GlobalData : IGlobalData
    {
        // TODO: Add Constants.var files to each Map's Content folder and assign values by reading them, currently this Data is only for Map1 as a placeholder.
        public float AttackDelay { get; set; } = 1.600f;
        public float AttackDelayCastPercent { get; set; } = 0.300f;
        public float AttackMinDelay { get; set; } = 0.400f;
        public float PercentAttackSpeedModMinimum { get; set; } = -0.950f;
        public float AttackMaxDelay { get; set; } = 5.000f;
        public float CooldownMinimum { get; set; } = 0.000f;
        //Ideally CDR would be a negative value
        public float PercentCooldownModMinimun { get; set; } = 0.400f;
        public float PercentRespawnTimeModMinimum { get; set; } = -0.950f;
        public float PercentGoldLostOnDeathModMinimum { get; set; } = -0.950f;
        public float PercentEXPBonusMinimum { get; set; } = -1.000f;
        public float PercentEXPBonusMaximum { get; set; } = 5.000f;
    }
}
