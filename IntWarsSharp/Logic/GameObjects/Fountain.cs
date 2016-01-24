using IntWarsSharp.Core.Logic;
using IntWarsSharp.Logic.Enet;
using IntWarsSharp.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.GameObjects
{
    public class Fountain
    {
        protected const int NUM_SIDES = 2;// Change implementation when more game-modes with more teams are available
        protected const float PERCENT_MAX_HEALTH_HEAL = 0.15f;
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
            if (healTickTimer > 1000000)
            {
                healTickTimer = 0;

                int team = 0;
                foreach (var f in healLocations)
                {
                    foreach (var c in map.getChampionsInRange(f, fountainSize))
                    {
                        if (c.getTeam() == (TeamId)team)
                        {
                            float HP = c.getStats().getCurrentHealth(), MaxHP = c.getStats().getMaxHealth();
                            if (HP + MaxHP * PERCENT_MAX_HEALTH_HEAL < MaxHP)
                            {
                                c.getStats().setCurrentHealth(HP + MaxHP * PERCENT_MAX_HEALTH_HEAL);
                            }
                            else if (HP < MaxHP)
                            {
                                c.getStats().setCurrentHealth(MaxHP);
                                Logger.LogCoreInfo("Fully healed at fountain");
                            }
                        }
                    }
                    team++;
                }
            }
        }
    }
}
