using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Inhibitor : Unit
    {
        public Inhibitor(Map map, uint id, string model, TeamId team, int collisionRadius = 40, float x = 0, float y = 0, int visionRadius = 0) : base(map, id, model, new Stats(), collisionRadius, x, y, visionRadius)
        {
            stats.setCurrentHealth(4000);
            stats.setMaxHealth(4000);

            setTeam(team);
        }
        
        public override float getMoveSpeed()
        {
            return 0;
        }

        public override void die(Unit killer)
        {
            //TEMP
            this.deathFlag = false;
            getStats().setCurrentHealth(getStats().getMaxHealth());
            Logger.LogCoreInfo("Inhibitor just died.");
            //TEMP
        }

        public override void refreshWaypoints()
        {
            
        }

        public override void setToRemove()
        {
           
        }
    }
}
