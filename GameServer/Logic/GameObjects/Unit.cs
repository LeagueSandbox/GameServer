using LeagueSandbox.GameServer.Core.Logic;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using NLua.Exceptions;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;

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

    public class Unit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;
        internal const long UPDATE_TIME = 500;

        protected Stats stats;
        public InventoryManager Inventory { get; protected set; }
        protected ItemManager _itemManager = Program.ResolveDependency<ItemManager>();
        protected RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

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
        private long _statUpdateTimer;
        private uint _autoAttackProjId;
        public MoveOrder MoveOrder { get; set; }

        /**
         * Unit we want to attack as soon as in range
         */
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
        protected IScriptEngine _scriptEngine = new LuaScriptEngine();
        protected Logger _logger = Program.ResolveDependency<Logger>();

        public int KillDeathCounter { get; protected set; }
        private object _buffsLock = new object();
        private Dictionary<string, Buff> _buffs = new Dictionary<string, Buff>();

        private long _timerUpdate;

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

        public virtual void LoadLua()
        {
            _scriptEngine = new LuaScriptEngine();

            _scriptEngine.Execute("package.path = 'LuaLib/?.lua;' .. package.path");
            _scriptEngine.Execute(@"
                function onAutoAttack(target)
                end");
            _scriptEngine.Execute(@"
                function onUpdate(diff)
                end");
            _scriptEngine.Execute(@"
                function onDealDamage(target, damage, type, source)
                end");
            _scriptEngine.Execute(@"
                function onDie(killer)
                end");
            _scriptEngine.Execute(@"
                function onDamageTaken(attacker, damage, type, source)
                end");

            ApiFunctionManager.AddBaseFunctionToLuaScript(_scriptEngine);
        }

        public Stats GetStats()
        {
            return stats;
        }

        public override void update(long diff)
        {
            _timerUpdate += diff;
            if (_timerUpdate >= UPDATE_TIME)
            {
                if (_scriptEngine.IsLoaded())
                {
                    try
                    {
                        _scriptEngine.SetGlobalVariable("diff", _timerUpdate);
                        _scriptEngine.SetGlobalVariable("me", this);
                        _scriptEngine.Execute("onUpdate(diff)");
                    }
                    catch (LuaScriptException e)
                    {
                        _logger.LogCoreError("LUA ERROR : " + e.Message);
                    }
                }
                _timerUpdate = 0;
            }

            if (IsDead)
            {
                if (TargetUnit != null)
                {
                    SetTargetUnit(null);
                    AutoAttackTarget = null;
                    IsAttacking = false;
                    _game.PacketNotifier.notifySetTarget(this, null);
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
                    _game.PacketNotifier.notifySetTarget(this, null);
                    _hasMadeInitialAttack = false;

                }
                else if (IsAttacking && AutoAttackTarget != null)
                {
                    _autoAttackCurrentDelay += diff / 1000.0f;
                    if (_autoAttackCurrentDelay >= AutoAttackDelay / stats.AttackSpeedMultiplier.Total)
                    {
                        if (!IsMelee)
                        {
                            Projectile p = new Projectile(
                                X,
                                Y,
                                5,
                                this,
                                AutoAttackTarget,
                                null,
                                AutoAttackProjectileSpeed,
                                0,
                                0,
                                _autoAttackProjId
                            );
                            _game.Map.AddObject(p);
                            _game.PacketNotifier.notifyShowProjectile(p);
                        }
                        else
                        {
                            autoAttackHit(AutoAttackTarget);
                        }
                        _autoAttackCurrentCooldown = 1.0f / (stats.GetTotalAttackSpeed());
                        IsAttacking = false;
                    }

                }
                else if (GetDistanceTo(TargetUnit) <= stats.Range.Total)
                {
                    refreshWaypoints();
                    _isNextAutoCrit = new Random().Next(0, 100) < stats.CriticalChance.Total * 100;
                    if (_autoAttackCurrentCooldown <= 0)
                    {
                        IsAttacking = true;
                        _autoAttackCurrentDelay = 0;
                        _autoAttackProjId = _networkIdManager.GetNewNetID();
                        AutoAttackTarget = TargetUnit;

                        if (!_hasMadeInitialAttack)
                        {
                            _hasMadeInitialAttack = true;
                            _game.PacketNotifier.notifyBeginAutoAttack(
                                this,
                                TargetUnit,
                                _autoAttackProjId,
                                _isNextAutoCrit
                            );
                        }
                        else
                        {
                            _nextAttackFlag = !_nextAttackFlag; // The first auto attack frame has occurred
                            _game.PacketNotifier.notifyNextAutoAttack(
                                this,
                                TargetUnit,
                                _autoAttackProjId,
                                _isNextAutoCrit,
                                _nextAttackFlag
                                );
                        }

                        var attackType = IsMelee ? AttackType.ATTACK_TYPE_MELEE : AttackType.ATTACK_TYPE_TARGETED;
                        _game.PacketNotifier.notifyOnAttack(this, TargetUnit, attackType);
                    }

                }
                else
                {
                    refreshWaypoints();
                }

            }
            else if (IsAttacking)
            {
                if (
                    AutoAttackTarget == null
                    || AutoAttackTarget.IsDead
                    || !_game.Map.TeamHasVisionOn(Team, AutoAttackTarget)
                )
                {
                    IsAttacking = false;
                    _hasMadeInitialAttack = false;
                    AutoAttackTarget = null;
                }
            }

            base.update(diff);

            if (_autoAttackCurrentCooldown > 0)
            {
                _autoAttackCurrentCooldown -= diff / 1000.0f;
            }

            _statUpdateTimer += diff;
            if (_statUpdateTimer >= 500)
            { // update stats (hpregen, manaregen) every 0.5 seconds
                stats.update(_statUpdateTimer);
                _statUpdateTimer = 0;
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
            /*if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("object", collider);
                    _scriptEngine.Execute("onCollide(object)");
                }
                catch (LuaException e)
                {
                    Logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }*/
        }

        /**
        * This is called by the AA projectile when it hits its target
        */
        public virtual void autoAttackHit(Unit target)
        {
            var damage = stats.AttackDamage.Total;
            if (_isNextAutoCrit)
            {
                damage *= stats.getCritDamagePct();
            }

            dealDamageTo(target, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             _isNextAutoCrit);

            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("target", target);
                    _scriptEngine.Execute("onAutoAttack(target)");
                }
                catch (LuaScriptException e)
                {
                    _logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }
        }

        public virtual void dealDamageTo(Unit target, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }

            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("target", target);
                    _scriptEngine.SetGlobalVariable("damage", damage);
                    _scriptEngine.SetGlobalVariable("type", type);
                    _scriptEngine.SetGlobalVariable("source", source);
                    _scriptEngine.Execute("onDealDamage(target, damage, type, source)");
                }
                catch (LuaScriptException e)
                {
                    _logger.LogCoreError("ERROR LUA : " + e.Message);
                }
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
            if (target._scriptEngine.IsLoaded())
            {
                try
                {
                    target._scriptEngine.SetGlobalVariable("attacker", this);
                    target._scriptEngine.SetGlobalVariable("damage", damage);
                    target._scriptEngine.SetGlobalVariable("type", type);
                    target._scriptEngine.SetGlobalVariable("source", source);
                    target._scriptEngine.Execute(@"
                        function modifyIncomingDamage(value)
                            damage = value
                        end");
                    target._scriptEngine.Execute("onDamageTaken(attacker, damage, type, source)");
                }
                catch (LuaScriptException e)
                {
                    _logger.LogCoreError("LUA ERROR : " + e);
                }
            }

            target.GetStats().CurrentHealth = Math.Max(0.0f, target.GetStats().CurrentHealth - damage);
            if (!target.IsDead && target.GetStats().CurrentHealth <= 0)
            {
                target.IsDead = true;
                target.die(this);
            }
            _game.PacketNotifier.notifyDamageDone(this, target, damage, type, text);

            //Get health from lifesteal/spellvamp
            if (regain != 0)
            {
                stats.CurrentHealth = Math.Min(stats.HealthPoints.Total, stats.CurrentHealth + regain * damage);
                _game.PacketNotifier.notifyUpdatedStats(this);
            }
        }

        public virtual void die(Unit killer)
        {
            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("killer", killer);
                    _scriptEngine.Execute("onDie(killer)");
                }
                catch (LuaScriptException e)
                {
                    _logger.LogCoreError(string.Format("LUA ERROR : {0}", e.Message));
                }
            }

            setToRemove();
            _game.Map.StopTargeting(this);

            _game.PacketNotifier.notifyNpcDie(this, killer);

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
                _game.PacketNotifier.notifyAddGold(cKiller, this, gold);

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
                Target t = new Target(Waypoints[Waypoints.Count - 1]);
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
