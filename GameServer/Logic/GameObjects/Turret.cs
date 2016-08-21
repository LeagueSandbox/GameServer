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
        private Game game;
        private TurretType type;
        private float globalGold = 250.0f;
        private float globalExp = 0.0f;
		private bool _turretHPUpdated = false;

        public Turret(Game game, uint id, string name, float x = 0, float y = 0, TeamId team = TeamId.TEAM_BLUE, TurretType type = TurretType.OuterTurret) : base(game, id, "", new Stats(), 50, x, y, 1200)
        {
            this.name = name;
            this.game = game;
            this.type = type;

            buildTurret(type);

            setTeam(team);
        }

        public int getEnemyChampionsCount()
        {
            var blueTeam = new List<Champion>();
            var purpTeam = new List<Champion>();
            foreach (var player in game.GetMap().GetAllChampionsFromTeam(TeamId.TEAM_BLUE))
            {
                blueTeam.Add(player);
            }

            foreach (var player in game.GetMap().GetAllChampionsFromTeam(TeamId.TEAM_PURPLE))
            {
                purpTeam.Add(player);
            }
            if (getTeam() == TeamId.TEAM_BLUE)
                return purpTeam.Count;
            else
                return blueTeam.Count;
        }

        public TurretType getTurretType()
        {
            return type;
        }

        public void buildTurret(TurretType type)
        {
            switch (type)
            {
                case TurretType.InnerTurret:
                    globalGold = 100;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 60.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 170.0f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.OuterTurret:
                    globalGold = 125;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.AttackDamage.BaseValue = 100;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 60.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 152.0f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.InhibitorTurret:
                    globalGold = 150;
                    globalExp = 500;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.HealthRegeneration.BaseValue = 5;
                    stats.ArmorPenetration.PercentBonus = 0.825f;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 67.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 190.0f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.NexusTurret:
                    globalGold = 50;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.HealthRegeneration.BaseValue = 5;
                    stats.ArmorPenetration.PercentBonus = 0.825f;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 65.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 180.0f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.FountainTurret:
                    stats.AttackSpeedFlat = 1.6f;
                    stats.GrowthAttackSpeed = 2.125f;
                    stats.CurrentHealth = 9999;
                    stats.HealthPoints.BaseValue = 9999;
                    globalExp = 400.0f;
                    stats.AttackDamage.BaseValue = 999.0f;
                    globalGold = 100.0f;
                    stats.Range.BaseValue = 1250.0f;
                    autoAttackProjectileSpeed = 1250.0f;
                    autoAttackDelay = (1.0f / 30.0f); //Pretty sure this aint the true value
                    break;
                default:

                    stats.CurrentHealth = 2000;
                    stats.HealthPoints.BaseValue = 2000;
                    stats.AttackDamage.BaseValue = 100;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.PercentBonus = 0.5f;
                    stats.MagicResist.PercentBonus = 0.5f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;

                    break;
            }
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
                                    enemyChamp.distanceWith(enemyChampTarget) <= enemyChamp.GetStats().Range.Total &&     // Enemy within range of ally
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
                    _game.PacketNotifier.notifySetTarget(this, nextTarget);
                }
            }

            // Lose focus of the unit target if the target is out of range
            if (targetUnit != null && distanceWith(targetUnit) > TURRET_RANGE)
            {
                setTargetUnit(null);
                _game.PacketNotifier.notifySetTarget(this, null);
            }

			//Update stats if it's time
            switch (type)
            {
                case TurretType.OuterTurret:
                    if (!_turretHPUpdated)
                    {
                        stats.CurrentHealth = 1300 + getEnemyChampionsCount() * 250;
                        stats.HealthPoints.BaseValue = 1300 + getEnemyChampionsCount() * 250;
                    }

                    if (_game.GetMap().GetGameTime() > 40000 - (getEnemyChampionsCount() * 2000) &&
                        _game.GetMap().GetGameTime() < 400000 - (getEnemyChampionsCount() * 2000))
                    {
                        stats.MagicResist.BaseValue = 100.0f + ((_game.GetMap().GetGameTime() - 30000) / 60000);
                        stats.AttackDamage.BaseValue = 152.0f + ((_game.GetMap().GetGameTime() - 30000) / 60000) * 4;
                    }
                    else if (_game.GetMap().GetGameTime() < 30000)
                    {
                        stats.MagicResist.BaseValue = 100.0f;
                        stats.AttackDamage.BaseValue = 152.0f;
                    }
                    else
                    {
                        stats.MagicResist.BaseValue = 107.0f;
                        stats.AttackDamage.BaseValue = 180.0f;
                    }
                    break;
                case TurretType.InnerTurret:
                    if (!_turretHPUpdated)
                    {
                        stats.CurrentHealth = 1300.0f + getEnemyChampionsCount() * 250.0f;
                        stats.HealthPoints.BaseValue = 1300.0f + getEnemyChampionsCount() * 250.0f;
                    }
                    if (_game.GetMap().GetGameTime() > 480000 && _game.GetMap().GetGameTime() < 1620000)
                    {
                        stats.Armor.BaseValue = 60.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000);
                        stats.MagicResist.BaseValue = 100.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000);
                        stats.AttackDamage.BaseValue = 170.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000) * 4;
                    }
                    else if (_game.GetMap().GetGameTime() < 480000)
                    {
                        stats.Armor.BaseValue = 60.0f;
                        stats.MagicResist.BaseValue = 100.0f;
                        stats.AttackDamage.BaseValue = 170.0f;
                    }
                    else
                    {
                        stats.Armor.BaseValue = 80.0f;
                        stats.MagicResist.BaseValue = 120.0f;
                        stats.AttackDamage.BaseValue = 250.0f;
                    }
                    break;
                case TurretType.InhibitorTurret:
                    if (!_turretHPUpdated)
                    {
                        stats.CurrentHealth = 1300 + getEnemyChampionsCount() * 250;
                        stats.HealthPoints.BaseValue = 1300 + getEnemyChampionsCount() * 250;
                    }

                    if (_game.GetMap().GetGameTime() > 480000 && _game.GetMap().GetGameTime() < 2220000)
                    {
                        stats.Armor.BaseValue = 67.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000);
                        stats.MagicResist.BaseValue = 100.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000);
                        stats.AttackDamage.BaseValue = 190.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000) * 4;
                    }
                    else if (_game.GetMap().GetGameTime() < 480000)
                    {
                        stats.Armor.BaseValue = 67.0f;
                        stats.MagicResist.BaseValue = 100.0f;
                        stats.AttackDamage.BaseValue = 190.0f;
                    }
                    else
                    {
                        stats.Armor.BaseValue = 97.0f;
                        stats.MagicResist.BaseValue = 130.0f;
                        stats.AttackDamage.BaseValue = 250.0f;
                    }
                    break;
                case TurretType.NexusTurret:
                    if (!_turretHPUpdated)
                    {
                        stats.CurrentHealth = 1300 + getEnemyChampionsCount() * 125;
                        stats.HealthPoints.BaseValue = 1300 + getEnemyChampionsCount() * 125;
                    }

                    if (_game.GetMap().GetGameTime() < 1)
                    {
                        stats.Armor.BaseValue = 65.0f;
                        stats.MagicResist.BaseValue = 100.0f;
                        stats.AttackDamage.BaseValue = 180.0f;
                    }
                    else if (_game.GetMap().GetGameTime() > 480000 && _game.GetMap().GetGameTime() < 2220000)
                    {
                        stats.Armor.BaseValue = 65.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000);
                        stats.MagicResist.BaseValue = 100.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000);
                        stats.AttackDamage.BaseValue = 180.0f + ((_game.GetMap().GetGameTime() - 480000) / 60000) * 4;
                    }
                    else if (_game.GetMap().GetGameTime() < 480000)
                    {
                        stats.Armor.BaseValue = 65.0f;
                        stats.MagicResist.BaseValue = 100.0f;
                        stats.AttackDamage.BaseValue = 180.0f;
                    }
                    else
                    {
                        stats.Armor.BaseValue = 95.0f;
                        stats.MagicResist.BaseValue = 130.0f;
                        stats.AttackDamage.BaseValue = 300.0f;
                    }
                    break;
            }
            _turretHPUpdated = true;
            base.update(diff);
        }

        //todo
        public override void die(Unit killer)
        {
            foreach (var player in game.GetMap().GetAllChampionsFromTeam(killer.getTeam()))
            {
                var goldEarn = globalGold;

                // Champions in Range within TURRET_RANGE * 1.5f will gain 150% more (obviously)
                if (player.distanceWith(this) <= (TURRET_RANGE * 1.5f) && !player.isDead())
                {
                    goldEarn = globalGold * 2.5f;
                    if(globalExp > 0)
                        player.GetStats().Experience += globalExp;
                }
                

                player.GetStats().Gold += goldEarn;
                _game.PacketNotifier.notifyAddGold(player, this, goldEarn);
            }
            _game.PacketNotifier.notifyUnitAnnounceEvent(UnitAnnounces.TurretDestroyed, this, killer);
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

    public enum TurretType
    {
        OuterTurret,
        InnerTurret,
        InhibitorTurret,
        NexusTurret,
        FountainTurret
    }
}
