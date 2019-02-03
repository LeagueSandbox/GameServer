using System;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public abstract class Replication : IReplication
    {
        public class Replicate : IReplicate
        {
            public uint Value { get; set; }
            public bool IsFloat { get; set; }
            public bool Changed { get; set; }
        }

        protected Replication(IAttackableUnit owner)
        {
            Owner = owner;
            Update();
        }

        protected readonly IAttackableUnit Owner;
        protected IStats Stats => Owner.Stats;

        public uint NetId => Owner.NetId;
        public Replicate[,] Values { get; private set; } = new Replicate[6, 32];
        public bool Changed { get; private set; }

        IReplicate[,] IReplication.Values => Values;

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

        public void MarkAsUnchanged()
        {
            foreach (var x in Values)
            {
                if (x != null) 
                    x.Changed = false;
            }

            Changed = false;
        }

        public abstract void Update();
    }
}