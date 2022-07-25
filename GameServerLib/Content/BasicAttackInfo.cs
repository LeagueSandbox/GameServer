using System;

namespace LeagueSandbox.GameServer.Content
{
    public class BasicAttackInfo
    {
        public string Name { get; init; }
        public float AttackCastTime { get; init; }
        public float AttackDelayCastOffsetPercent { get; private set; }
        public float AttackDelayCastOffsetPercentAttackSpeedRatio { get; private set; }
        public float AttackDelayOffsetPercent { get; private set; }
        public float AttackTotalTime { get; init; }
        public float Probability { get; init; }

        //Hardcoding a few values here temporarily until i get the GlobalConstants update through
        const float _attackDelay = 1.6f;
        const float _attackDelayCastPercent = 0.3f;

        public BasicAttackInfo(float delayCastOffset = 0, float delayCastOffsetPercent = 0, float delayCastOffsetPercentAttackSpeedRatio = 0)
        {
            AttackDelayOffsetPercent = delayCastOffset;
            AttackDelayCastOffsetPercent = delayCastOffsetPercent;
            AttackDelayCastOffsetPercentAttackSpeedRatio = delayCastOffsetPercentAttackSpeedRatio;
        }

        public void GetAttackValues()
        {
            float atkCastTime = Math.Min(AttackTotalTime, AttackCastTime);
            float atkDelayCastOffsetPercent = AttackDelayCastOffsetPercent;
            if (AttackTotalTime > 0.0f && atkCastTime > 0.0f)
            {
                AttackDelayOffsetPercent = (AttackTotalTime / _attackDelay) - 1.0f;
                AttackDelayCastOffsetPercent = (atkCastTime / AttackTotalTime) - _attackDelayCastPercent;
                AttackDelayCastOffsetPercentAttackSpeedRatio = 1.0f;
            }
            else
            {
                AttackDelayCastOffsetPercent = Math.Max(atkDelayCastOffsetPercent, -_attackDelayCastPercent);
            }
        }
    }
}
