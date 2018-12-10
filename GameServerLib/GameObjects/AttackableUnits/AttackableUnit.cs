using System;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits
{
    public class AttackableUnit : GameObject, IAttackableUnit
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;

        public IStats Stats { get; protected set; }
        private float _statUpdateTimer;
        public bool IsModelUpdated { get; set; }
        public bool IsDead { get; protected set; }

        public string Model { get; protected set; }

        protected readonly ILog Logger;
        public IInventoryManager Inventory { get; protected set; }
        public int KillDeathCounter { get; set; }
        public int MinionCounter { get; protected set; }
        public IReplication Replication { get; protected set; }

        public AttackableUnit(
            IGame game,
            string model,
            IStats stats,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base((Game)game, x, y, collisionRadius, visionRadius, netId)

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

        public virtual void Die(IAttackableUnit killer)
        {
            SetToRemove();
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            var exp = _game.Map.MapProperties.GetExperienceFor(this);
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

            if (killer != null && killer is IChampion champion)
                champion.OnKill(this);
        }

        public virtual bool IsInDistress()
        {
            return false; //return DistressCause;
        }

        public bool ChangeModel(string model)
        {
            if (Model.Equals(model))
                return false;
            IsModelUpdated = true;
            Model = model;
            return true;
        }

        public virtual void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source,
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

            int attackerId = 0, targetId = 0;

            // todo: check if damage dealt by disconnected players cause anything bad 
            if (attacker is IChampion attackerChamp)
            {
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion(attackerChamp).UserId;
            }

            if (this is IChampion targetChamp)
            {
                targetId = (int)_game.PlayerManager.GetClientInfoByChampion(targetChamp).UserId;
            }

            _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText,
                _game.Config.IsDamageTextGlobal, attackerId, targetId);
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

        public virtual void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
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
        MINION = 6,
        SUPER_OR_CANNON_MINION = 7,
        CASTER_MINION = 8,
        MELEE_MINION = 9,
        TURRET = 10,
        CHAMPION = 11,
        INHIBITOR = 12,
        NEXUS = 13,
        DEFAULT = 14
    }
}
