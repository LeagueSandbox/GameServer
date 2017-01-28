﻿using LeagueSandbox.GameServer.Core.Logic;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum DamageType : byte
    {
        DAMAGE_TYPE_PHYSICAL = 0,
        DAMAGE_TYPE_MAGICAL = 1,
        DAMAGE_TYPE_TRUE = 2
    }

    public enum DamageText : byte
    {
        DAMAGE_TEXT_INVULNERABLE = 0x00,
        DAMAGE_TEXT_DODGE = 0x02,
        DAMAGE_TEXT_CRITICAL = 0x03,
        DAMAGE_TEXT_NORMAL = 0x04,
        DAMAGE_TEXT_MISS = 0x05,
    }

    public enum DamageSource
    {
        DAMAGE_SOURCE_ATTACK,
        DAMAGE_SOURCE_SPELL,
        DAMAGE_SOURCE_SUMMONER_SPELL, //Ignite shouldn't destroy Banshee's
        DAMAGE_SOURCE_PASSIVE //Red/Thornmail shouldn't as well
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

    public class Unit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;
        internal const long UPDATE_TIME = 500;

        protected Stats stats;
        public InventoryManager Inventory { get; protected set; }
        protected ItemManager _itemManager = Program.ResolveDependency<ItemManager>();
        protected RAFManager _rafManager = Program.ResolveDependency<RAFManager>();
        protected PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        private Random random = new Random();

        public float AutoAttackDelay { get; set; }
        public float AutoAttackProjectileSpeed { get; set; }
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        public bool IsAttacking { protected get; set; }
        public bool IsModelUpdated { get; set; }
        public bool IsMelee { get; set; }
        protected internal bool _hasMadeInitialAttack;
        private bool _nextAttackFlag;
        public Unit DistressCause { get; protected set; }
        private float _statUpdateTimer;
        private uint _autoAttackProjId;
        public MoveOrder MoveOrder { get; set; }

        /// <summary>
        /// Unit we want to attack as soon as in range
        /// </summary>
        public Unit TargetUnit { get; set; }
        public Unit AutoAttackTarget { get; set; }

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

        private bool _isNextAutoCrit;
        protected CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();
        protected Logger _logger = Program.ResolveDependency<Logger>();

        public int KillDeathCounter { get; protected set; }
        private object _buffsLock = new object();
        private Dictionary<string, Buff> _buffs = new Dictionary<string, Buff>();

        private float _timerUpdate;

        public bool IsCastingSpell { get; set; }

        public Unit(
            string model,
            Stats stats,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, visionRadius, netId)

        {
            this.stats = stats;
            this.Model = model;
        }
        public Stats GetStats()
        {
            return stats;
        }

        public override void update(float diff)
        {
            _timerUpdate += diff;
            if (_timerUpdate >= UPDATE_TIME)
            {
                _timerUpdate = 0;
            }

            UpdateAutoAttackTarget(diff);

            base.update(diff);

            _statUpdateTimer += diff;
            if (_statUpdateTimer >= 500)
            { // update stats (hpregen, manaregen) every 0.5 seconds
                stats.update(_statUpdateTimer);
                _statUpdateTimer = 0;
            }
        }

        public void UpdateAutoAttackTarget(float diff)
        {
            if (IsDead)
            {
                if (TargetUnit != null)
                {
                    SetTargetUnit(null);
                    AutoAttackTarget = null;
                    IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(this, null);
                    _hasMadeInitialAttack = false;
                }
                return;
            }

            if (TargetUnit != null)
            {
                if (TargetUnit.IsDead || !_game.Map.TeamHasVisionOn(Team, TargetUnit))
                {
                    SetTargetUnit(null);
                    IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(this, null);
                    _hasMadeInitialAttack = false;

                }
                else if (IsAttacking && AutoAttackTarget != null)
                {
                    _autoAttackCurrentDelay += diff / 1000.0f;
                    if (_autoAttackCurrentDelay >= AutoAttackDelay / stats.AttackSpeedMultiplier.Total)
                    {
                        if (!IsMelee)
                        {
                            var p = new Projectile(
                                X,
                                Y,
                                5,
                                this,
                                AutoAttackTarget,
                                null,
                                AutoAttackProjectileSpeed,
                                "",
                                0,
                                _autoAttackProjId
                            );
                            _game.Map.AddObject(p);
                            _game.PacketNotifier.NotifyShowProjectile(p);
                        }
                        else
                        {
                            AutoAttackHit(AutoAttackTarget);
                        }
                        _autoAttackCurrentCooldown = 1.0f / (stats.GetTotalAttackSpeed());
                        IsAttacking = false;
                    }

                }
                else if (GetDistanceTo(TargetUnit) <= stats.Range.Total)
                {
                    refreshWaypoints();
                    _isNextAutoCrit = random.Next(0, 100) < stats.CriticalChance.Total * 100;
                    if (_autoAttackCurrentCooldown <= 0)
                    {
                        IsAttacking = true;
                        _autoAttackCurrentDelay = 0;
                        _autoAttackProjId = _networkIdManager.GetNewNetID();
                        AutoAttackTarget = TargetUnit;

                        if (!_hasMadeInitialAttack)
                        {
                            _hasMadeInitialAttack = true;
                            _game.PacketNotifier.NotifyBeginAutoAttack(
                                this,
                                TargetUnit,
                                _autoAttackProjId,
                                _isNextAutoCrit
                            );
                        }
                        else
                        {
                            _nextAttackFlag = !_nextAttackFlag; // The first auto attack frame has occurred
                            _game.PacketNotifier.NotifyNextAutoAttack(
                                this,
                                TargetUnit,
                                _autoAttackProjId,
                                _isNextAutoCrit,
                                _nextAttackFlag
                                );
                        }

                        var attackType = IsMelee ? AttackType.ATTACK_TYPE_MELEE : AttackType.ATTACK_TYPE_TARGETED;
                        _game.PacketNotifier.NotifyOnAttack(this, TargetUnit, attackType);
                    }

                }
                else
                {
                    refreshWaypoints();
                }

            }
            else if (IsAttacking)
            {
                if (AutoAttackTarget == null
                    || AutoAttackTarget.IsDead
                    || !_game.Map.TeamHasVisionOn(Team, AutoAttackTarget)
                )
                {
                    IsAttacking = false;
                    _hasMadeInitialAttack = false;
                    AutoAttackTarget = null;
                }
            }

            if (_autoAttackCurrentCooldown > 0)
            {
                _autoAttackCurrentCooldown -= diff / 1000.0f;
            }
        }

        public override float getMoveSpeed()
        {
            return stats.MoveSpeed.Total;
        }

        public Dictionary<string, Buff> GetBuffs()
        {
            var toReturn = new Dictionary<string, Buff>();
            lock (_buffsLock)
            {
                foreach (var buff in _buffs)
                    toReturn.Add(buff.Key, buff.Value);

                return toReturn;
            }
        }

        public int GetBuffsCount()
        {
            return _buffs.Count;
        }

        public override void onCollision(GameObject collider)
        {
            base.onCollision(collider);
                if (collider == null)
                {
                    //_scriptEngine.RunFunction("onCollideWithTerrain");
                }
                else
                {
                   // _scriptEngine.RunFunction("onCollide", collider);
                }
        }

        /// <summary>
        /// This is called by the AA projectile when it hits its target
        /// </summary>
        public virtual void AutoAttackHit(Unit target)
        {
            var damage = stats.AttackDamage.Total;
            if (_isNextAutoCrit)
            {
                damage *= stats.getCritDamagePct();
            }

            DealDamageTo(target, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             _isNextAutoCrit);
        }

        public virtual void DealDamageTo(Unit target, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }
            float defense = 0;
            float regain = 0;
            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = target.GetStats().Armor.Total;
                    defense = (1 - stats.ArmorPenetration.PercentBonus) * defense - stats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = target.GetStats().MagicPenetration.Total;
                    defense = (1 - stats.MagicPenetration.PercentBonus)*defense - stats.MagicPenetration.FlatBonus;
                    break;
            }

            switch (source)
            {
                case DamageSource.DAMAGE_SOURCE_SPELL:
                    regain = stats.SpellVamp.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_ATTACK:
                    regain = stats.LifeSteal.Total;
                    break;
            }

            //Damage dealing. (based on leagueoflegends' wikia)
            damage = defense >= 0 ? (100 / (100 + defense)) * damage : (2 - (100 / (100 - defense))) * damage;
            target.GetStats().CurrentHealth = Math.Max(0.0f, target.GetStats().CurrentHealth - damage);
            if (!target.IsDead && target.GetStats().CurrentHealth <= 0)
            {
                target.IsDead = true;
                target.die(this);
            }
            _game.PacketNotifier.NotifyDamageDone(this, target, damage, type, text);
            _game.PacketNotifier.NotifyUpdatedStats(target, false);

            //Get health from lifesteal/spellvamp
            if (regain != 0)
            {
                stats.CurrentHealth = Math.Min(stats.HealthPoints.Total, stats.CurrentHealth + regain * damage);
                _game.PacketNotifier.NotifyUpdatedStats(this, false);
            }
        }

        public virtual void die(Unit killer)
        {
            setToRemove();
            _game.Map.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            float exp = _game.Map.GetExperienceFor(this);
            var champs = _game.Map.GetChampionsInRange(this, EXP_RANGE, true);
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

                float gold = _game.Map.GetGoldFor(this);
                if (gold <= 0)
                    return;

                cKiller.GetStats().Gold += gold;
                _game.PacketNotifier.NotifyAddGold(cKiller, this, gold);

                if (cKiller.KillDeathCounter < 0)
                {
                    cKiller.ChampionGoldFromMinions += gold;
                    _logger.LogCoreInfo(string.Format(
                        "Adding gold form minions to reduce death spree: {0}",
                        cKiller.ChampionGoldFromMinions
                    ));
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

        public void AddBuff(Buff b)
        {
            lock (_buffsLock)
            {
                if (!_buffs.ContainsKey(b.Name))
                {
                    _buffs.Add(b.Name, b);
                }
                else
                {
                    _buffs[b.Name].TimeElapsed = 0; // if buff already exists, just restart its timer
                }
            }
        }

        public void RemoveBuff(Buff b)
        {
            //TODO add every stat
            RemoveBuff(b.Name);
        }

        public void RemoveBuff(string b)
        {
            lock (_buffsLock)
                _buffs.Remove(b);
        }

        public virtual bool isInDistress()
        {
            return false; //return DistressCause;
        }

        //todo: use statmods
        public Buff GetBuff(string name)
        {
            lock (_buffsLock)
            {
                if (_buffs.ContainsKey(name))
                    return _buffs[name];
                return null;
            }
        }

        public void SetTargetUnit(Unit target)
        {
            if (target == null) // If we are unsetting the target (moving around)
            {
                if (TargetUnit != null) // and we had a target
                    TargetUnit.DistressCause = null; // Unset the distress call
                                                      // TODO: Replace this with a delay?

                IsAttacking = false;
            }
            else
            {
                target.DistressCause = this; // Otherwise set the distress call
            }

            TargetUnit = target;
            refreshWaypoints();
        }

        public virtual void refreshWaypoints()
        {
            if (TargetUnit == null || (GetDistanceTo(TargetUnit) <= stats.Range.Total && Waypoints.Count == 1))
                return;

            if (GetDistanceTo(TargetUnit) <= stats.Range.Total - 2.0f)
            {
                SetWaypoints(new List<Vector2> { new Vector2(X, Y) });
            }
            else
            {
                var t = new Target(Waypoints[Waypoints.Count - 1]);
                if (t.GetDistanceTo(TargetUnit) >= 25.0f)
                {
                    SetWaypoints(new List<Vector2> { new Vector2(X, Y), new Vector2(TargetUnit.X, TargetUnit.Y) });
                }
            }
        }

        public ClassifyUnit ClassifyTarget(Unit target)
        {
            if (target.TargetUnit != null && target.TargetUnit.isInDistress()) // If an ally is in distress, target this unit. (Priority 1~5)
            {
                if (target is Champion && target.TargetUnit is Champion) // If it's a champion attacking an allied champion
                {
                    return ClassifyUnit.ChampionAttackingChampion;
                }

                if (target is Minion && target.TargetUnit is Champion) // If it's a minion attacking an allied champion.
                {
                    return ClassifyUnit.MinionAttackingChampion;
                }

                if (target is Minion && target.TargetUnit is Minion) // Minion attacking minion
                {
                    return ClassifyUnit.MinionAttackingMinion;
                }

                if (target is BaseTurret && target.TargetUnit is Minion) // Turret attacking minion
                {
                    return ClassifyUnit.TurretAttackingMinion;
                }

                if (target is Champion && target.TargetUnit is Minion) // Champion attacking minion
                {
                    return ClassifyUnit.ChampionAttackingMinion;
                }
            }

            var p = target as Placeable;
            if (p != null)
            {
                return ClassifyUnit.Placeable;
            }

            var m = target as Minion;
            if (m != null)
            {
                switch (m.getType())
                {
                    case MinionSpawnType.MINION_TYPE_MELEE:
                        return ClassifyUnit.MeleeMinion;
                    case MinionSpawnType.MINION_TYPE_CASTER:
                        return ClassifyUnit.CasterMinion;
                    case MinionSpawnType.MINION_TYPE_CANNON:
                    case MinionSpawnType.MINION_TYPE_SUPER:
                        return ClassifyUnit.SuperOrCannonMinion;
                }
            }

            if (target is BaseTurret)
            {
                return ClassifyUnit.Turret;
            }

            if (target is Champion)
            {
                return ClassifyUnit.Champion;
            }

            if (target is Inhibitor && !target.IsDead)
            {
                return ClassifyUnit.Inhibitor;
            }

            if (target is Nexus)
            {
                return ClassifyUnit.Nexus;
            }

            return ClassifyUnit.Default;
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
}
