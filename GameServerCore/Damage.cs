using GameServerCore.Enums;

namespace GameServerCore {

    public struct Damage
    {
        public Damage(float value, DamageType type, DamageSource source, bool isCrit)
        {
            DamageValue = value;
            DamageType = type;
            DamageSource = source;
            IsCritical = isCrit;
        }

        public float DamageValue { get; set; }
        public DamageType DamageType { get; set; }
        public DamageSource DamageSource { get; set; }
        public bool IsCritical { get; set; }
    }

}
