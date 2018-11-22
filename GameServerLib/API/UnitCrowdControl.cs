using GameServerCore.Domain;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.API
{
    public class UnitCrowdControl: ICrowdControl
    {
        public CrowdControlType Type { get; }
        public float Duration { get; }
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
