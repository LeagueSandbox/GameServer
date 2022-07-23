namespace LeagueSandbox.GameServer.Content
{
    public class BasicAttackInfo
    {
        public string Name { get; init; }
        public float AttackCastTime { get; init; }
        public float AttackDelayCastOffsetPercent { get; init; }
        public float AttackDelayCastOffsetPercentAttackSpeedRatio { get; init; }
        public float AttackDelayOffsetPercent { get; init; }
        public float AttackTotalTime { get; init; }
        public float Probability { get; init; }
    }
}
