﻿using System.Collections.Generic;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Content
{
    public class RuneCollection : IRuneCollection
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
