using System;

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

        public float Unk;
        public float Unk2;
        public float Unk3;

        //Hardcoding a few values here temporarily until i get the GlobalConstants update through
        const float _attackDelay = 1.6f;
        const float _attackDelayCastPercent = 0.3f;

        public void GetAttackValues()
        {
            var atkCastTime = Math.Min(AttackTotalTime, AttackCastTime);
            if (AttackTotalTime > 0.0f && atkCastTime > 0.0f)
            {
                Unk = (AttackTotalTime / _attackDelay) - 1.0f;
                Unk2 = (atkCastTime / AttackTotalTime) - _attackDelayCastPercent;
                Unk3 = 1.0f;
            }
            else
            {
                Unk = AttackDelayOffsetPercent;
                Unk2 = Math.Max(AttackDelayCastOffsetPercent, -_attackDelayCastPercent);
                Unk3 = AttackDelayCastOffsetPercentAttackSpeedRatio;
            }
        }
    }
}
