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
        protected const int NUM_SIDES = 2;// Change implementation when more game-modes with more teams are available
        protected const float PERCENT_MAX_HEALTH_HEAL = 0.15f;
        protected const float PERCENT_MAX_MANA_HEAL = 0.15f;
        protected float fountainSize;
        protected long healTickTimer;
        protected List<Target> healLocations = new List<Target>();

        public Fountain()
        {
            fountainSize = 1000.0f;
            healTickTimer = 0; ;
        }
        public Fountain(float fountainSize)
        {
            this.fountainSize = fountainSize;
            healTickTimer = 0;
        }

        public void setHealLocations(Map map)
        {
            for (int i = 0; i < NUM_SIDES; i++)
                healLocations.Add(map.getRespawnLocation(i));
        }

        public void healChampions(Map map, long diff)
        {
            healTickTimer += diff;
            if (healTickTimer > 1000)
            {
                healTickTimer = 0;

                int team = 0;
                foreach (var f in healLocations)
                {
                    foreach (var c in map.getChampionsInRange(f, fountainSize))
                    {
                        if (c.getTeam() == CustomConvert.toTeamId(team))
                        {
                            var hp = c.getStats().getCurrentHealth();
                            var maxHP = c.getStats().getMaxHealth();
                            if (hp + maxHP * PERCENT_MAX_HEALTH_HEAL < maxHP)
                                c.getStats().setCurrentHealth(hp + maxHP * PERCENT_MAX_HEALTH_HEAL);
                            else if (hp < maxHP)
                                c.getStats().setCurrentHealth(maxHP);

                            var mp = c.getStats().getCurrentMana();
                            var maxMp = c.getStats().getMaxMana();
                            if (mp + maxMp * PERCENT_MAX_MANA_HEAL < maxMp)
                                c.getStats().setCurrentMana(mp + maxMp * PERCENT_MAX_MANA_HEAL);
                            else if (mp < maxMp)
                                c.getStats().setCurrentMana(maxMp);
                        }
                    }
                    team++;
                }
            }
        }
    }
}
