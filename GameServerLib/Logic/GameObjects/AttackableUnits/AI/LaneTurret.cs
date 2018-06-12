using LeagueSandbox.GameServer.Logic.Enet;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

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
        private float _statUpdateTimer;
        
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
                    {
                        continue;
                    }
                    Inventory.AddItem(itemTemplate);
                }
            }

            BuildTurret(type);
        }

        public int GetEnemyChampionsCount()
        {
            var blueTeam = new List<Champion>();
            var purpTeam = new List<Champion>();
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
            Stats = new Stats.Stats
            {
                Level = 1,
                IsMelee = false
            };

            switch (type)
            {
                case TurretType.InnerTurret:
                    globalGold = 100;

                    Stats.Level1Health = 1300;
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = 0;
                    Stats.Level1Armor = 60;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 170;
                    //Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
                    //Stats.IsInvulnerable = true;
                    Stats.AttackDamageGrowth = 4;
                    Stats.MagicResistGrowth = 1;
                    Stats.ArmorGrowth = 1;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.OuterTurret:
                    globalGold = 125;

                    Stats.Level1Health = 1300;
                    Stats.Level1AttackDamage = 100;
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;
                    Stats.Level1Armor = 60;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 152;
                    Stats.AttackDamageGrowth = 4;
                    Stats.MagicResistGrowth = 1;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.InhibitorTurret:
                    globalGold = 150;
                    globalExp = 500;

                    Stats.Level1Health = 1300;
                    Stats.Level1HealthRegen = 5;
                    Stats.PercentArmorPenetration.Add(-0.825f);
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;
                    Stats.Level1Armor = 67;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 190;
                    //Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
                    //Stats.IsInvulnerable = true;
                    Stats.AttackDamageGrowth = 4;
                    Stats.MagicResistGrowth = 1;
                    Stats.ArmorGrowth = 1;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.NexusTurret:
                    globalGold = 50;

                    Stats.Level1Health = 1300;
                    Stats.Level1HealthRegen = 5;
                    Stats.PercentArmorPenetration.Add(-0.825f);
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;
                    Stats.Level1Armor = 65;
                    Stats.Level1MagicResist = 100;
                    Stats.Level1AttackDamage = 180;
                    //Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
                    //Stats.IsInvulnerable = true;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case TurretType.FountainTurret:
                    Stats.AttackDelay = -0.610f;
                    Stats.Level1Health = 9999;
                    Stats.Level1AttackDamage = 999;
                    Stats.BaseAttackRange = 1250;
                    SetTargetableToTeam(TeamId.TEAM_BLUE, false);
                    SetTargetableToTeam(TeamId.TEAM_PURPLE, false);
                    Stats.IsInvulnerable = true;

                    AutoAttackDelay = 0.165f;
                    AutoAttackProjectileSpeed = 2000.0f;
                    break;
                default:
                    Stats.Level1Health = 2000;
                    Stats.Level1AttackDamage = 100;
                    Stats.BaseAttackRange = 905;
                    Stats.AttackDelay = -0.205f;

                    AutoAttackDelay = 4.95f / 30.0f;
                    AutoAttackProjectileSpeed = 1200.0f;

                    break;
            }
        }

        public override void update(float diff)
        {
            base.update(diff);
            _statUpdateTimer += diff;
            if (_statUpdateTimer < 500)
            {
                return;
            }

            _statUpdateTimer = 0;
            switch (Type)
            {
                case TurretType.OuterTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.Level1Health = 1300 + GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime <= 400000 - 2000 * GetEnemyChampionsCount())
                    {
                        var time = _game.GameTime - (40000 - 2000 * GetEnemyChampionsCount());
                        while ((int)time / 60000 + 1 > Stats.Level)
                        {
                            Stats.Level++;
                        }
                    }

                    if (_game.GameTime > 400000 - 2000 * GetEnemyChampionsCount())
                    {
                        Stats.Level = 8;
                    }

                    break;
                case TurretType.InnerTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.Level1Health = 1300 + GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime <= 1620000)
                    {
                        var time = _game.GameTime - 480000;
                        while ((int)time / 60000 + 1 > Stats.Level)
                        {
                            Stats.Level++;
                        }
                    }

                    if (_game.GameTime > 1620000)
                    {
                        Stats.Level = 21;
                    }

                    break;
                case TurretType.InhibitorTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.Level1Health = 1300 + GetEnemyChampionsCount() * 250;
                    }

                    if (_game.GameTime <= 2220000)
                    {
                        var time = _game.GameTime - 480000;
                        while ((int) time / 60000 + 1 > Stats.Level)
                        {
                            Stats.Level++;
                        }
                    }

                    if (_game.GameTime > 2220000)
                    {
                        Stats.Level = 31;
                    }

                    break;
                case TurretType.NexusTurret:
                    if (!_turretHPUpdated)
                    {
                        Stats.Level1Health = 1300 + GetEnemyChampionsCount() * 125;
                    }

                    break;
            }

            if (!_turretHPUpdated)
            {
                Stats.CurrentHealth = Stats.TotalHealth;
                _turretHPUpdated = true;
            }
        }

        public override void refreshWaypoints()
        {
        }

        public override void AutoAttackHit(AttackableUnit target)
        {
            if (Type == TurretType.FountainTurret)
            {
                target.TakeDamage(this, 1000, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
            }
            else
            {
                base.AutoAttackHit(target);
            }
        }
    }
}
