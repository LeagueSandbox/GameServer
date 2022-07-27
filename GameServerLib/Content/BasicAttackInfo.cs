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
                AttackDelayOffsetPercent = (AttackTotalTime / GlobalData.GlobalCharacterDataConstants.AttackDelay) - 1.0f;
                AttackDelayCastOffsetPercent = (atkCastTime / AttackTotalTime) - GlobalData.GlobalCharacterDataConstants.AttackDelayCastPercent;
                AttackDelayCastOffsetPercentAttackSpeedRatio = 1.0f;
            }
            else
            {
                AttackDelayCastOffsetPercent = Math.Max(atkDelayCastOffsetPercent, -GlobalData.GlobalCharacterDataConstants.AttackDelayCastPercent);
            }
        }
    }
}
