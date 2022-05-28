using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;
using System.Linq;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class StatSpeed : Stat, IStatSpeed
    {
        List<float> Slows = new List<float>();
        List<float> MultiplicativeBonuses = new List<float>();
        float _slowResist;

        public float SlowResist
        {
            get => _slowResist;
            set => _slowResist = value;
        }

        public float TotalRaw => BaseValue + FlatBonus;

        public override float Total
        {
            get
            {
                float speed = TotalRaw;
                if (speed > 490)
                {
                    speed = speed * 0.5f + 230;
                }
                else if (speed >= 415)
                {
                    speed = speed * 0.8f + 83;
                }
                else if (speed < 220)
                {
                    speed = speed * 0.5f + 110;
                }

                speed *= 1 + PercentBonus;

                if (MultiplicativeBonuses.Count > 0)
                {
                    foreach (var bonus in MultiplicativeBonuses)
                    {
                        speed *= 1 + bonus;
                    }
                }

                if (Slows.Count > 0)
                {
                    //Only takes into account the highest slow
                    speed *= 1 - (Slows.Max(z => z) * (1 - SlowResist));
                }

                return speed;
            }
        }

        public override bool ApplyStatModifier(IStatModifier statModifier)
        {
            if (!statModifier.StatModified)
            {
                return false;
            }
            BaseValue += statModifier.BaseValue;
            BaseBonus += statModifier.BaseBonus;
            PercentBaseBonus += statModifier.PercentBaseBonus;
            FlatBonus += statModifier.FlatBonus;
            if (statModifier.PercentBaseBonus >= 0)
            {
                PercentBonus += statModifier.PercentBonus;
            }
            else
            {
                Slows.Add(statModifier.PercentBonus);
            }

            if(statModifier is StatModifierSpeed speed)
            {
                SlowResist += speed.SlowResist;
            }

            return true;
        }

        public override bool RemoveStatModifier(IStatModifier statModifier)
        {
            if (!statModifier.StatModified)
            {
                return false;
            }
            BaseValue -= statModifier.BaseValue;
            BaseBonus -= statModifier.BaseBonus;
            PercentBaseBonus -= statModifier.PercentBaseBonus;
            FlatBonus -= statModifier.FlatBonus;
            if (statModifier.PercentBaseBonus >= 0)
            {
                PercentBonus -= statModifier.PercentBonus;
            }
            else
            {
                Slows.Remove(statModifier.PercentBonus);
            }

            if (statModifier is StatModifierSpeed speed)
            {
                SlowResist -= speed.SlowResist;
            }

            return true;
        }
    }
}
