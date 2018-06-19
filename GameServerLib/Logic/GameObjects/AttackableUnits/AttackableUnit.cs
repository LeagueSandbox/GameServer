using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits
{
    public class AttackableUnit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;

        protected Stats Stats { get; }
        private float _statUpdateTimer;
        public bool IsModelUpdated { get; set; }
        public bool IsDead { get; protected set; }
        private string _model;
        public string Model
        {
            get { return _model; }
            set
            {
                _model = value;
                IsModelUpdated = true;
            }
        }
        protected Logger _logger = Program.ResolveDependency<Logger>();
        public InventoryManager Inventory { get; protected set; }
        public int KillDeathCounter { get; protected set; }

        public AttackableUnit(
            string model,
            Stats stats,
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

        public Stats GetStats()
        {
            return Stats;
        }

        public override void update(float diff)
        {
            base.update(diff);

            _statUpdateTimer += diff;
            while (_statUpdateTimer >= 500)
            { // update Stats (hpregen, manaregen) every 0.5 seconds
                Stats.update(_statUpdateTimer);
                _statUpdateTimer -= 500;
            }
        }

        public override float getMoveSpeed()
        {
            return Stats.MoveSpeed.Total;
        }

        public virtual void die(AttackableUnit killer)
        {
            setToRemove();
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            var exp = _game.Map.MapGameScript.GetExperienceFor(this);
            var champs = _game.ObjectManager.GetChampionsInRange(this, EXP_RANGE, true);
            //Cull allied champions
            champs.RemoveAll(l => l.Team == Team);

            if (champs.Count > 0)
            {
                float expPerChamp = exp / champs.Count;
                foreach (var c in champs)
                {
                    c.GetStats().Experience += expPerChamp;
                    _game.PacketNotifier.NotifyAddXP(c, expPerChamp);
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

                cKiller.GetStats().Gold += gold;
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

        public virtual bool isInDistress()
        {
            return false; //return DistressCause;
        }

        public virtual void TakeDamage(AttackableUnit attacker, float damage, DamageType type, DamageSource source,
            DamageText damageText)
        {
            float defense = 0;
            float regain = 0;
            var attackerStats = attacker.GetStats();

            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = GetStats().Armor.Total;
                    defense = (1 - attackerStats.ArmorPenetration.PercentBonus) * defense -
                              attackerStats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = GetStats().MagicPenetration.Total;
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


            GetStats().CurrentHealth = Math.Max(0.0f, GetStats().CurrentHealth - damage);
            if (!IsDead && GetStats().CurrentHealth <= 0)
            {
                IsDead = true;
                die(attacker);
            }

            _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
            _game.PacketNotifier.NotifyUpdatedStats(this, false);

            // Get health from lifesteal/spellvamp
            if (regain > 0)
            {
                attackerStats.CurrentHealth = Math.Min(attackerStats.HealthPoints.Total,
                    attackerStats.CurrentHealth + regain * damage);
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

        public virtual void TakeDamageBySpell(AttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit, Spell spell)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }

            ApiEventManager.OnSpellHit.Publish(this, spell);

            TakeDamage(attacker, damage, type, source, text);
        }
    }

    public enum UnitAnnounces : byte
    {
        Death = 0x04,
        InhibitorDestroyed = 0x1F,
        InhibitorAboutToSpawn = 0x20,
        InhibitorSpawned = 0x21,
        TurretDestroyed = 0x24,
        SummonerDisconnected = 0x47,
        SummonerReconnected = 0x48
    }

    public enum ClassifyUnit
    {
        ChampionAttackingChampion = 1,
        MinionAttackingChampion = 2,
        MinionAttackingMinion = 3,
        TurretAttackingMinion = 4,
        ChampionAttackingMinion = 5,
        Placeable = 6,
        MeleeMinion = 7,
        CasterMinion = 8,
        SuperOrCannonMinion = 9,
        Turret = 10,
        Champion = 11,
        Inhibitor = 12,
        Nexus = 13,
        Default = 14
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
        GreenShield = 0x01,
        MagicShield = 0x02,
        NormalShield = 0x03
    }
}
