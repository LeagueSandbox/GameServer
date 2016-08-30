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
    public enum TurretType
    {
        OuterTurret,
        InnerTurret,
        InhibitorTurret,
        NexusTurret,
        FountainTurret
    }

    public class LaneTurret : BaseTurret
    {
        private const float TURRET_RANGE = 905.0f;
        public TurretType Type { get; private set; }
        private bool _turretHPUpdated = false;

        public LaneTurret(Game game, uint id, string name, float x = 0, float y = 0, TeamId team = TeamId.TEAM_BLUE, TurretType type = TurretType.OuterTurret)
               : base(game, id, name, "", x, y)
        {
            this.Type = type;

            BuildTurret(type);

            setTeam(team);
        }

        public int GetEnemyChampionsCount()
        {
            var blueTeam = new List<Champion>();
            var purpTeam = new List<Champion>();
            foreach (var player in Game.GetMap().GetAllChampionsFromTeam(TeamId.TEAM_BLUE))
            {
                blueTeam.Add(player);
            }

            foreach (var player in Game.GetMap().GetAllChampionsFromTeam(TeamId.TEAM_PURPLE))
            {
                purpTeam.Add(player);
            }
            if (getTeam() == TeamId.TEAM_BLUE)
                return purpTeam.Count;
            else
                return blueTeam.Count;
        }

        public void BuildTurret(TurretType type)
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

        public override void update(long diff)
        {
            //Update stats if it's time
            switch (Type)
            {
                case TurretType.OuterTurret:
                    if (!_turretHPUpdated)
                    {
                        stats.CurrentHealth = 1300 + GetEnemyChampionsCount() * 250;
                        stats.HealthPoints.BaseValue = 1300 + GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GetMap().GetGameTime() > 40000 - (GetEnemyChampionsCount() * 2000) &&
                        _game.GetMap().GetGameTime() < 400000 - (GetEnemyChampionsCount() * 2000))
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
                        stats.CurrentHealth = 1300.0f + GetEnemyChampionsCount() * 250.0f;
                        stats.HealthPoints.BaseValue = 1300.0f + GetEnemyChampionsCount() * 250.0f;
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
                        stats.CurrentHealth = 1300 + GetEnemyChampionsCount() * 250;
                        stats.HealthPoints.BaseValue = 1300 + GetEnemyChampionsCount() * 250;
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
                        stats.CurrentHealth = 1300 + GetEnemyChampionsCount() * 125;
                        stats.HealthPoints.BaseValue = 1300 + GetEnemyChampionsCount() * 125;
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

        public override void refreshWaypoints()
        {
        }

        public override float getMoveSpeed()
        {
            return 0;
        }
    }
}
