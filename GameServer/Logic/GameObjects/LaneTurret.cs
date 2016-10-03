using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
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
        public TurretType Type { get; private set; }
        private bool _turretHPUpdated;

        public LaneTurret(
            string name,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            TurretType type = TurretType.OuterTurret,
            int[] items = null,
            uint netId = 0
        ) : base(name, "", x, y, team, netId)
        {
            Type = type;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var itemTemplate = _itemManager.SafeGetItemType(item);
                    if (itemTemplate == null)
                        continue;
                    var i = Inventory.AddItem(itemTemplate);
                    GetStats().AddBuff(itemTemplate);
                }
            }

            BuildTurret(type);
        }

        public int GetEnemyChampionsCount()
        {
            var blueTeam = new List<Champion>();
            var purpTeam = new List<Champion>();
            foreach (var player in _game.Map.GetAllChampionsFromTeam(TeamId.TEAM_BLUE))
            {
                blueTeam.Add(player);
            }

            foreach (var player in _game.Map.GetAllChampionsFromTeam(TeamId.TEAM_PURPLE))
            {
                purpTeam.Add(player);
            }
            if (Team == TeamId.TEAM_BLUE)
                return purpTeam.Count;

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
                    stats.Range.BaseValue = 905.0f;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 60.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 170.0f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.OuterTurret:
                    globalGold = 125;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.AttackDamage.BaseValue = 100;
                    stats.Range.BaseValue = 905.0f;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 60.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 152.0f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.InhibitorTurret:
                    globalGold = 150;
                    globalExp = 500;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.HealthRegeneration.BaseValue = 5;
                    stats.ArmorPenetration.PercentBonus = 0.825f;
                    stats.Range.BaseValue = 905.0f;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 67.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 190.0f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.NexusTurret:
                    globalGold = 50;

                    stats.CurrentHealth = 1300;
                    stats.HealthPoints.BaseValue = 1300;
                    stats.HealthRegeneration.BaseValue = 5;
                    stats.ArmorPenetration.PercentBonus = 0.825f;
                    stats.Range.BaseValue = 905.0f;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.BaseValue = 65.0f;
                    stats.MagicResist.BaseValue = 100.0f;
                    stats.AttackDamage.BaseValue = 180.0f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;
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
                    AutoAttackDelay = 1.0f / 30.0f;
                    AutoAttackProjectileSpeed = 2000.0f;
                    break;
                default:

                    stats.CurrentHealth = 2000;
                    stats.HealthPoints.BaseValue = 2000;
                    stats.AttackDamage.BaseValue = 100;
                    stats.Range.BaseValue = 905.0f;
                    stats.AttackSpeedFlat = 0.83f;
                    stats.Armor.PercentBonus = 0.5f;
                    stats.MagicResist.PercentBonus = 0.5f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;

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

                    if (_game.Map.GameTime > 40000 - (GetEnemyChampionsCount() * 2000) &&
                        _game.Map.GameTime < 400000 - (GetEnemyChampionsCount() * 2000))
                    {
                        stats.MagicResist.BaseValue = 100.0f + ((_game.Map.GameTime - 30000) / 60000);
                        stats.AttackDamage.BaseValue = 152.0f + ((_game.Map.GameTime - 30000) / 60000) * 4;
                    }
                    else if (_game.Map.GameTime < 30000)
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
                    if (_game.Map.GameTime > 480000 && _game.Map.GameTime < 1620000)
                    {
                        stats.Armor.BaseValue = 60.0f + ((_game.Map.GameTime - 480000) / 60000);
                        stats.MagicResist.BaseValue = 100.0f + ((_game.Map.GameTime - 480000) / 60000);
                        stats.AttackDamage.BaseValue = 170.0f + ((_game.Map.GameTime - 480000) / 60000) * 4;
                    }
                    else if (_game.Map.GameTime < 480000)
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

                    if (_game.Map.GameTime > 480000 && _game.Map.GameTime < 2220000)
                    {
                        stats.Armor.BaseValue = 67.0f + ((_game.Map.GameTime - 480000) / 60000);
                        stats.MagicResist.BaseValue = 100.0f + ((_game.Map.GameTime - 480000) / 60000);
                        stats.AttackDamage.BaseValue = 190.0f + ((_game.Map.GameTime - 480000) / 60000) * 4;
                    }
                    else if (_game.Map.GameTime < 480000)
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

                    if (_game.Map.GameTime < 1)
                    {
                        stats.Armor.BaseValue = 65.0f;
                        stats.MagicResist.BaseValue = 100.0f;
                        stats.AttackDamage.BaseValue = 180.0f;
                    }
                    else if (_game.Map.GameTime > 480000 && _game.Map.GameTime < 2220000)
                    {
                        stats.Armor.BaseValue = 65.0f + ((_game.Map.GameTime - 480000) / 60000);
                        stats.MagicResist.BaseValue = 100.0f + ((_game.Map.GameTime - 480000) / 60000);
                        stats.AttackDamage.BaseValue = 180.0f + ((_game.Map.GameTime - 480000) / 60000) * 4;
                    }
                    else if (_game.Map.GameTime < 480000)
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

        public override void autoAttackHit(Unit target)
        {
            if (Type == TurretType.FountainTurret)
            {
                dealDamageTo(target, 1000, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            }
            else
            {
                base.autoAttackHit(target);
            }
        }
    }
}
