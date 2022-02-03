using GameServerCore.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeagueSandbox.GameServer.GameObjects.Spell
{
    public class ToolTipData : IScriptToolTipData
    {
        private Spell _speel;

        public byte SpeelIndex { get => (byte)(_speel.CastInfo.SpellSlot + 60) ; }

        public ToolTipValue[] Values { get; protected set; }

        public bool HasUpdates()
        {
            var list = Values.Where(s => s.IsUpdated).ToArray();
            for (int x = 0; x < list.Count(); x++)
                list[x].IsUpdated = false;
            return list.Count() > 0;
        }

        public ToolTipData(Spell OwnerSpeel)
        {
            this._speel = OwnerSpeel;
            Values = new ToolTipValue[16];
        }
    }
}
