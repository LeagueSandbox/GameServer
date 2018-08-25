﻿namespace LeagueSandbox.GameServer.API
{
    public enum CrowdControlType
    {
        AIRBORNE, BLIND, DISARM, GROUND, INVULNERABLE, NEARSIGHT, ROOT, SILENCE, STASIS, STUN, SUPPRESSION, SNARE
    }

    public class UnitCrowdControl
    {
        public CrowdControlType Type { get; private set; }
        public float Duration { get; private set; }
        public float CurrentTime { get; private set; }
        public bool IsRemoved { get; private set; }

        public UnitCrowdControl(CrowdControlType type, float duration = -1)
        {
            Type = type;
            Duration = duration;
        }

        public void Update(float diff)
        {
            CurrentTime += diff / 1000.0f;
            if (CurrentTime >= Duration && !IsRemoved && Duration != -1)
            {
                IsRemoved = true;
            }
        }

        public bool IsTypeOf(CrowdControlType type)
        {
            return type == Type;
        }
    }
}
