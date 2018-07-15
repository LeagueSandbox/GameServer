using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class RuneCollection
    {
        public Dictionary<int, int> Runes { get; set; }

        public RuneCollection()
        {
            Runes = new Dictionary<int, int>();
        }

        public void Add(int runeSlotId, int runeId)
        {
            Runes.Add(runeSlotId, runeId);
        }
    }
}
