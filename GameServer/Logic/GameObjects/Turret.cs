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

        public Turret(Game game, uint id, string name, float x = 0, float y = 0, TeamId team = TeamId.TEAM_BLUE, TurretType type = TurretType.OuterTurret) : base(game, id, "", new Stats(), 50, x, y, 1200)
        {
            this.name = name;
            this.game = game;
            this.type = type;

            buildTurret(type);

            setTeam(team);
        }

        public void buildTurret(TurretType type)
        {
            switch (type)
            {
                case TurretType.InnerTurret:
                    globalGold = 100;

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
                case TurretType.OuterTurret:
                    globalGold = 125;

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
                case TurretType.InhibitorTurret:
                    globalGold = 150;
                    globalExp = 500;

                    stats.CurrentHealth = 2500;
                    stats.HealthPoints.BaseValue = 2500;
                    stats.HealthRegeneration.BaseValue = 5;
                    stats.ArmorPenetration.PercentBonus = 0.825f;
                    stats.AttackDamage.BaseValue = 100;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.PercentBonus = 0.5f;
                    stats.MagicResist.PercentBonus = 0.5f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.NexusTurret:
                    globalGold = 50;

                    stats.CurrentHealth = 2500;
                    stats.HealthPoints.BaseValue = 2500;
                    stats.HealthRegeneration.BaseValue = 5;
                    stats.ArmorPenetration.PercentBonus = 0.825f;
                    stats.AttackDamage.BaseValue = 100;
                    stats.Range.BaseValue = TURRET_RANGE;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.PercentBonus = 0.5f;
                    stats.MagicResist.PercentBonus = 0.5f;

                    autoAttackDelay = 4.95f / 30.0f;
                    autoAttackProjectileSpeed = 1200.0f;
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
