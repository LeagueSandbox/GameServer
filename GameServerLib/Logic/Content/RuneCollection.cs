using System.Collections;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class RuneCollection : IEnumerable<KeyValuePair<int, int>>
    {
        private Dictionary<int, int> _runes { get; }

        public RuneCollection()
        {
            _runes = new Dictionary<int, int>();
        }

        public void Add(int runeSlotId, int runeId)
        {
            _runes.Add(runeSlotId, runeId);
        }

        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            return _runes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
