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
    /// <summary>
    /// Base class for all attackable units.
    /// AttackableUnits normally follow these guidelines of functionality: Death state, basic Movement, Stats (including modifiers and basic replication), Buffs (and their scripts), and Call for Help.
    /// </summary>
    public class AttackableUnit : GameObject, IAttackableUnit
    {
        // Crucial Vars.
        private float _statUpdateTimer;

        // Utility Vars.
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;
        protected readonly ILog Logger;

        /// <summary>
        /// Whether or not this Unit is dead. Refer to TakeDamage() and Die().
        /// </summary>
        public bool IsDead { get; protected set; }
        /// <summary>
        /// Whether or not this Unit's model has been changeds this tick. Resets to False when the next tick update happens in ObjectManager.
        /// </summary>
        public bool IsModelUpdated { get; set; }
        /// <summary>
        /// The "score" of this Unit which increases as kills are gained and decreases as deaths are inflicted.
        /// Used in determining kill gold rewards.
        /// </summary>
        public int KillDeathCounter { get; set; }
        /// <summary>
        /// Number of minions this Unit has killed. Unused besides in replication which is used for packets, refer to NotifyUpdateStats in PacketNotifier.
        /// </summary>
        /// TODO: Verify if we want to move this to ObjAIBase since AttackableUnits cannot attack or kill anything.
        public int MinionCounter { get; protected set; }
        /// <summary>
        /// This Unit's current internally named model.
        /// </summary>
        public string Model { get; protected set; }
        /// <summary>
        /// Stats used purely in networking the accompishments or status of units and their gameplay affecting stats.
        /// </summary>
        public IReplication Replication { get; protected set; }
        /// <summary>
        /// Variable housing all of this Unit's stats such as health, mana, armor, magic resist, ActionState, etc.
        /// Currently these are only initialized manually by ObjAIBase and ObjBuilding.
        /// </summary>
        public IStats Stats { get; protected set; }

        public AttackableUnit(
            Game game,
            string model,
            IStats stats,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL
        ) : base(game, x, y, collisionRadius, visionRadius, netId, team)

        {
            Logger = LoggerProvider.GetLogger();
            Stats = stats;
            Model = model;
            Stats.AttackSpeedMultiplier.BaseValue = 1.0f;
        }

        /// <summary>
        /// Sets this unit's current model to the specified internally named model. *NOTE*: If the model is not present in the client files, all connected players will crash.
        /// </summary>
        /// <param name="model">Internally named model to set.</param>
        /// <returns></returns>
        /// TODO: Implement model verification (perhaps by making a list of all models in Content) so that clients don't crash if a model which doesn't exist in client files is given.
        public bool ChangeModel(string model)
        {
            if (Model.Equals(model))
            {
                return false;
            }
            IsModelUpdated = true;
            Model = model;
            return true;
        }

        /// <summary>
        /// Called when this unit dies.
        /// </summary>
        /// <param name="killer">Unit that killed this unit.</param>
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

        /// <summary>
        /// Returns whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to check for.</param>
        /// <returns>True/False.</returns>
        public bool GetIsTargetableToTeam(TeamId team)
        {
            if (!Stats.IsTargetable)
            {
                return false;
            }

            if (Team == team)
            {
                return !Stats.IsTargetableToTeam.HasFlag(SpellFlags.NonTargetableAlly);
            }

            return !Stats.IsTargetableToTeam.HasFlag(SpellFlags.NonTargetableEnemy);
        }

        /// <summary>
        /// Gets the movement speed stat of this unit.
        /// </summary>
        /// <returns>Float units/sec.</returns>
        public override float GetMoveSpeed()
        {
            return Stats.MoveSpeed.Total;
        }

        /// <summary>
        /// Whether or not this unit is currently calling for help. Unimplemented.
        /// </summary>
        /// <returns>True/False.</returns>
        /// TODO: Implement this.
        public virtual bool IsInDistress()
        {
            return false; //return DistressCause;
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

        /// <summary>
        /// Sets whether or not this unit should be targetable.
        /// </summary>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetable(bool targetable)
        {
            Stats.IsTargetable = targetable;
        }

        /// <summary>
        /// Sets whether or not this unit is targetable to the specified team.
        /// </summary>
        /// <param name="team">TeamId to change.</param>
        /// <param name="targetable">True/False.</param>
        public void SetIsTargetableToTeam(TeamId team, bool targetable)
        {
            Stats.IsTargetableToTeam &= ~SpellFlags.TargetableToAll;
            if (team == Team)
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= SpellFlags.NonTargetableAlly;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~SpellFlags.NonTargetableAlly;
                }
            }
            else
            {
                if (!targetable)
                {
                    Stats.IsTargetableToTeam |= SpellFlags.NonTargetableEnemy;
                }
                else
                {
                    Stats.IsTargetableToTeam &= ~SpellFlags.NonTargetableEnemy;
                }
            }
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="damageText">Type of damage the damage text should be.</param>
        public virtual void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source,
            DamageResultType damageText)
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
                case DamageSource.DAMAGE_SOURCE_DEFAULT:
                    break;
                case DamageSource.DAMAGE_SOURCE_SPELLPERSIST:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }

            if (damage < 0f)
            {
                damage = 0f;
            }
            else
            {
                //Damage dealing. (based on leagueoflegends' wikia)
                damage = defense >= 0 ? 100 / (100 + defense) * damage : (2 - 100 / (100 - defense)) * damage;
            }

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
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion(attackerChamp).PlayerId;
            }

            if (this is IChampion targetChamp)
            {
                targetId = (int)_game.PlayerManager.GetClientInfoByChampion(targetChamp).PlayerId;
            }
            // Show damage text for owner of pet
            if (attacker is IMinion attackerMinion && attackerMinion.IsPet && attackerMinion.Owner is IChampion)
            {
                attackerId = (int)_game.PlayerManager.GetClientInfoByChampion((IChampion)attackerMinion.Owner).PlayerId;
            }

            _game.PacketNotifier.NotifyUnitApplyDamage(attacker, this, damage, type, damageText,
                _game.Config.IsDamageTextGlobal, attackerId, targetId);
            
            // TODO: send this in one place only
            _game.PacketNotifier.NotifyUpdatedStats(this, false);

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min(attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * damage);
                // TODO: send this in one place only (preferably a central EventHandler class)
                _game.PacketNotifier.NotifyUpdatedStats(attacker, false);
            }
        }

        /// <summary>
        /// Applies damage to this unit.
        /// </summary>
        /// <param name="attacker">Unit that is dealing the damage.</param>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="type">Whether the damage is physical, magical, or true.</param>
        /// <param name="source">What the damage came from: attack, spell, summoner spell, or passive.</param>
        /// <param name="isCrit">Whether or not the damage text should be shown as a crit.</param>
        public virtual void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageResultType.RESULT_NORMAL;

            if (isCrit)
            {
                text = DamageResultType.RESULT_CRITICAL;
            }

            TakeDamage(attacker, damage, type, source, text);
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
    }
}
