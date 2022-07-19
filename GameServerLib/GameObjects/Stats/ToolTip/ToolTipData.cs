using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace LeagueSandbox.GameServer.GameObjects.StatsNS
{
    // TODO: Cleanup
    public class ToolTipData
    {
        public class ToolTipValue
        {
            public bool Hide { get; set; } = false;
            public float Value { get; set; } = 0.0f;
            public bool IsFloat { get; set; } = false;
            public bool Changed { get; set; } = false;

            public ToolTipValue() { }
        }

        protected readonly AttackableUnit Owner;
        protected readonly Spell Spell;
        protected readonly Buff Buff;

        public uint NetID => Owner.NetId;
        public byte Slot { get; private set; }
        // 16 ToolTip slots per spell.
        public ToolTipValue[] Values { get; private set; } = new ToolTipValue[16];
        public bool Changed { get; private set; }

        public ToolTipData(AttackableUnit owner, Spell spell, Buff buff = null)
        {
            Populate(Values);

            Owner = owner;
            Spell = spell;
            Buff = buff;

            if (spell != null)
            {
                // The slots seem to start at 60 for spell tooltips.
                Slot = (byte)(Spell.CastInfo.SpellSlot + 60);
            }
            else if (buff != null)
            {
                // Slots start at 0 for buff tooltips.
                Slot = buff.Slot;
            }

            // NOTE Client behavior with higher slots: Slot > 120 => Slot - 120 => 120 > Slot > 59
            // End result is counted as a spell tooltip as slot is read from 60.
        }

        private void DoUpdate(float value, int primary, bool isFloat, bool isHidden = false)
        {
            if (Values[primary] == null)
            {
                Values[primary] = new ToolTipValue
                {
                    Hide = isHidden,
                    Value = value,
                    IsFloat = isFloat,
                    Changed = true
                };
                Changed = true;
            }
            else if (Values[primary].Value != value)
            {
                Values[primary].Hide = isHidden;
                Values[primary].IsFloat = isFloat;
                Values[primary].Value = value;
                Values[primary].Changed = true;
                Changed = true;
            }
        }

        protected void UpdateUint(uint value, int primary)
        {
            DoUpdate(value, primary, false);
        }

        protected void UpdateInt(int value, int primary)
        {
            DoUpdate(value, primary, false);
        }

        protected void UpdateBool(bool value, int primary)
        {
            // TODO: Verify
            DoUpdate(value ? 1f : 0f, primary, false);
        }

        protected void UpdateFloat(float value, int primary)
        {
            DoUpdate(value, primary, true);
        }

        // TODO: Find a use case for this.
        public void MarkAsUnchanged()
        {
            foreach (var x in Values)
            {
                if (x != null)
                {
                    x.Changed = false;
                }
            }

            Changed = false;
        }

        public void Update<T>(int tipIndex, T value) where T : struct
        {
            if (value is bool boolVal)
            {
                UpdateBool(boolVal, tipIndex);
            }
            else if (value is int intVal)
            {
                UpdateInt(intVal, tipIndex);
            }
            else if (value is uint uintVal)
            {
                UpdateUint(uintVal, tipIndex);
            }
            else if (value is float floatVal)
            {
                UpdateFloat(floatVal, tipIndex);
            }
        }

        public static void Populate(ToolTipValue[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new ToolTipValue();
            }
        }
    }
}
