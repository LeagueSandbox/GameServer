using IntWarsSharp.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    class ChampionFactory
    {
        public static Champion getChampionFromType(string type, Map map, int id, int playerId)
        {
            return new Champion(type, map, id, playerId);
        }
    }
}
