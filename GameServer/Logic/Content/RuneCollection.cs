using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class RuneCollection
    {
        public Dictionary<int, int> _runes { get; set; }

        public RuneCollection()
        {
            _runes = new Dictionary<int, int>();
        }

        public void Add(int runeSlotId, int runeId)
        {
            _runes.Add(runeSlotId, runeId);
        }
    }
}
