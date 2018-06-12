using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAIBase : AttackableUnit
    {
        private Buff[] AppliedBuffs { get; }
        private List<BuffGameScriptController> BuffGameScriptControllers { get; }
        private object BuffsLock { get; }
        private Dictionary<string, Buff> Buffs { get; }
        
        protected ItemManager _itemManager = Program.ResolveDependency<ItemManager>();
        protected CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();

        /// <summary>
        /// Unit we want to attack as soon as in range
        /// </summary>
        public AttackableUnit TargetUnit { get; set; }
        public AttackableUnit AutoAttackTarget { get; set; }
        public CharData CharData { get; protected set; }
        public SpellData AASpellData { get; protected set; }
        private bool _isNextAutoCrit;
        public float AutoAttackDelay { get; set; }
        public float AutoAttackProjectileSpeed { get; set; }
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        public bool IsAttacking { protected get; set; }
        protected internal bool _hasMadeInitialAttack;
        private bool _nextAttackFlag;
        private uint _autoAttackProjId;
        public MoveOrder MoveOrder { get; set; }
        public bool IsCastingSpell { get; set; }
        private Random _random = new Random();
        public bool MovementUpdated { get; set; }
        protected float _dashSpeed;
        public bool IsDashing { get; set; }

        public List<Vector2> Waypoints { get; private set; }
        public int CurWaypoint { get; protected set; }

        public ObjAIBase(string model, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, collisionRadius, x, y, visionRadius, netId)
        {
            CharData = _game.Config.ContentManager.GetCharData(Model);

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
            if (!string.IsNullOrEmpty(model))
            {
                AASpellData = _game.Config.ContentManager.GetSpellData(model + "BasicAttack");
                AutoAttackDelay = AASpellData.CastFrame / 30.0f;
                AutoAttackProjectileSpeed = AASpellData.MissileSpeed;
                Stats.IsMelee = CharData.IsMelee;
            } 
            else
            {
                AutoAttackDelay = 0;
                AutoAttackProjectileSpeed = 500;
                Stats.IsMelee = true;
            }

            Waypoints = new List<Vector2>();
            
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

            var buffController = new BuffGameScriptController(this, buffNamespace, buffClass, ownerSpell, removeAfter);
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
                if (b.IsBuffSame(buffNamespace, buffClass))
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveBuffGameScriptsWithName(string buffNamespace, string buffClass)
        {
            foreach (var b in BuffGameScriptControllers)
            {
                if (b.IsBuffSame(buffNamespace, buffClass)) b.DeactivateBuff();
            }
            BuffGameScriptControllers.RemoveAll((b) => b.NeedsRemoved());
        }

        public Dictionary<string, Buff> GetBuffs()
        {
            var toReturn = new Dictionary<string, Buff>();
            lock (BuffsLock)
            {
                foreach (var buff in Buffs)
                {
                    toReturn.Add(buff.Key, buff.Value);
                }

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
                {
                    return Buffs[name];
                }

                return null;
            }
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
            byte slot = GetBuffSlot();
            AppliedBuffs[slot] = b;
            return slot;
        }

        public void RemoveBuffSlot(Buff b)
        {
            byte slot = GetBuffSlot(b);
            AppliedBuffs[slot] = null;
        }

        public void StopMovement()
        {
            SetWaypoints(new List<Vector2> { GetPosition(), GetPosition() });
        }

        public void SetWaypoints(List<Vector2> newWaypoints)
        {
            Waypoints = newWaypoints;

            setPosition(Waypoints[0].X, Waypoints[0].Y);
            MovementUpdated = true;
            if (Waypoints.Count == 1)
            {
                Target = null;
                return;
            }

            Target = new Target(Waypoints[1]);
            CurWaypoint = 1;
        }

        public virtual void refreshWaypoints()
        {
            if (TargetUnit == null || (GetDistanceTo(TargetUnit) <= Stats.TotalAttackRange && Waypoints.Count == 1))
            {
                return;
            }

            if (GetDistanceTo(TargetUnit) <= Stats.TotalAttackRange + Stats.FlatPathfindingRadiusMod +
                TargetUnit.Stats.FlatPathfindingRadiusMod)
            {
                SetWaypoints(new List<Vector2> {new Vector2(X, Y)});
            }
            else
            {
                // todo pathfinding
                var t = new Target(Waypoints[Waypoints.Count - 1]);
                if (t.GetDistanceTo(TargetUnit) >= 25.0f)
                {
                    SetWaypoints(new List<Vector2> { new Vector2(X, Y), new Vector2(TargetUnit.X, TargetUnit.Y) });
                }
            }
        }

        public ClassifyUnit ClassifyTarget(AttackableUnit target)
        {
            var ai = target as ObjAIBase;
            if (ai != null)
            {
                if (ai.TargetUnit != null && ai.TargetUnit.isInDistress()) // If an ally is in distress, target this unit. (Priority 1~5)
                {
                    if (target is Champion && ai.TargetUnit is Champion) // If it's a champion attacking an allied champion
                    {
                        return ClassifyUnit.ChampionAttackingChampion;
                    }

                    if (target is Minion && ai.TargetUnit is Champion) // If it's a minion attacking an allied champion.
                    {
                        return ClassifyUnit.MinionAttackingChampion;
                    }

                    if (target is Minion && ai.TargetUnit is Minion) // Minion attacking minion
                    {
                        return ClassifyUnit.MinionAttackingMinion;
                    }

                    if (target is BaseTurret && ai.TargetUnit is Minion) // Turret attacking minion
                    {
                        return ClassifyUnit.TurretAttackingMinion;
                    }

                    if (target is Champion && ai.TargetUnit is Minion) // Champion attacking minion
                    {
                        return ClassifyUnit.ChampionAttackingMinion;
                    }
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


        public void SetTargetUnit(AttackableUnit target)
        {
            TargetUnit = target;
            refreshWaypoints();
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

        public bool HasBuffOfType(BuffType type)
        {
            return GetBuffs().Any(x => x.Value.BuffType == type);
        }

        /// <summary>
        /// This is called by the AA projectile when it hits its target
        /// </summary>
        public virtual void AutoAttackHit(AttackableUnit target)
        {
            if (HasBuffOfType(BuffType.Blind) || _random.Next(0, 100) <= Stats.DodgeChance * 100)
            {
                target.TakeDamage(this, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageText.DAMAGE_TEXT_MISS);
                return;
            }

            var damage = Stats.TotalAttackDamage;
            if (_isNextAutoCrit)
            {
                damage *= Stats.TotalCriticalDamage;
            }

            var onAutoAttack = _scriptEngine.GetStaticMethod<Action<AttackableUnit, AttackableUnit>>(Model, "Passive", "OnAutoAttack");
            onAutoAttack?.Invoke(this, target);

            target.TakeDamage(this, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                DamageSource.DAMAGE_SOURCE_ATTACK,
                _isNextAutoCrit);
        }

        public virtual void UpdateAutoAttackTarget(float diff)
        {
            if (!Stats.GetActionState(ActionState.CanAttack) || Stats.GetActionState(ActionState.CanNotAttack))
            {
                SetTargetUnit(null);
                IsAttacking = false;
                _game.PacketNotifier.NotifySetTarget(this, null);
                _hasMadeInitialAttack = false;
                _autoAttackCurrentCooldown = 1.0f / Stats.TotalAttackSpeed;
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
                    _autoAttackCurrentCooldown = 1.0f / Stats.TotalAttackSpeed;
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
                    _autoAttackCurrentCooldown = 1.0f / Stats.TotalAttackSpeed;
                }
                else if (IsAttacking && AutoAttackTarget != null)
                {
                    _autoAttackCurrentDelay += diff / 1000.0f;

                    if (_autoAttackCurrentDelay >= AutoAttackDelay / (Stats.PercentAttackSpeedMod + 1))
                    {
                        if (!Stats.IsMelee)
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
                        _autoAttackCurrentCooldown = 1.0f / Stats.TotalAttackSpeed;
                        IsAttacking = false;
                    }
                }
                else if (GetDistanceTo(TargetUnit) <= Stats.TotalAttackRange)
                {
                    refreshWaypoints();
                    _isNextAutoCrit = _random.Next(0, 100) < Stats.CriticalChance * 100;
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

                        var attackType = Stats.IsMelee ? AttackType.ATTACK_TYPE_MELEE : AttackType.ATTACK_TYPE_TARGETED;
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

        public void UpdateActionStates()
        {
            Stats.ActionState = ActionState.CanAttack | ActionState.CanMove | ActionState.CanCast |
                    (Stats.ActionState & ActionState.NoRender) | (Stats.ActionState & ActionState.RevealSpecificUnit) |
                    (Stats.ActionState & ActionState.ForceRenderParticles) | (Stats.ActionState & ActionState.Unknown);

            if (IsDashing || IsCastingSpell || IsDead)
            {
                Stats.SetActionState(ActionState.CanMove, false);
            }

            foreach (var type in GetBuffs())
            {
                switch (type.Value.BuffType)
                {
                    case BuffType.Charm:
                        Stats.SetActionState(ActionState.CanAttack | ActionState.CanCast | ActionState.CanMove, false);
                        Stats.SetActionState(ActionState.CanNotAttack | ActionState.Charmed, true);
                        break;
                    case BuffType.Disarm:
                        Stats.SetActionState(ActionState.CanAttack, false);
                        Stats.SetActionState(ActionState.CanNotAttack, true);
                        break;
                    case BuffType.Fear:
                        Stats.SetActionState(ActionState.CanAttack | ActionState.CanCast | ActionState.CanMove, false);
                        Stats.SetActionState(ActionState.CanNotAttack | ActionState.Feared, true);
                        break;
                    case BuffType.Flee:
                        Stats.SetActionState(ActionState.CanAttack | ActionState.CanCast | ActionState.CanMove, false);
                        Stats.SetActionState(ActionState.CanNotAttack | ActionState.IsFleeing, true);
                        break;
                    case BuffType.Invisibility:
                        Stats.SetActionState(ActionState.Stealthed, true);
                        break;
                    case BuffType.Knockback:
                    case BuffType.Knockup:
                    case BuffType.Stun:
                    case BuffType.Suppression:
                        Stats.SetActionState(ActionState.CanAttack | ActionState.CanCast | ActionState.CanMove, false);
                        Stats.SetActionState(ActionState.CanNotAttack | ActionState.CanNotMove, true);
                        break;
                    case BuffType.NearSight:
                        Stats.SetActionState(ActionState.IsNearSighted, true);
                        break;
                    case BuffType.Polymorph:
                        Stats.SetActionState(ActionState.CanAttack | ActionState.CanCast, false);
                        Stats.SetActionState(ActionState.CanNotAttack, true);
                        break;
                    case BuffType.Silence:
                        Stats.SetActionState(ActionState.CanCast, false);
                        break;
                    case BuffType.Snare:
                        Stats.SetActionState(ActionState.CanMove, false);
                        Stats.SetActionState(ActionState.CanNotMove, true);
                        break;
                    case BuffType.Taunt:
                        Stats.SetActionState(ActionState.CanCast | ActionState.CanMove, false);
                        Stats.SetActionState(ActionState.Taunted, true);
                        break;
                }
            }
        }

        public override void update(float diff)
        {
            var onUpdate = _scriptEngine.GetStaticMethod<Action<AttackableUnit, double>>(Model, "Passive", "OnUpdate");
            onUpdate?.Invoke(this, diff);
            BuffGameScriptControllers.RemoveAll(b => b.NeedsRemoved());
            base.update(diff);
            UpdateAutoAttackTarget(diff);
            UpdateActionStates();
        }

        public override void die(ObjAIBase killer)
        {
            var onDie = _scriptEngine.GetStaticMethod<Action<AttackableUnit, AttackableUnit>>(Model, "Passive", "OnDie");
            onDie?.Invoke(this, killer);
            base.die(killer);
        }

        public override void TakeDamage(ObjAIBase attacker, float damage, DamageType type, DamageSource source, DamageText damageText)
        {
            if (Stats.IsInvulnerable && source != DamageSource.DAMAGE_SOURCE_INTERNALRAW)
            {
                damageText = DamageText.DAMAGE_TEXT_INVULNERABLE;
                _game.PacketNotifier.NotifyDamageDone(attacker, this, 0, type, damageText);
                return;
            }

            // fountain laser
            if (source == DamageSource.DAMAGE_SOURCE_INTERNALRAW)
            {
                if ((Stats.CurrentHealth -= damage) <= 0)
                {
                    Stats.CurrentHealth = 0;
                    IsDead = true;
                    die(attacker);
                }

                _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
                return;
            }

            // todo shields
            if (source == DamageSource.DAMAGE_SOURCE_RAW)
            {
                if ((Stats.CurrentHealth -= damage) <= 0)
                {
                    Stats.CurrentHealth = 0;
                    IsDead = true;
                    die(attacker);
                }

                _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
                return;
            }

            if (type == DamageType.DAMAGE_TYPE_PHYSICAL || type == DamageType.DAMAGE_TYPE_MAGICAL)
            {
                float percentPenetration;
                float flatPenetration;
                float pretendedDefense;
                if (type == DamageType.DAMAGE_TYPE_PHYSICAL)
                {
                    pretendedDefense = Stats.TotalArmor;
                    percentPenetration = attacker.Stats.PercentArmorPenetration;
                    flatPenetration = attacker.Stats.FlatArmorPenetration;
                }
                else
                {
                    pretendedDefense = Stats.TotalMagicResist;
                    percentPenetration = attacker.Stats.PercentMagicPenetration;
                    flatPenetration = attacker.Stats.FlatMagicPenetration;
                }

                if (pretendedDefense > 0)
                {
                    pretendedDefense *= percentPenetration;
                }

                if (pretendedDefense > 0)
                {
                    pretendedDefense = Math.Max(0, pretendedDefense - flatPenetration);
                }

                if (pretendedDefense >= 0)
                {
                    damage *= 100 / (100 + pretendedDefense);
                }
                else
                {
                    damage *= 2 - 100 / (100 - pretendedDefense);
                }

                if (type == DamageType.DAMAGE_TYPE_PHYSICAL)
                {
                    damage *= Stats.PhysicalDamageReductionPercent;
                }

                if (type == DamageType.DAMAGE_TYPE_MAGICAL)
                {
                    damage *= Stats.MagicalDamageReductionPercent;
                }
            }

            if ((Stats.CurrentHealth -= damage) <= 0)
            {
                Stats.CurrentHealth = 0;
                IsDead = true;
                die(attacker);
            }

            if (source == DamageSource.DAMAGE_SOURCE_ATTACK)
            {
                attacker.Stats.CurrentHealth = Math.Min(attacker.Stats.TotalHealth,
                    attacker.Stats.CurrentHealth + damage * (attacker.Stats.LifeSteal / 100));
            }

            if (source == DamageSource.DAMAGE_SOURCE_SPELL)
            {
                attacker.Stats.CurrentHealth = Math.Min(attacker.Stats.TotalHealth,
                    attacker.Stats.CurrentHealth + damage * attacker.Stats.SpellVamp);
            }

            _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
        }

        public override void Move(float diff)
        {
            if (Target == null)
            {
                _direction = new Vector2();
                return;
            }

            var to = new Vector2(Target.X, Target.Y);
            var cur = new Vector2(X, Y); //?

            var goingTo = to - cur;
            _direction = Vector2.Normalize(goingTo);
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                Console.WriteLine("X or Y is NaN");
                _direction = new Vector2(0, 0);
            }

            var moveSpeed = Stats.TotalMovementSpeed;
            if (IsDashing)
            {
                moveSpeed = _dashSpeed;
            }

            var deltaMovement = moveSpeed * 0.001f * diff;

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

            X += xx;
            Y += yy;

            // If the target was a simple point, stop when it is reached

            if (GetDistanceTo(Target) < deltaMovement * 2)
            {
                if (IsDashing)
                {
                    var animList = new List<string>();
                    _game.PacketNotifier.NotifySetAnimation(this, animList);

                    Target = null;
                }
                else if (++CurWaypoint >= Waypoints.Count)
                {
                    Target = null;
                }
                else
                {
                    Target = new Target(Waypoints[CurWaypoint]);
                }

                if (IsDashing)
                {
                    IsDashing = false;
                }
            }
        }

        public void DashToTarget(Target t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            _dashSpeed = dashSpeed;
            Target = t;
            Waypoints.Clear();
        }

        public override void onCollision(GameObject collider)
        {
            base.onCollision(collider);
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
