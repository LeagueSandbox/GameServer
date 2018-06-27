using System;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits
{
    public class AttackableUnit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;

        public Stats.Stats Stats { get; protected set; }
        private float _statUpdateTimer;
        public bool IsModelUpdated { get; set; }
        public bool IsDead { get; protected set; }
        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                IsModelUpdated = true;
            }
        }
        protected Logger _logger = Program.ResolveDependency<Logger>();
        public InventoryManager Inventory { get; protected set; }
        public int KillDeathCounter { get; protected set; }
        public int MinionCounter { get; protected set; }
        public Replication Replication { get; protected set; }

        public AttackableUnit(
            string model,
            Stats.Stats stats,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, visionRadius, netId)

        {
            Stats = stats;
            Model = model;
            CollisionRadius = 40;
            Stats.AttackSpeedMultiplier.BaseValue = 1.0f;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddVisionUnit(this);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.ObjectManager.RemoveVisionUnit(this);
        }

        public override void Update(float diff)
        {
            base.Update(diff);

            _statUpdateTimer += diff;
            while (_statUpdateTimer >= 500)
            { // update Stats (hpregen, manaregen) every 0.5 seconds
                Stats.Update(_statUpdateTimer);
                _statUpdateTimer -= 500;
            }
        }

        public override float GetMoveSpeed()
        {
            return Stats.MoveSpeed.Total;
        }

        public virtual void Die(AttackableUnit killer)
        {
            SetToRemove();
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            var exp = _game.Map.MapGameScript.GetExperienceFor(this);
            var champs = _game.ObjectManager.GetChampionsInRange(this, EXP_RANGE, true);
            //Cull allied champions
            champs.RemoveAll(l => l.Team == Team);

            if (champs.Count > 0)
            {
                var expPerChamp = exp / champs.Count;
                foreach (var c in champs)
                {
                    c.Stats.Experience += expPerChamp;
                    _game.PacketNotifier.NotifyAddXp(c, expPerChamp);
                }
            }

            if (killer != null)
            {
                var cKiller = killer as Champion;

                if (cKiller == null)
                    return;

                var gold = _game.Map.MapGameScript.GetGoldFor(this);
                if (gold <= 0)
                {
                    return;
                }

                cKiller.Stats.Gold += gold;
                _game.PacketNotifier.NotifyAddGold(cKiller, this, gold);

                if (cKiller.KillDeathCounter < 0)
                {
                    cKiller.ChampionGoldFromMinions += gold;
                    _logger.LogCoreInfo($"Adding gold form minions to reduce death spree: {cKiller.ChampionGoldFromMinions}");
                }

                if (cKiller.ChampionGoldFromMinions >= 50 && cKiller.KillDeathCounter < 0)
                {
                    cKiller.ChampionGoldFromMinions = 0;
                    cKiller.KillDeathCounter += 1;
                }
            }

            if (IsDashing)
            {
                IsDashing = false;
            }
        }

        public virtual bool IsInDistress()
        {
            return false; //return DistressCause;
        }

        public virtual void TakeDamage(AttackableUnit attacker, float damage, DamageType type, DamageSource source,
            DamageText damageText)
        {
            float defense = 0;
            float regain = 0;
            var attackerStats = attacker.Stats;

            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = Stats.Armor.Total;
                    defense = (1 - attackerStats.ArmorPenetration.PercentBonus) * defense -
                              attackerStats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = Stats.MagicPenetration.Total;
                    defense = (1 - attackerStats.MagicPenetration.PercentBonus) * defense -
                              attackerStats.MagicPenetration.FlatBonus;
                    break;
                case DamageType.DAMAGE_TYPE_TRUE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            switch (source)
            {
                case DamageSource.DAMAGE_SOURCE_SPELL:
                    regain = attackerStats.SpellVamp.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_ATTACK:
                    regain = attackerStats.LifeSteal.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_SUMMONER_SPELL:
                    break;
                case DamageSource.DAMAGE_SOURCE_PASSIVE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }

            //Damage dealing. (based on leagueoflegends' wikia)
            damage = defense >= 0 ? (100 / (100 + defense)) * damage : (2 - (100 / (100 - defense))) * damage;

            ApiEventManager.OnUnitDamageTaken.Publish(this);

            Stats.CurrentHealth = Math.Max(0.0f, Stats.CurrentHealth - damage);
            if (!IsDead && Stats.CurrentHealth <= 0)
            {
                IsDead = true;
                Die(attacker);
            }

            _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
            // TODO: send this in one place only
            _game.PacketNotifier.NotifyUpdatedStats(this, false);

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min(attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * damage);
                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(attacker, false);
            }
        }

        public virtual void TakeDamage(AttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }

            TakeDamage(attacker, damage, type, source, text);
        }

        public bool GetIsTargetableToTeam(TeamId team)
        {
            if (!Stats.IsTargetable)
            {
                return false;
            }

            if (Team == team)
            {
                return !Stats.IsTargetableToTeam.HasFlag(IsTargetableToTeamFlags.NON_TARGETABLE_ALLY);
            }

            return !Stats.IsTargetableToTeam.HasFlag(IsTargetableToTeamFlags.NON_TARGETABLE_ENEMY);
        }

        public void SetIsTargetableToTeam(TeamId team, bool targetable)
        {
            if (team == Team)
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= IsTargetableToTeamFlags.NON_TARGETABLE_ALLY;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~IsTargetableToTeamFlags.NON_TARGETABLE_ALLY;
                }
            }
            else
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= IsTargetableToTeamFlags.NON_TARGETABLE_ENEMY;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~IsTargetableToTeamFlags.NON_TARGETABLE_ENEMY;
                }
            }
        }
    }

    public enum UnitAnnounces : byte
    {
        DEATH = 0x04,
        INHIBITOR_DESTROYED = 0x1F,
        INHIBITOR_ABOUT_TO_SPAWN = 0x20,
        INHIBITOR_SPAWNED = 0x21,
        TURRET_DESTROYED = 0x24,
        SUMMONER_DISCONNECTED = 0x47,
        SUMMONER_RECONNECTED = 0x48
    }

    public enum ClassifyUnit
    {
        CHAMPION_ATTACKING_CHAMPION = 1,
        MINION_ATTACKING_CHAMPION = 2,
        MINION_ATTACKING_MINION = 3,
        TURRET_ATTACKING_MINION = 4,
        CHAMPION_ATTACKING_MINION = 5,
        PLACEABLE = 6,
        MELEE_MINION = 7,
        CASTER_MINION = 8,
        SUPER_OR_CANNON_MINION = 9,
        TURRET = 10,
        CHAMPION = 11,
        INHIBITOR = 12,
        NEXUS = 13,
        DEFAULT = 14
    }

    public enum DamageType : byte
    {
        DAMAGE_TYPE_PHYSICAL = 0x0,
        DAMAGE_TYPE_MAGICAL = 0x1,
        DAMAGE_TYPE_TRUE = 0x2
    }

    public enum DamageText : byte
    {
        DAMAGE_TEXT_INVULNERABLE = 0x0,
        DAMAGE_TEXT_DODGE = 0x2,
        DAMAGE_TEXT_CRITICAL = 0x3,
        DAMAGE_TEXT_NORMAL = 0x4,
        DAMAGE_TEXT_MISS = 0x5
    }

    public enum DamageSource
    {
        DAMAGE_SOURCE_ATTACK,
        DAMAGE_SOURCE_SPELL,
        DAMAGE_SOURCE_SUMMONER_SPELL, // Ignite shouldn't destroy Banshee's
        DAMAGE_SOURCE_PASSIVE // Red/Thornmail shouldn't as well
    }

    public enum AttackType : byte
    {
        ATTACK_TYPE_RADIAL,
        ATTACK_TYPE_MELEE,
        ATTACK_TYPE_TARGETED
    }

    public enum MoveOrder
    {
        MOVE_ORDER_MOVE,
        MOVE_ORDER_ATTACKMOVE
    }

    public enum ShieldType : byte
    {
        GREEN_SHIELD = 0x01,
        MAGIC_SHIELD = 0x02,
        NORMAL_SHIELD = 0x03
    }
}
