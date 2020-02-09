using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameMaths.Geometry.Polygons;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.GameObjects.Spells;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class ObjAiBase : AttackableUnit, IObjAiBase
    {
        private IBuff[] AppliedBuffs { get; }
        private List<BuffGameScriptController> BuffGameScriptControllers { get; }
        private object BuffsLock { get; }
        private Dictionary<string, IBuff> Buffs { get; }

        private List<ICrowdControl> _crowdControlList = new List<ICrowdControl>();
        protected ItemManager _itemManager;
        protected CSharpScriptEngine _scriptEngine;

        /// <summary>
        /// Unit we want to attack as soon as in range
        /// </summary>
        public IAttackableUnit TargetUnit { get; set; }
        public IAttackableUnit AutoAttackTarget { get; set; }
        public ICharData CharData { get; }
        public ISpellData AaSpellData { get; private set; }
        private bool _isNextAutoCrit;
        public float AutoAttackCastTime { get; set; }
        public float AutoAttackProjectileSpeed { get; set; }
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        public bool IsAttacking { get; set; }
        public bool HasMadeInitialAttack { get; set; }
        public IInventoryManager Inventory { get; protected set; }
        private bool _nextAttackFlag;
        private uint _autoAttackProjId;
        public MoveOrder MoveOrder { get; set; }
        public bool IsCastingSpell { get; set; }
        public bool IsMelee { get; set; }
        public bool IsDashing { get; protected set; }

        private Random _random = new Random();

        public ObjAiBase(Game game, string model, Stats.Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(game, model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            _itemManager = game.ItemManager;
            _scriptEngine = game.ScriptEngine;

            CharData = _game.Config.ContentManager.GetCharData(Model);

            stats.LoadStats(CharData);

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
                float baseAttackCooldown = 1.6f * (1.0f + CharData.AttackDelayOffsetPercent);
                AutoAttackCastTime = baseAttackCooldown * (0.3f + CharData.AttackDelayCastOffsetPercent);
                AutoAttackProjectileSpeed = AaSpellData.MissileSpeed;
                IsMelee = CharData.IsMelee;
            }
            else
            {
                AutoAttackCastTime = 0;
                AutoAttackProjectileSpeed = 500;
                IsMelee = true;
            }

            AppliedBuffs = new IBuff[256];
            BuffGameScriptControllers = new List<BuffGameScriptController>();
            BuffsLock = new object();
            Buffs = new Dictionary<string, IBuff>();
            IsDashing = false;
        }

        public void AddBuffGameScript(string buffNamespace, string buffClass, ISpell ownerSpell, float removeAfter = -1f, bool isUnique = false)
        {
            if (isUnique)
            {
                RemoveBuffGameScriptsWithName(buffNamespace, buffClass);
            }

            var buffController =
                new BuffGameScriptController(_game, this, buffNamespace, buffClass, ownerSpell, removeAfter);
            BuffGameScriptControllers.Add(buffController);
            buffController.ActivateBuff();

            // TODO: should handle the controllers in the class, and don't pass them outside
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

        public Dictionary<string, IBuff> GetBuffs()
        {
            var toReturn = new Dictionary<string, IBuff>();
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
        public IBuff GetBuff(string name)
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

        public void AddStatModifier(IStatsModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        public void RemoveStatModifier(IStatsModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }

        public void AddBuff(IBuff b)
        {
            lock (BuffsLock)
            {
                if (!Buffs.ContainsKey(b.Name))
                {
                    Buffs.Add(b.Name, b);
                }
                else
                {
                    Buffs[b.Name].ResetDuration(); // if buff already exists, just restart its timer
                }
            }
        }

        public void RemoveBuff(IBuff b)
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

        public byte GetNewBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot();
            AppliedBuffs[slot] = b;
            return slot;
        }

        public void RemoveBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot(b);
            AppliedBuffs[slot] = null;
        }

        public void ApplyCrowdControl(ICrowdControl cc)
        {
            if (cc.IsTypeOf(CrowdControlType.STUN) || cc.IsTypeOf(CrowdControlType.ROOT))
            {
                StopMovement();
            }

            ApiEventManager.OnUnitCrowdControlled.Publish(TargetUnit);

            _crowdControlList.Add(cc);
        }

        public void RemoveCrowdControl(ICrowdControl cc)
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

        public void ChangeAutoAttackSpellData(ISpellData newAutoAttackSpellData)
        {
            AaSpellData = newAutoAttackSpellData;
        }

        public void ChangeAutoAttackSpellData(string newAutoAttackSpellDataName)
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(newAutoAttackSpellDataName);
        }

        public void ResetAutoAttackSpellData()
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(Model + "BasicAttack");
        }

        public void StopMovement()
        {
            SetWaypoints(new List<Vector2> { GetPosition(), GetPosition() });
        }

        public virtual void RefreshWaypoints()
        {
            if (TargetUnit == null || TargetUnit.IsDead || GetDistanceTo(TargetUnit) <= Stats.Range.Total && Waypoints.Count == 1)
            {
                return;
            }

            if (GetDistanceTo(TargetUnit) <= Stats.Range.Total - 2f)
            {
                SetWaypoints(new List<Vector2> { new Vector2(X, Y) });
            }
            else
            {
                SetWaypoints(new List<Vector2>() { GetPosition(), TargetUnit.GetPosition() });
                /* TODO: Soon we will use path finding for this.
                 * if(CurWaypoint >= Waypoints.Count)
                {
                    var newWaypoints = _game.Map.NavGrid.GetPath(GetPosition(), TargetUnit.GetPosition());
                    if (newWaypoints.Count > 1)
                    {
                        SetWaypoints(newWaypoints);
                    }
                }*/
            }
        }

        public ClassifyUnit ClassifyTarget(IAttackableUnit target)
        {
            if (target is IObjAiBase ai && ai.TargetUnit != null && ai.TargetUnit.IsInDistress()) // If an ally is in distress, target this unit. (Priority 1~5)
            {
                switch (target)
                {
                    // Champion attacking an allied champion
                    case IChampion _ when ai.TargetUnit is IChampion:
                        return ClassifyUnit.CHAMPION_ATTACKING_CHAMPION;
                    // Champion attacking lane minion
                    case IChampion _ when ai.TargetUnit is ILaneMinion:
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    // Champion attacking minion
                    case IChampion _ when ai.TargetUnit is IMinion:
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    // Minion attacking an allied champion.
                    case IMinion _ when ai.TargetUnit is IChampion:
                        return ClassifyUnit.MINION_ATTACKING_CHAMPION;
                    // Minion attacking lane minion
                    case IMinion _ when ai.TargetUnit is ILaneMinion:
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    // Minion attacking minion
                    case IMinion _ when ai.TargetUnit is IMinion:
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    // Turret attacking lane minion
                    case IBaseTurret _ when ai.TargetUnit is ILaneMinion:
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                    // Turret attacking minion
                    case IBaseTurret _ when ai.TargetUnit is IMinion:
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                }
            }
        
            switch (target)
            {
                case IMinion _:
                    return ClassifyUnit.MINION;
                case ILaneMinion m:
                    switch (m.MinionSpawnType)
                    {
                        case MinionSpawnType.MINION_TYPE_MELEE:
                            return ClassifyUnit.MELEE_MINION;
                        case MinionSpawnType.MINION_TYPE_CASTER:
                            return ClassifyUnit.CASTER_MINION;
                        case MinionSpawnType.MINION_TYPE_CANNON:
                        case MinionSpawnType.MINION_TYPE_SUPER:
                            return ClassifyUnit.SUPER_OR_CANNON_MINION;
                    }

                    break;
                case IBaseTurret _:
                    return ClassifyUnit.TURRET;
                case IChampion _:
                    return ClassifyUnit.CHAMPION;
                case IInhibitor _ when !target.IsDead:
                    return ClassifyUnit.INHIBITOR;
                case INexus _:
                    return ClassifyUnit.NEXUS;
            }

            return ClassifyUnit.DEFAULT;
        }

        public void SetTargetUnit(IAttackableUnit target)
        {
            TargetUnit = target;
            RefreshWaypoints();
        }

        private byte GetBuffSlot(IBuff buffToLookFor = null)
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
        public virtual void AutoAttackHit(IAttackableUnit target)
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

            var onAutoAttack = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "OnAutoAttack");
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
                    if (_autoAttackCurrentDelay >= AutoAttackCastTime / Stats.AttackSpeedMultiplier.Total)
                    {
                        if (!IsMelee)
                        {
                            var p = new Projectile(
                                _game,
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
                            _game.PacketNotifier.NotifyForceCreateMissile(p);
                        }
                        else
                        {
                            AutoAttackHit(AutoAttackTarget);
                        }
                        _autoAttackCurrentCooldown = 1.0f / Stats.GetTotalAttackSpeed();
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
            else
            {
                IsAttacking = false;
                HasMadeInitialAttack = false;
                AutoAttackTarget = null;
                _autoAttackCurrentDelay = 0;
                _game.PacketNotifier.NotifyInstantStopAttack(this, false);
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

            _crowdControlList.RemoveAll(cc => cc.IsRemoved);

            var onUpdate = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, double>>(Model, "Passive", "OnUpdate");
            onUpdate?.Invoke(this, diff);
            BuffGameScriptControllers.RemoveAll(b => b.NeedsRemoved());
            base.Update(diff);
            UpdateAutoAttackTarget(diff);
        }

        public override void Die(IAttackableUnit killer)
        {
            var onDie = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "OnDie");
            onDie?.Invoke(this, killer);
            base.Die(killer);
        }

        public override void OnCollision(IGameObject collider)
        {
            base.OnCollision(collider);
            if (collider == null)
            {
                var onCollideWithTerrain = _scriptEngine.GetStaticMethod<Action<IAttackableUnit>>(Model, "Passive", "onCollideWithTerrain");
                onCollideWithTerrain?.Invoke(this);
            }
            else
            {
                var onCollide = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "onCollide");
                onCollide?.Invoke(this, collider as IAttackableUnit);
            }
        }

        public bool RecalculateAttackPosition()
        {
            if (Target != null && TargetUnit != null && !TargetUnit.IsDead && GetDistanceTo(Target) < CollisionRadius && GetDistanceTo(TargetUnit.X, TargetUnit.Y) <= Stats.Range.Total)//If we are already where we should be, do not move.
            {
                return false;
            }
            var objects = _game.ObjectManager.GetObjects();
            List<CirclePoly> usedPositions = new List<CirclePoly>();
            var isCurrentlyOverlapping = false;

            var thisCollisionCircle = new CirclePoly(Target?.GetPosition() ?? GetPosition(), CollisionRadius + 10);

            foreach (var gameObject in objects)
            {
                var unit = gameObject.Value as IAttackableUnit;
                if (unit == null ||
                    unit.NetId == NetId ||
                    unit.IsDead ||
                    unit.Team != Team ||
                    unit.GetDistanceTo(TargetUnit) > DETECT_RANGE
                )
                {
                    continue;
                }
                var targetCollisionCircle = new CirclePoly(unit.Target?.GetPosition() ?? unit.GetPosition(), unit.CollisionRadius + 10);
                if (targetCollisionCircle.CheckForOverLaps(thisCollisionCircle))
                {
                    isCurrentlyOverlapping = true;
                }
                usedPositions.Add(targetCollisionCircle);
            }
            if (isCurrentlyOverlapping)
            {
                var targetCircle = new CirclePoly(TargetUnit.Target?.GetPosition() ?? TargetUnit.GetPosition(), Stats.Range.Total, 72);
                //Find optimal position...
                foreach (var point in targetCircle.Points.OrderBy(x => GetDistanceTo(X, Y)))
                {
                    if (!_game.Map.NavGrid.IsWalkable(point) && !_game.Map.NavGrid.IsSeeThrough(point))
                        continue;
                    var positionUsed = false;
                    foreach (var circlePoly in usedPositions)
                    {
                        if (circlePoly.CheckForOverLaps(new CirclePoly(point, CollisionRadius + 10, 20)))
                        {
                            positionUsed = true;
                        }
                    }

                    if (positionUsed)
                        continue;
                    SetWaypoints(new List<Vector2> { GetPosition(), point });
                    return true;
                }
            }
            return false;
        }

        /// <summary> TODO: Probably not the best place to have this, but still better than packet notifier </summary>
        public void TeleportTo(float x, float y)
        {
            if (!_game.Map.NavGrid.IsWalkable(x, y))
            {
                var walkableSpot = _game.Map.NavGrid.GetClosestTerrainExit(new Vector2(x, y));
                SetPosition(walkableSpot);

                x = MovementVector.TargetXToNormalFormat(_game.Map.NavGrid, walkableSpot.X);
                y = MovementVector.TargetYToNormalFormat(_game.Map.NavGrid, walkableSpot.Y);
            }
            else
            {
                SetPosition(x, y);

                x = MovementVector.TargetXToNormalFormat(_game.Map.NavGrid, x);
                y = MovementVector.TargetYToNormalFormat(_game.Map.NavGrid, y);
            }

            _game.PacketNotifier.NotifyTeleport(this, new Vector2(x, y));
        }

        public void UpdateTargetUnit(IAttackableUnit unit)
        {
            TargetUnit = unit;
        }

        public void DashToTarget(ITarget t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            Target = t;
            Waypoints.Clear();
        }

        public void SetDashingState(bool state)
        {
            IsDashing = state;
        }
    }
}
