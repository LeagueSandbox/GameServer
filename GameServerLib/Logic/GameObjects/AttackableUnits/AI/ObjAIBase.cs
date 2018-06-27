using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAiBase : AttackableUnit
    {
        private Buff[] AppliedBuffs { get; }
        private List<BuffGameScriptController> BuffGameScriptControllers { get; }
        private object BuffsLock { get; }
        private Dictionary<string, Buff> Buffs { get; }

        private List<UnitCrowdControl> _crowdControlList = new List<UnitCrowdControl>();
        protected ItemManager _ıtemManager = Program.ResolveDependency<ItemManager>();
        protected CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();


        /// <summary>
        /// Unit we want to attack as soon as in range
        /// </summary>
        public AttackableUnit TargetUnit { get; set; }
        public AttackableUnit AutoAttackTarget { get; set; }
        public CharData CharData { get; protected set; }
        public SpellData AaSpellData { get; protected set; }
        private bool _isNextAutoCrit;
        public float AutoAttackDelay { get; set; }
        public float AutoAttackProjectileSpeed { get; set; }
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        public bool IsAttacking { protected get; set; }
        protected internal bool HasMadeInitialAttack;
        private bool _nextAttackFlag;
        private uint _autoAttackProjId;
        public MoveOrder MoveOrder { get; set; }
        public bool IsCastingSpell { get; set; }
        public bool IsMelee { get; set; }
        private Random _random = new Random();

        public ObjAiBase(string model, Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            CharData = _game.Config.ContentManager.GetCharData(Model);
            Stats.LoadStats(CharData);

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
            Stats.CurrentMana = stats.ManaPoints.Total;
            Stats.CurrentHealth = stats.HealthPoints.Total;
            if (!string.IsNullOrEmpty(model))
            {
                AaSpellData = _game.Config.ContentManager.GetSpellData(model + "BasicAttack");
                AutoAttackDelay = AaSpellData.CastFrame / 30.0f;
                AutoAttackProjectileSpeed = AaSpellData.MissileSpeed;
                IsMelee = CharData.IsMelee;
            }
            else
            {
                AutoAttackDelay = 0;
                AutoAttackProjectileSpeed = 500;
                IsMelee = true;
            }

            AppliedBuffs = new Buff[256];
            BuffGameScriptControllers = new List<BuffGameScriptController>();
            BuffsLock = new object();
            Buffs = new Dictionary<string, Buff>();
        }

        public BuffGameScriptController AddBuffGameScript(string buffNamespace, string buffClass, Spell ownerSpell, float removeAfter = -1f, bool isUnique = false)
        {
            if (isUnique)
            {
                RemoveBuffGameScriptsWithName(buffNamespace, buffClass);
            }

            var buffController =
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
        public bool HasBuffGameScriptActive(string buffNamespace, string buffClass)
        {
            foreach (var b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) return true;
            }
            return false;
        }
        public void RemoveBuffGameScriptsWithName(string buffNamespace, string buffClass)
        {
            foreach (var b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) b.DeactivateBuff();
            }
            BuffGameScriptControllers.RemoveAll(b => b.NeedsRemoved());
        }

        public List<BuffGameScriptController> GetBuffGameScriptController()
        {
            return BuffGameScriptControllers;
        }

        public Dictionary<string, Buff> GetBuffs()
        {
            var toReturn = new Dictionary<string, Buff>();
            lock (BuffsLock)
            {
                foreach (var buff in Buffs)
                    toReturn.Add(buff.Key, buff.Value);

                return toReturn;
            }
        }

        public int GetBuffsCount()
        {
            return Buffs.Count;
        }

        //todo: use statmods
        public Buff GetBuff(string name)
        {
            lock (BuffsLock)
            {
                if (Buffs.ContainsKey(name))
                    return Buffs[name];
                return null;
            }
        }

        public void AddStatModifier(StatsModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        public void RemoveStatModifier(StatsModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }

        public void AddBuff(Buff b)
        {
            lock (BuffsLock)
            {
                if (!Buffs.ContainsKey(b.Name))
                {
                    Buffs.Add(b.Name, b);
                }
                else
                {
                    Buffs[b.Name].TimeElapsed = 0; // if buff already exists, just restart its timer
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
            lock (BuffsLock)
            {
                Buffs.Remove(b);
            }
        }

        public byte GetNewBuffSlot(Buff b)
        {
            var slot = GetBuffSlot();
            AppliedBuffs[slot] = b;
            return slot;
        }

        public void RemoveBuffSlot(Buff b)
        {
            var slot = GetBuffSlot(b);
            AppliedBuffs[slot] = null;
        }

        public void ApplyCrowdControl(UnitCrowdControl cc)
        {
            if (cc.IsTypeOf(CrowdControlType.STUN) || cc.IsTypeOf(CrowdControlType.ROOT))
            {
                StopMovement();
            }
            _crowdControlList.Add(cc);
        }
        public void RemoveCrowdControl(UnitCrowdControl cc)
        {
            _crowdControlList.Remove(cc);
        }
        public void ClearAllCrowdControl()
        {
            _crowdControlList.Clear();
        }
        public bool HasCrowdControl(CrowdControlType ccType)
        {
            return _crowdControlList.FirstOrDefault(cc => cc.IsTypeOf(ccType)) != null;
        }

        public void StopMovement()
        {
            SetWaypoints(new List<Vector2> { GetPosition(), GetPosition() });
        }

        public virtual void RefreshWaypoints()
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

        public ClassifyUnit ClassifyTarget(AttackableUnit target)
        {
            var ai = target as ObjAiBase;
            if (ai != null)
            {
                if (ai.TargetUnit != null && ai.TargetUnit.IsInDistress()) // If an ally is in distress, target this unit. (Priority 1~5)
                {
                    if (target is Champion && ai.TargetUnit is Champion) // If it's a champion attacking an allied champion
                    {
                        return ClassifyUnit.CHAMPION_ATTACKING_CHAMPION;
                    }

                    if (target is Minion && ai.TargetUnit is Champion) // If it's a minion attacking an allied champion.
                    {
                        return ClassifyUnit.MINION_ATTACKING_CHAMPION;
                    }

                    if (target is Minion && ai.TargetUnit is Minion) // Minion attacking minion
                    {
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    }

                    if (target is BaseTurret && ai.TargetUnit is Minion) // Turret attacking minion
                    {
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                    }

                    if (target is Champion && ai.TargetUnit is Minion) // Champion attacking minion
                    {
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    }
                }
            }


            var p = target as Placeable;
            if (p != null)
            {
                return ClassifyUnit.PLACEABLE;
            }

            var m = target as Minion;
            if (m != null)
            {
                switch (m.GetType())
                {
                    case MinionSpawnType.MINION_TYPE_MELEE:
                        return ClassifyUnit.MELEE_MINION;
                    case MinionSpawnType.MINION_TYPE_CASTER:
                        return ClassifyUnit.CASTER_MINION;
                    case MinionSpawnType.MINION_TYPE_CANNON:
                    case MinionSpawnType.MINION_TYPE_SUPER:
                        return ClassifyUnit.SUPER_OR_CANNON_MINION;
                }
            }

            if (target is BaseTurret)
            {
                return ClassifyUnit.TURRET;
            }

            if (target is Champion)
            {
                return ClassifyUnit.CHAMPION;
            }

            if (target is Inhibitor && !target.IsDead)
            {
                return ClassifyUnit.INHIBITOR;
            }

            if (target is Nexus)
            {
                return ClassifyUnit.NEXUS;
            }

            return ClassifyUnit.DEFAULT;
        }


        public void SetTargetUnit(AttackableUnit target)
        {
            TargetUnit = target;
            RefreshWaypoints();
        }

        private byte GetBuffSlot(Buff buffToLookFor = null)
        {
            for (byte i = 1; i < AppliedBuffs.Length; i++) // Find the first open slot or the slot corresponding to buff
            {
                if (AppliedBuffs[i] == buffToLookFor)
                {
                    return i;
                }
            }
            throw new Exception("No slot found with requested value"); // If no open slot or no corresponding slot
        }

        /// <summary>
        /// This is called by the AA projectile when it hits its target
        /// </summary>
        public virtual void AutoAttackHit(AttackableUnit target)
        {
            if (HasCrowdControl(CrowdControlType.BLIND))
            {
                target.TakeDamage(this, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageText.DAMAGE_TEXT_MISS);
                return;
            }

            var damage = Stats.AttackDamage.Total;
            if (_isNextAutoCrit)
            {
                damage *= Stats.CriticalDamage.Total;
            }

            var onAutoAttack = _scriptEngine.GetStaticMethod<Action<AttackableUnit, AttackableUnit>>(Model, "Passive", "OnAutoAttack");
            onAutoAttack?.Invoke(this, target);

            target.TakeDamage(this, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                DamageSource.DAMAGE_SOURCE_ATTACK,
                _isNextAutoCrit);
        }


        public void UpdateAutoAttackTarget(float diff)
        {
            if (HasCrowdControl(CrowdControlType.DISARM) || HasCrowdControl(CrowdControlType.STUN))
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
                    HasMadeInitialAttack = false;
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
                    HasMadeInitialAttack = false;

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
                    RefreshWaypoints();
                    _isNextAutoCrit = _random.Next(0, 100) < Stats.CriticalChance.Total * 100;
                    if (_autoAttackCurrentCooldown <= 0)
                    {
                        IsAttacking = true;
                        _autoAttackCurrentDelay = 0;
                        _autoAttackProjId = _networkIdManager.GetNewNetId();
                        AutoAttackTarget = TargetUnit;

                        if (!HasMadeInitialAttack)
                        {
                            HasMadeInitialAttack = true;
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
                    RefreshWaypoints();
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
                    HasMadeInitialAttack = false;
                    AutoAttackTarget = null;
                }
            }

            if (_autoAttackCurrentCooldown > 0)
            {
                _autoAttackCurrentCooldown -= diff / 1000.0f;
            }
        }


        public override void Update(float diff)
        {
            foreach (var cc in _crowdControlList)
            {
                cc.Update(diff);
            }
            _crowdControlList.RemoveAll(cc => cc.IsDead());

            var onUpdate = _scriptEngine.GetStaticMethod<Action<AttackableUnit, double>>(Model, "Passive", "OnUpdate");
            onUpdate?.Invoke(this, diff);
            BuffGameScriptControllers.RemoveAll(b => b.NeedsRemoved());
            base.Update(diff);
            UpdateAutoAttackTarget(diff);
        }
        public override void Die(AttackableUnit killer)
        {
            var onDie = _scriptEngine.GetStaticMethod<Action<AttackableUnit, AttackableUnit>>(Model, "Passive", "OnDie");
            onDie?.Invoke(this, killer);
            base.Die(killer);
        }

        public override void OnCollision(GameObject collider)
        {
            base.OnCollision(collider);
            if (collider == null)
            {
                var onCollideWithTerrain = _scriptEngine.GetStaticMethod<Action<AttackableUnit>>(Model, "Passive", "onCollideWithTerrain");
                onCollideWithTerrain?.Invoke(this);
            }
            else
            {
                var onCollide = _scriptEngine.GetStaticMethod<Action<AttackableUnit, AttackableUnit>>(Model, "Passive", "onCollide");
                onCollide?.Invoke(this, collider as AttackableUnit);
            }
        }
    }
}
