using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public abstract class Replication
    {
        public class Replicate
        {
            public uint Value { get; set; }
            public bool IsFloat { get; set; }
            public bool Changed { get; set; }
        }

        protected Replication(AttackableUnit owner)
        {
            Owner = owner;
            Update();
        }

        protected readonly AttackableUnit Owner;
        protected Stats Stats => Owner.Stats;

        public UInt32 NetId => Owner.NetId;
        public Replicate[,] Values { get; private set; } = new Replicate[6, 32];
        public bool Changed { get; set; }

        private void DoUpdate(uint value, int primary, int secondary, bool isFloat)
        {
            if (Values[primary, secondary] == null)
            {
                Values[primary, secondary] = new Replicate
                {
                    Value = value,
                    IsFloat = isFloat,
                    Changed = true
                };
                Changed = true;
            }
            else if (Values[primary, secondary].Value != value)
            {
                Values[primary, secondary].IsFloat = isFloat;
                Values[primary, secondary].Value = value;
                Values[primary, secondary].Changed = true;
                Changed = true;
            }
        }

        protected void UpdateUint(uint value, int primary, int secondary)
        {
            DoUpdate(value, primary, secondary, false);
        }

        protected void UpdateInt(int value, int primary, int secondary)
        {
            DoUpdate((uint)value, primary, secondary, false);
        }

        protected void UpdateBool(bool value, int primary, int secondary)
        {
            DoUpdate(value ? 1u : 0u, primary, secondary, false);
        }

        protected void UpdateFloat(float value, int primary, int secondary)
        {
            DoUpdate(BitConverter.ToUInt32(BitConverter.GetBytes(value), 0), primary, secondary, true);
        }

        public abstract void Update();
    }
}