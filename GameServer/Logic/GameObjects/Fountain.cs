using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Fountain
    {
        private const float PERCENT_MAX_HEALTH_HEAL = 0.15f;
        private const float PERCENT_MAX_MANA_HEAL = 0.15f;
        private const float HEAL_FREQUENCY = 1000f;
        private float X;
        private float Y;
        private float FountainSize;
        private long HealTickTimer;
        private TeamId Team;

        public Fountain(TeamId team, float x, float y, float size)
        {
            X = x;
            Y = y;
            FountainSize = size;
            HealTickTimer = 0;
            Team = team;
        }

        internal void Update(Map map, long diff)
        {
            HealTickTimer += diff;
            if (HealTickTimer < HEAL_FREQUENCY)
                return;

            HealTickTimer = 0;

            var champions = map.getChampionsInRange(X, Y, FountainSize, true);
            foreach (var champion in champions)
            {
                if (champion.getTeam() != Team)
                    continue;

                var hp = champion.getStats().getCurrentHealth();
                var maxHP = champion.getStats().getMaxHealth();
                if (hp + maxHP * PERCENT_MAX_HEALTH_HEAL < maxHP)
                    champion.getStats().setCurrentHealth(hp + maxHP * PERCENT_MAX_HEALTH_HEAL);
                else if (hp < maxHP)
                    champion.getStats().setCurrentHealth(maxHP);

                var mp = champion.getStats().getCurrentMana();
                var maxMp = champion.getStats().getMaxMana();
                if (mp + maxMp * PERCENT_MAX_MANA_HEAL < maxMp)
                    champion.getStats().setCurrentMana(mp + maxMp * PERCENT_MAX_MANA_HEAL);
                else if (mp < maxMp)
                    champion.getStats().setCurrentMana(maxMp);
            }
        }
    }
}
