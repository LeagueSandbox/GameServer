using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Turret : Unit
    {
        private const float TURRET_RANGE = 905.0f;
        private string name;

        public Turret(Game game, uint id, string name, float x = 0, float y = 0, float hp = 0, float ad = 0, TeamId team = TeamId.TEAM_BLUE) : base(game, id, "", new TurretStats(), 50, x, y, 1200)
        {
            this.name = name;

            stats.setCurrentHealth(hp);
            stats.setMaxHealth(hp);
            stats.setBaseAd(ad);
            stats.setRange(TURRET_RANGE);

            autoAttackDelay = 4.95f / 30.0f;
            autoAttackProjectileSpeed = 1200.0f;

            setTeam(team);
        }

        public string getName()
        {
            return name;
        }
        public override void update(long diff)
        {
            if (!isAttacking)
            {
                var objects = _game.GetMap().GetObjects();
                Unit nextTarget = null;
                int nextTargetPriority = 10;

                foreach (var it in objects)
                {
                    var u = it.Value as Unit;

                    if (u == null || u.isDead() || u.getTeam() == getTeam() || distanceWith(u) > TURRET_RANGE)
                        continue;

                    // Note: this method means that if there are two champions within turret range,
                    // The player to have been added to the game first will always be targeted before the others
                    if (targetUnit == null)
                    {
                        var priority = classifyTarget(u);
                        if (priority < nextTargetPriority)
                        {
                            nextTarget = u;
                            nextTargetPriority = priority;
                        }
                    }
                    else
                    {
                        var targetIsChampion = targetUnit as Champion;

                        // Is the current target a champion? If it is, don't do anything
                        if (targetIsChampion != null)
                        {
                            // Find the next champion in range targeting an enemy champion who is also in range
                            var enemyChamp = u as Champion;
                            if (enemyChamp != null && enemyChamp.getTargetUnit() != null)
                            {
                                var enemyChampTarget = enemyChamp.getTargetUnit() as Champion;
                                if (enemyChampTarget != null &&                                                          // Enemy Champion is targeting an ally
                                    enemyChamp.distanceWith(enemyChampTarget) <= enemyChamp.getStats().getRange() &&     // Enemy within range of ally
                                    distanceWith(enemyChampTarget) <= TURRET_RANGE)
                                {                                     // Enemy within range of this turret
                                    nextTarget = enemyChamp; // No priority required
                                    break;
                                }
                            }
                        }
                    }
                }
                if (nextTarget != null)
                {
                    targetUnit = nextTarget;
                    _game.GetPacketNotifier().notifySetTarget(this, nextTarget);
                }
            }

            // Lose focus of the unit target if the target is out of range
            if (targetUnit != null && distanceWith(targetUnit) > TURRET_RANGE)
            {
                setTargetUnit(null);
                _game.GetPacketNotifier().notifySetTarget(this, null);
            }

            base.update(diff);
        }

        //todo
        public override void die(Unit killer)
        {
            base.die(killer);
        }

        public override void refreshWaypoints()
        {
        }

        public override float getMoveSpeed()
        {
            return 0;
        }
    }
}
