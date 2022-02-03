using System;
using System.Collections.Generic;
using System.Text;

namespace GameServerCore.Scripting.CSharp
{
    public interface IScriptToolTipData
    {
        public byte SpeelIndex { get; }

        public ToolTipValue[] Values { get; }

        public bool HasUpdates();
    }

    public struct ToolTipValue
    {
        private float _value;
        private bool _hide;

        public float Value
        {
            get => _value; 
            set
            {
                if (value == _value)
                    return;
                _value = value;
                IsUpdated = true;
            }
        }
        public bool  Hide { get => _hide;
            set
            {
                if (value == _hide)
                    return;
                _hide = value;
                IsUpdated = true;
            }

        }

        public bool IsUpdated { get; set; }
    }
}
