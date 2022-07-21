namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface IBasicAttackInfo
    {
        string Name { get; }
        float AttackCastTime { get; }
        float AttackDelayCastOffsetPercent { get; }
        float AttackDelayCastOffsetPercentAttackSpeedRatio { get; }
        float AttackDelayOffsetPercent { get; }
        float AttackTotalTime { get; }
        float Probability { get; }
    }
}
