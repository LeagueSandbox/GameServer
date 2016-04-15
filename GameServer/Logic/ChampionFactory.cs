using LeagueSandbox.GameServer.Core.Logic;
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
        public static Champion getChampionFromType(Game game, string type, Map map, uint id, uint playerId)
        {
            return new Champion(game, type, map, id, playerId);
        }
    }
}
