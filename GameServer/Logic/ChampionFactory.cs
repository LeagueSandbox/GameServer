using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic
{
    class ChampionFactory
    {
        public static Champion getChampionFromType(string type, Map map, int id, int playerId)
        {
            return new Champion(type, map, id, playerId);
        }
    }
}
