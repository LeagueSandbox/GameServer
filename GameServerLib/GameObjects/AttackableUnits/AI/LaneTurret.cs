using System.Collections.Generic;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class LaneTurret : BaseTurret, ILaneTurret
    {
        public TurretType Type { get; }
        private bool _turretHpUpdated;

        public LaneTurret(
            Game game,
            string name,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            TurretType type = TurretType.OUTER_TURRET,
            int[] items = null,
            uint netId = 0
        ) : base(game, name, "", x, y, team, netId)
        {
            Type = type;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var itemTemplate = _itemManager.SafeGetItemType(item);
                    if (itemTemplate == null)
                    {
                        continue;
                    }
                    Inventory.AddItem(itemTemplate);
                    Stats.AddModifier(itemTemplate);
                }
            }

            BuildTurret(type);
        }

        public int GetEnemyChampionsCount()
        {
            var blueTeam = new List<IChampion>();
            var purpTeam = new List<IChampion>();
            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(TeamId.TEAM_BLUE))
            {
                blueTeam.Add(player);
            }

            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(TeamId.TEAM_PURPLE))
            {
                purpTeam.Add(player);
            }

            if (Team == TeamId.TEAM_BLUE)
            {
                return purpTeam.Count;
            }

            return blueTeam.Count;
        }

        public void BuildTurret(TurretType type)
        {
            IsMelee = false;
            switch (type)
            {
                case TurretType.INNER_TURRET:
                    _globalGold = 100;

                    Stats.CurrentHealth = 1300;
                    Stats.HealthPoints.BaseValue = 1300;
                    Stats.Range.BaseValue = 905.0f;
                    Stats.AttackSpeedFlat = 0.83f;
                    Stats.Armor.BaseValue = 60.0f;
                    Stats.MagicResist.BaseValue = 100.0f;
                    Stats.AttackDamage.BaseValue = 170.0f;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.OUTER_TURRET:
                    _globalGold = 125;

                    Stats.CurrentHealth = 1300;
                    Stats.HealthPoints.BaseValue = 1300;
                    Stats.AttackDamage.BaseValue = 100;
                    Stats.Range.BaseValue = 905.0f;
                    Stats.AttackSpeedFlat = 0.83f;
                    Stats.Armor.BaseValue = 60.0f;
                    Stats.MagicResist.BaseValue = 100.0f;
                    Stats.AttackDamage.BaseValue = 152.0f;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.INHIBITOR_TURRET:
                    _globalGold = 150;
                    _globalExp = 500;

                    Stats.CurrentHealth = 1300;
                    Stats.HealthPoints.BaseValue = 1300;
                    Stats.HealthRegeneration.BaseValue = 5;
                    Stats.ArmorPenetration.PercentBonus = 0.825f;
                    Stats.Range.BaseValue = 905.0f;
                    Stats.AttackSpeedFlat = 0.83f;
                    Stats.Armor.BaseValue = 67.0f;
                    Stats.MagicResist.BaseValue = 100.0f;
                    Stats.AttackDamage.BaseValue = 190.0f;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.NEXUS_TURRET:
                    _globalGold = 50;

                    Stats.CurrentHealth = 1300;
                    Stats.HealthPoints.BaseValue = 1300;
                    Stats.HealthRegeneration.BaseValue = 5;
                    Stats.ArmorPenetration.PercentBonus = 0.825f;
                    Stats.Range.BaseValue = 905.0f;
                    Stats.AttackSpeedFlat = 0.83f;
                    Stats.Armor.BaseValue = 65.0f;
                    Stats.MagicResist.BaseValue = 100.0f;
                    Stats.AttackDamage.BaseValue = 180.0f;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.FOUNTAIN_TURRET:
                    IsMelee = false;
                    Stats.AttackSpeedFlat = 1.6f;
                    Stats.GrowthAttackSpeed = 2.125f;
                    Stats.CurrentHealth = 9999;
                    Stats.HealthPoints.BaseValue = 9999;
                    _globalExp = 400.0f;
                    Stats.AttackDamage.BaseValue = 999.0f;
                    _globalGold = 100.0f;
                    Stats.Range.BaseValue = 1250.0f;
                    AutoAttackDelay = 1f / 30;
                    AutoAttackProjectileSpeed = 2000.0f;
                    break;
                default:

                    Stats.CurrentHealth = 2000;
                    Stats.HealthPoints.BaseValue = 2000;
                    Stats.AttackDamage.BaseValue = 100;
                    Stats.Range.BaseValue = 905.0f;
                    Stats.AttackSpeedFlat = 0.83f;
                    Stats.Armor.PercentBonus = 0.5f;
                    Stats.MagicResist.PercentBonus = 0.5f;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;

                    break;
            }
        }

        public override void Update(float diff)
        {
            //Update Stats if it's time
            switch (Type)
            {
                case TurretType.OUTER_TURRET:
                    if (!_turretHpUpdated)
                    {
                        Stats.CurrentHealth = 1300 + GetEnemyChampionsCount() * 250;
                        Stats.HealthPoints.BaseValue = 1300 + GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime > 40000 - GetEnemyChampionsCount() * 2000 &&
                        _game.GameTime < 400000 - GetEnemyChampionsCount() * 2000)
                    {
                        Stats.MagicResist.BaseValue = 100.0f + (_game.GameTime - 30000) / 60000;
                        Stats.AttackDamage.BaseValue = 152.0f + (_game.GameTime - 30000) / 60000 * 4;
                    }
                    else if (_game.GameTime < 30000)
                    {
                        Stats.MagicResist.BaseValue = 100.0f;
                        Stats.AttackDamage.BaseValue = 152.0f;
                    }
                    else
                    {
                        Stats.MagicResist.BaseValue = 107.0f;
                        Stats.AttackDamage.BaseValue = 180.0f;
                    }
                    break;
                case TurretType.INNER_TURRET:
                    if (!_turretHpUpdated)
                    {
                        Stats.CurrentHealth = 1300.0f + GetEnemyChampionsCount() * 250.0f;
                        Stats.HealthPoints.BaseValue = 1300.0f + GetEnemyChampionsCount() * 250.0f;
                    }
                    if (_game.GameTime > 480000 && _game.GameTime < 1620000)
                    {
                        Stats.Armor.BaseValue = 60.0f + (_game.GameTime - 480000) / 60000;
                        Stats.MagicResist.BaseValue = 100.0f + (_game.GameTime - 480000) / 60000;
                        Stats.AttackDamage.BaseValue = 170.0f + (_game.GameTime - 480000) / 60000 * 4;
                    }
                    else if (_game.GameTime < 480000)
                    {
                        Stats.Armor.BaseValue = 60.0f;
                        Stats.MagicResist.BaseValue = 100.0f;
                        Stats.AttackDamage.BaseValue = 170.0f;
                    }
                    else
                    {
                        Stats.Armor.BaseValue = 80.0f;
                        Stats.MagicResist.BaseValue = 120.0f;
                        Stats.AttackDamage.BaseValue = 250.0f;
                    }
                    break;
                case TurretType.INHIBITOR_TURRET:
                    if (!_turretHpUpdated)
                    {
                        Stats.CurrentHealth = 1300 + GetEnemyChampionsCount() * 250;
                        Stats.HealthPoints.BaseValue = 1300 + GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime > 480000 && _game.GameTime < 2220000)
                    {
                        Stats.Armor.BaseValue = 67.0f + (_game.GameTime - 480000) / 60000;
                        Stats.MagicResist.BaseValue = 100.0f + (_game.GameTime - 480000) / 60000;
                        Stats.AttackDamage.BaseValue = 190.0f + (_game.GameTime - 480000) / 60000 * 4;
                    }
                    else if (_game.GameTime < 480000)
                    {
                        Stats.Armor.BaseValue = 67.0f;
                        Stats.MagicResist.BaseValue = 100.0f;
                        Stats.AttackDamage.BaseValue = 190.0f;
                    }
                    else
                    {
                        Stats.Armor.BaseValue = 97.0f;
                        Stats.MagicResist.BaseValue = 130.0f;
                        Stats.AttackDamage.BaseValue = 250.0f;
                    }
                    break;
                case TurretType.NEXUS_TURRET:
                    if (!_turretHpUpdated)
                    {
                        Stats.CurrentHealth = 1300 + GetEnemyChampionsCount() * 125;
                        Stats.HealthPoints.BaseValue = 1300 + GetEnemyChampionsCount() * 125;
                    }

                    if (_game.GameTime < 1)
                    {
                        Stats.Armor.BaseValue = 65.0f;
                        Stats.MagicResist.BaseValue = 100.0f;
                        Stats.AttackDamage.BaseValue = 180.0f;
                    }
                    else if (_game.GameTime > 480000 && _game.GameTime < 2220000)
                    {
                        Stats.Armor.BaseValue = 65.0f + (_game.GameTime - 480000) / 60000;
                        Stats.MagicResist.BaseValue = 100.0f + (_game.GameTime - 480000) / 60000;
                        Stats.AttackDamage.BaseValue = 180.0f + (_game.GameTime - 480000) / 60000 * 4;
                    }
                    else if (_game.GameTime < 480000)
                    {
                        Stats.Armor.BaseValue = 65.0f;
                        Stats.MagicResist.BaseValue = 100.0f;
                        Stats.AttackDamage.BaseValue = 180.0f;
                    }
                    else
                    {
                        Stats.Armor.BaseValue = 95.0f;
                        Stats.MagicResist.BaseValue = 130.0f;
                        Stats.AttackDamage.BaseValue = 300.0f;
                    }
                    break;
            }

            _turretHpUpdated = true;
            base.Update(diff);
        }

        public override void RefreshWaypoints()
        {
        }

        public override float GetMoveSpeed()
        {
            return 0;
        }

        public override void AutoAttackHit(IAttackableUnit target)
        {
            if (Type == TurretType.FOUNTAIN_TURRET)
            {
                target.TakeDamage(this, 1000, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            }
            else
            {
                base.AutoAttackHit(target);
            }
        }
    }
}
