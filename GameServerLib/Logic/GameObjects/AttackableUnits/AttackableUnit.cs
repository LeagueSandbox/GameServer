using System;
using GameServerCore.Logic.Domain;
using GameServerCore.Logic.Domain.GameObjects;
using GameServerCore.Logic.Enums;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Logging;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits
{
    public class AttackableUnit : GameObject, IAttackableUnit
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

        protected readonly ILogger Logger;
        public InventoryManager Inventory { get; protected set; }
        public int KillDeathCounter { get; protected set; }
        public int MinionCounter { get; protected set; }
        public Replication Replication { get; protected set; }

        IReplication IAttackableUnit.Replication => Replication;
        IStats IAttackableUnit.Stats => Stats;
        IInventoryManager IAttackableUnit.Inventory => Inventory;

        public AttackableUnit(
            Game game,
            string model,
            Stats.Stats stats,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, x, y, collisionRadius, visionRadius, netId)

        {
            Logger = LoggerProvider.GetLogger();
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
            {
                // update Stats (hpregen, manaregen) every 0.5 seconds
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

            if ((killer != null) && (killer is Champion))
            {
                ((Champion)killer).OnKill(this);
            }
            IsDashing = false;
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
            damage = defense >= 0 ? 100 / (100 + defense) * damage : (2 - 100 / (100 - defense)) * damage;

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

    public enum ClassifyUnit
    {
        CHAMPION_ATTACKING_CHAMPION = 1,
        MINION_ATTACKING_CHAMPION = 2,
        MINION_ATTACKING_MINION = 3,
        TURRET_ATTACKING_MINION = 4,
        CHAMPION_ATTACKING_MINION = 5,
        PLACEABLE = 6,
        SUPER_OR_CANNON_MINION = 7,
        CASTER_MINION = 8,
        MELEE_MINION = 9,
        TURRET = 10,
        CHAMPION = 11,
        INHIBITOR = 12,
        NEXUS = 13,
        DEFAULT = 14
    }

    public enum DamageSource
    {
        DAMAGE_SOURCE_ATTACK,
        DAMAGE_SOURCE_SPELL,
        DAMAGE_SOURCE_SUMMONER_SPELL, // Ignite shouldn't destroy Banshee's
        DAMAGE_SOURCE_PASSIVE // Red/Thornmail shouldn't as well
    }
}
