﻿using System;
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
    public class Nexus : Unit
    {
        public Nexus(Map map, uint id, string model, TeamId team, int collisionRadius = 40, float x = 0, float y = 0, int visionRadius = 0) : base(map, id, model, new BuildingStats(), collisionRadius, x, y, visionRadius)
        {
            stats.CurrentHealth = 5500;
            stats.HealthPoints.BaseValue = 5500;

            setTeam(team);
        }

        public override void die(Unit killer)
        {
            map.GetGame().stopGame();
            PacketNotifier.NotifyGameEnd(this);
        }

        public override void refreshWaypoints()
        {

        }

        public override void setToRemove()
        {

        }

        public override float getMoveSpeed()
        {
            return 0;
        }
    }
}
