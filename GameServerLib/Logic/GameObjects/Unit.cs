using LeagueSandbox.GameServer.Core.Logic;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
using LeagueSandbox.GameServer.Logic.API;
using System.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
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
    
    public enum BuffSlotType
    {
        New,
        Existing
    }

    public class Unit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;
        internal const long UPDATE_TIME = 500;

        protected Stats Stats { get; }
        public InventoryManager Inventory { get; protected set; }
        protected ItemManager _itemManager = Program.ResolveDependency<ItemManager>();
        protected PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        private Random random = new Random();

        public CharData CharData { get; protected set; }
        public SpellData AASpellData { get; protected set; }
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
        private Buff[] AppliedBuffs { get; }

        public List<BuffGameScriptController> BuffGameScriptControllers { get; private set; }

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

        private List<UnitCrowdControl> crowdControlList = new List<UnitCrowdControl>();

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
            BuffGameScriptControllers = new List<BuffGameScriptController>();
            AppliedBuffs = new Buff[256];
            Stats = stats;
            Model = model;
            CharData = _game.Config.ContentManager.GetCharData(Model);
            Stats.LoadStats(CharData);
            AutoAttackDelay = 0;
            AutoAttackProjectileSpeed = 500;
            IsMelee = CharData.IsMelee;
            Stats.CurrentMana = stats.ManaPoints.Total;
            Stats.CurrentHealth = stats.HealthPoints.Total;
            Stats.AttackSpeedMultiplier.BaseValue = 1.0f;

            if (CharData.PathfindingCollisionRadius > 0)
            {
                CollisionRadius = CharData.PathfindingCollisionRadius;
            }
            else if (collisionRadius > 0)
            {
                CollisionRadius = collisionRadius;
            }
            else
            {
                CollisionRadius = 40;
            }
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

        public void ApplyCrowdControl(UnitCrowdControl cc)
        {
            if (cc.IsTypeOf(CrowdControlType.Stun) || cc.IsTypeOf(CrowdControlType.Root))
            {
                this.StopMovement();
            }
            crowdControlList.Add(cc);
        }
        public void RemoveCrowdControl(UnitCrowdControl cc)
        {
            crowdControlList.Remove(cc);
        }
        public void ClearAllCrowdControl()
        {
            crowdControlList.Clear();
        }
        public bool HasCrowdControl(CrowdControlType ccType)
        {
            return crowdControlList.FirstOrDefault((cc)=>cc.IsTypeOf(ccType)) != null;
        }
        public void AddStatModifier(ChampionStatModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        public void UpdateStatModifier(ChampionStatModifier statModifier)
        {
            Stats.UpdateModifier(statModifier);
        }

        public void RemoveStatModifier(ChampionStatModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }

        public BuffGameScriptController AddBuffGameScript(String buffNamespace, String buffClass, Spell ownerSpell, float removeAfter = -1f, bool isUnique = false)
        {
            if (isUnique)
            {
                RemoveBuffGameScriptsWithName(buffNamespace, buffClass);
            }

            BuffGameScriptController buffController = 
                new BuffGameScriptController(this, buffNamespace, buffClass, ownerSpell, duration: removeAfter);
            BuffGameScriptControllers.Add(buffController);
            buffController.ActivateBuff();

            return buffController;
        }
        public void RemoveBuffGameScript(BuffGameScriptController buffController)
        {
            buffController.DeactivateBuff();
            BuffGameScriptControllers.Remove(buffController);
        }
        public bool HasBuffGameScriptActive(String buffNamespace, String buffClass)
        {
            foreach (BuffGameScriptController b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) return true;
            }
            return false;
        }
        public void RemoveBuffGameScriptsWithName(String buffNamespace, String buffClass)
        {
            foreach (BuffGameScriptController b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) b.DeactivateBuff();
            }
            BuffGameScriptControllers.RemoveAll((b) => b.NeedsRemoved());
        }
        public void StopMovement()
        {
            this.SetWaypoints(new List<Vector2> { this.GetPosition(), this.GetPosition() });
        }

        public override void update(float diff)
        {
            _timerUpdate += diff;
            if (_timerUpdate >= UPDATE_TIME)
            {
                _timerUpdate = 0;
            }

            foreach(UnitCrowdControl cc in crowdControlList)
            {
                cc.Update(diff);
            }
            crowdControlList.RemoveAll((cc)=>cc.IsDead());

            BuffGameScriptControllers.RemoveAll((b) => b.NeedsRemoved());

            var onUpdate = _scriptEngine.GetStaticMethod<Action<Unit, double>>(Model, "Passive", "OnUpdate");
            onUpdate?.Invoke(this, diff);

            UpdateAutoAttackTarget(diff);

            base.update(diff);

            _statUpdateTimer += diff;
            if (_statUpdateTimer >= 500)
            { // update Stats (hpregen, manaregen) every 0.5 seconds
                Stats.update(_statUpdateTimer);
                _statUpdateTimer = 0;
            }
        }

        public void UpdateAutoAttackTarget(float diff)
        {
            if (HasCrowdControl(CrowdControlType.Disarm) || HasCrowdControl(CrowdControlType.Stun))
            {
                return;
            }
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
                if (TargetUnit.IsDead || !_game.ObjectManager.TeamHasVisionOn(Team, TargetUnit))
                {
                    SetTargetUnit(null);
                    IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(this, null);
                    _hasMadeInitialAttack = false;

                }
                else if (IsAttacking && AutoAttackTarget != null)
                {
                    _autoAttackCurrentDelay += diff / 1000.0f;
                    if (_autoAttackCurrentDelay >= AutoAttackDelay / Stats.AttackSpeedMultiplier.Total)
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
                            _game.ObjectManager.AddObject(p);
                            _game.PacketNotifier.NotifyShowProjectile(p);
                        }
                        else
                        {
                            AutoAttackHit(AutoAttackTarget);
                        }
                        _autoAttackCurrentCooldown = 1.0f / (Stats.GetTotalAttackSpeed());
                        IsAttacking = false;
                    }

                }
                else if (GetDistanceTo(TargetUnit) <= Stats.Range.Total)
                {
                    refreshWaypoints();
                    _isNextAutoCrit = random.Next(0, 100) < Stats.CriticalChance.Total * 100;
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
                    || !_game.ObjectManager.TeamHasVisionOn(Team, AutoAttackTarget)
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
            return Stats.MoveSpeed.Total;
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
                var onCollideWithTerrain = _scriptEngine.GetStaticMethod<Action<Unit>>(Model, "Passive", "onCollideWithTerrain");
                onCollideWithTerrain?.Invoke(this);
            }
            else
            {
                var onCollide = _scriptEngine.GetStaticMethod<Action<Unit, Unit>>(Model, "Passive", "onCollide");
                onCollide?.Invoke(this, collider as Unit);
            }
        }

        /// <summary>
        /// This is called by the AA projectile when it hits its target
        /// </summary>
        public virtual void AutoAttackHit(Unit target)
        {
            if (HasCrowdControl(CrowdControlType.Blind)) {
                DealDamageTo(target, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageText.DAMAGE_TEXT_MISS);
                return;
            }

            var damage = Stats.AttackDamage.Total;
            if (_isNextAutoCrit)
            {
                damage *= Stats.getCritDamagePct();
            }

            var onAutoAttack = _scriptEngine.GetStaticMethod<Action<Unit, Unit>>(Model, "Passive", "OnAutoAttack");
            onAutoAttack?.Invoke(this, target);

            DealDamageTo(target, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             _isNextAutoCrit);
        }

        public virtual void DealDamageTo(Unit target, float damage, DamageType type, DamageSource source, DamageText damageText)
        {
            float defense = 0;
            float regain = 0;
            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = target.GetStats().Armor.Total;
                    defense = (1 - Stats.ArmorPenetration.PercentBonus) * defense - Stats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = target.GetStats().MagicPenetration.Total;
                    defense = (1 - Stats.MagicPenetration.PercentBonus) * defense - Stats.MagicPenetration.FlatBonus;
                    break;
            }

            switch (source)
            {
                case DamageSource.DAMAGE_SOURCE_SPELL:
                    regain = Stats.SpellVamp.Total;
                    break;
                case DamageSource.DAMAGE_SOURCE_ATTACK:
                    regain = Stats.LifeSteal.Total;
                    break;
            }

            //Damage dealing. (based on leagueoflegends' wikia)
            damage = defense >= 0 ? (100 / (100 + defense)) * damage : (2 - (100 / (100 - defense))) * damage;

            if (target.HasCrowdControl(CrowdControlType.Invulnerable))
            {
                bool attackerIsFountainTurret = this is LaneTurret && (this as LaneTurret).Type == TurretType.FountainTurret;
                if (attackerIsFountainTurret == false)
                {
                    damage = 0;
                    damageText = DamageText.DAMAGE_TEXT_INVULNERABLE;
                }
            }
            ApiEventManager.OnUnitDamageTaken.Publish(target);

            target.GetStats().CurrentHealth = Math.Max(0.0f, target.GetStats().CurrentHealth - damage);
            if (!target.IsDead && target.GetStats().CurrentHealth <= 0)
            {
                target.IsDead = true;
                target.die(this);
            }
            _game.PacketNotifier.NotifyDamageDone(this, target, damage, type, damageText);
            _game.PacketNotifier.NotifyUpdatedStats(target, false);

            //Get health from lifesteal/spellvamp
            if (regain != 0)
            {
                Stats.CurrentHealth = Math.Min(Stats.HealthPoints.Total, Stats.CurrentHealth + regain * damage);
                _game.PacketNotifier.NotifyUpdatedStats(this, false);
            }
        }

        public virtual void DealDamageTo(Unit target, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }
            DealDamageTo(target, damage, type, source, text);
        }

        public virtual void die(Unit killer)
        {
            setToRemove();
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            var onDie = _scriptEngine.GetStaticMethod<Action<Unit, Unit>>(Model, "Passive", "OnDie");
            onDie?.Invoke(this, killer);

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
            RemoveBuffSlot(b);
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
            if (TargetUnit == null || (GetDistanceTo(TargetUnit) <= Stats.Range.Total && Waypoints.Count == 1))
                return;

            if (GetDistanceTo(TargetUnit) <= Stats.Range.Total - 2.0f)
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

        public byte GetNewBuffSlot(Buff b)
        {
            byte slot = GetBuffSlot(b, BuffSlotType.New);
            AppliedBuffs[slot] = b;
            return slot;
        }

        public void RemoveBuffSlot(Buff b)
        {
            byte slot = GetBuffSlot(b, BuffSlotType.Existing);
            if (slot != 0x00) // Only remove the buff if it's already been applied
            {
                AppliedBuffs[slot] = null;
            }
        }

        private byte GetBuffSlot(Buff b, BuffSlotType type)
        {
            switch (type)
            {
                case BuffSlotType.New:
                    for (byte i = 0; i < AppliedBuffs.Length; i++) // Find the first open slot
                    {
                        if (AppliedBuffs[i] != null) // If the slot is already used, continue
                            continue;
                        return i; // If the slot is open, return this slot
                    }
                    throw new Exception("No open slot found.");
                case BuffSlotType.Existing:
                    foreach (var buff in AppliedBuffs) // Iterate through each buff in AppliedBuffs
                    {
                        if (!buff.Equals(b))
                            continue;
                        return buff.Slot; // Returns the slot corresponding to the buff
                    }
                    return 0x00; // Buff isn't applied
                default:
                    throw new ArgumentException("Invalid flag given.", nameof(type));
            }
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
