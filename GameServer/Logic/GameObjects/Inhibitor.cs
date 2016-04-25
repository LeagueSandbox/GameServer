using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Inhibitor : Unit
    {
        System.Timers.Timer respawnTimer;

        // 0xA3 Announce 
        // 31 = inhibitor death
        // 32 - respawning soon
        // 33 - respawned
        public Inhibitor(Map map, uint id, string model, TeamId team, int collisionRadius = 40, float x = 0, float y = 0, int visionRadius = 0) : base(map, id, model, new MinionStats(), collisionRadius, x, y, visionRadius)
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
            respawnTimer = new System.Timers.Timer(10000);
            respawnTimer.AutoReset = false;
            respawnTimer.Elapsed += (a, b) =>
            {
                getStats().setCurrentHealth(getStats().getMaxHealth());
                setModel("ChaosInhibitor");
                deathFlag = false;
            };
            respawnTimer.Start();
            
            setModel("ChaosInhibitor_D");
            base.die(killer);
        }

        public override void refreshWaypoints()
        {

        }

        public override void setToRemove()
        {

        }
    }
}
