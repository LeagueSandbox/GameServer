using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class RuneCollection
    {
        private Dictionary<int, int> _runes;

        public RuneCollection()
        {
            _runes = new Dictionary<int, int>();
        }

        public Dictionary<int, int> GetRunes()
        {
            return _runes;
        }

        public void Add(int runeSlotId, int runeId)
        {
            _runes.Add(runeSlotId, runeId);
        }
    }
}
