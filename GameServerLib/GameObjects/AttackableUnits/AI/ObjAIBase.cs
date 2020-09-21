using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameMaths.Geometry.Polygons;
using GameServerCore;
using GameServerCore.Content;
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
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        private uint _autoAttackProjId;
        private object _buffsLock;
        private List<ICrowdControl> _crowdControlList = new List<ICrowdControl>();
        private bool _isNextAutoCrit;
        protected ItemManager _itemManager;
        private bool _nextAttackFlag;
        private Random _random = new Random();
        protected CSharpScriptEngine _scriptEngine;
        private float _dashElapsedTime;
        private float _dashTime;

        public ISpellData AaSpellData { get; private set; }
        public float AutoAttackCastTime { get; set; }
        public float AutoAttackProjectileSpeed { get; set; }
        public IAttackableUnit AutoAttackTarget { get; set; }
        private IBuff[] BuffSlots { get; }
        private Dictionary<string, IBuff> Buffs { get; }
        private List<IBuff> BuffList { get; }
        public ICharData CharData { get; }
        public float DashSpeed { get; set; }
        public bool HasMadeInitialAttack { get; set; }
        public IInventoryManager Inventory { get; protected set; }
        public bool IsAttacking { get; set; }
        public bool IsCastingSpell { get; set; }
        public bool IsDashing { get; protected set; }
        public bool IsMelee { get; set; }
        public MoveOrder MoveOrder { get; set; }
        /// <summary>
        /// Unit we want to attack as soon as in range
        /// </summary>
        public IAttackableUnit TargetUnit { get; set; }

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

            BuffSlots = new IBuff[256];
            BuffList = new List<IBuff>();
            _buffsLock = new object();
            Buffs = new Dictionary<string, IBuff>();
            IsDashing = false;
        }

        public void AddBuff(IBuff b)
        {
            lock (_buffsLock)
            {
                if (!Buffs.ContainsKey(b.Name))
                {
                    if (HasBuff(b.Name))
                    {
                        var buff = GetBuffsWithName(b.Name)[0];
                        Buffs.Add(b.Name, buff);
                        return;
                    }
                    Buffs.Add(b.Name, b);
                    BuffList.Add(b);
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffAdd2(b);
                    }
                    b.ActivateBuff();
                }
                else if (b.BuffAddType == BuffAddType.REPLACE_EXISTING)
                {
                    var prevbuff = Buffs[b.Name];

                    prevbuff.DeactivateBuff();
                    RemoveBuff(b.Name);
                    BuffList.Remove(prevbuff);
                    RemoveBuffSlot(b);

                    BuffSlots[prevbuff.Slot] = b;
                    b.SetSlot(prevbuff.Slot);

                    Buffs.Add(b.Name, b);
                    BuffList.Add(b);

                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffReplace(b);
                    }
                    b.ActivateBuff();
                }
                else if (b.BuffAddType == BuffAddType.RENEW_EXISTING)
                {
                    Buffs[b.Name].ResetTimeElapsed();

                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffReplace(Buffs[b.Name]);
                    }
                    RemoveStatModifier(Buffs[b.Name].GetStatsModifier()); // TODO: Replace with a better method that unloads -> reloads all data of a script
                    Buffs[b.Name].ActivateBuff();
                }
                else if (b.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS)
                {
                    if (Buffs[b.Name].StackCount >= Buffs[b.Name].MaxStacks)
                    {
                        var tempbuffs = GetBuffsWithName(b.Name);
                        var oldestbuff = tempbuffs[0];

                        oldestbuff.DeactivateBuff();
                        RemoveBuff(b.Name);
                        BuffList.Remove(oldestbuff);
                        RemoveBuffSlot(oldestbuff);

                        tempbuffs = GetBuffsWithName(b.Name);

                        BuffSlots[oldestbuff.Slot] = tempbuffs[0];
                        Buffs.Add(oldestbuff.Name, tempbuffs[0]);
                        BuffList.Add(b);

                        if (!b.IsHidden)
                        {
                            if (Buffs[b.Name].BuffType == BuffType.COUNTER)
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Buffs[b.Name]);
                            }
                            else
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCount(b, b.Duration, b.TimeElapsed);
                            }
                        }
                        b.ActivateBuff();

                        return;
                    }
                    BuffList.Add(b);

                    Buffs[b.Name].IncrementStackCount();

                    GetBuffsWithName(b.Name).ForEach(buff => buff.SetStacks(Buffs[b.Name].StackCount));

                    if (!b.IsHidden)
                    {
                        if (b.BuffType == BuffType.COUNTER)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Buffs[b.Name]);
                        }
                        else
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCount(b, b.Duration, b.TimeElapsed);
                        }
                    }
                    b.ActivateBuff();
                }
                else if (Buffs[b.Name].BuffAddType == BuffAddType.STACKS_AND_RENEWS)
                {
                    RemoveBuffSlot(b);

                    Buffs[b.Name].ResetTimeElapsed();
                    Buffs[b.Name].IncrementStackCount();

                    if (!b.IsHidden)
                    {
                        if (Buffs[b.Name].BuffType == BuffType.COUNTER)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Buffs[b.Name]);
                        }
                        else
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateCount(Buffs[b.Name], Buffs[b.Name].Duration, Buffs[b.Name].TimeElapsed);
                        }
                    }

                    RemoveStatModifier(Buffs[b.Name].GetStatsModifier()); // TODO: Replace with a better method that unloads -> reloads all data of a script
                    Buffs[b.Name].ActivateBuff();
                }
            }
        }

        public void AddStatModifier(IStatsModifier statModifier)
        {
            Stats.AddModifier(statModifier);
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

        public void ChangeAutoAttackSpellData(ISpellData newAutoAttackSpellData)
        {
            AaSpellData = newAutoAttackSpellData;
        }

        public void ChangeAutoAttackSpellData(string newAutoAttackSpellDataName)
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(newAutoAttackSpellDataName);
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

        public void ClearAllCrowdControl()
        {
            _crowdControlList.Clear();
        }

        public void DashToTarget(ITarget t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            Target = t;
            _dashTime = this.GetDistanceTo(t)/(dashSpeed*0.001f);
            _dashElapsedTime = 0;
            DashSpeed = dashSpeed;
            Waypoints.Clear();
        }

        public override void Die(IAttackableUnit killer)
        {
            var onDie = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "OnDie");
            onDie?.Invoke(this, killer);
            base.Die(killer);
        }

        public IBuff GetBuffWithName(string name)
        {
            lock (_buffsLock)
            {
                if (Buffs.ContainsKey(name))
                {
                    return Buffs[name];
                }
                return null;
            }
        }

        private byte GetBuffSlot(IBuff buffToLookFor = null)
        {
            for (byte i = 1; i < BuffSlots.Length; i++) // Find the first open slot or the slot corresponding to buff
            {
                if (BuffSlots[i] == buffToLookFor)
                {
                    return i;
                }
            }

            throw new Exception("No slot found with requested value"); // If no open slot or no corresponding slot
        }

        public List<IBuff> GetBuffs()
        {
            return BuffList;
        }

        public int GetBuffsCount()
        {
            return Buffs.Count;
        }

        public List<IBuff> GetBuffsWithName(string buffName)
        {
            lock (_buffsLock)
            {
                return BuffList.FindAll(b => b.IsBuffSame(buffName));
            }
        }

        public override float GetMoveSpeed()
        {
            return IsDashing ? DashSpeed : base.GetMoveSpeed();
        }

        public byte GetNewBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot();
            BuffSlots[slot] = b;
            return slot;
        }

        public uint GetObjHash()
        {
            var gobj = "[Character]" + Model;

            // TODO: Account for any other units that have skins (requires skins to be implemented for those units)
            if (this is IChampion c)
            {
                var szSkin = "";
                if (c.Skin < 10)
                {
                    szSkin = "0" + c.Skin;
                }
                else
                {
                    szSkin = c.Skin.ToString();
                }
                gobj += szSkin;
            }

            return HashFunctions.HashStringNorm(gobj);
        }

        public bool HasBuff(IBuff buff)
        {
            return !(BuffList.Find(b => b == buff) == null);
        }

        public bool HasBuff(string buffName)
        {
            return !(BuffList.Find(b => b.IsBuffSame(buffName)) == null);
        }

        public bool HasCrowdControl(CrowdControlType ccType)
        {
            return _crowdControlList.FirstOrDefault(cc => cc.IsTypeOf(ccType)) != null;
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
            // If we are already where we should be, which means we are in attack range and not colliding, then keep our current position.
            if (Target != null && TargetUnit != null && !TargetUnit.IsDead && GetDistanceTo(Target) > CollisionRadius && GetDistanceTo(TargetUnit.X, TargetUnit.Y) <= Stats.Range.Total)
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
                    if (!_game.Map.NavigationGrid.IsWalkable(point) && !_game.Map.NavigationGrid.IsSeeThrough(point))
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

        public void RemoveBuff(IBuff b)
        {
            lock (_buffsLock)
            {
                if (b.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS && b.StackCount > 1)
                {
                    b.DecrementStackCount();

                    RemoveBuff(b.Name);
                    BuffList.Remove(b);
                    RemoveBuffSlot(b);

                    var tempbuffs = GetBuffsWithName(b.Name);

                    tempbuffs.ForEach(tempbuff => tempbuff.SetStacks(b.StackCount));

                    BuffSlots[b.Slot] = tempbuffs[0];
                    Buffs.Add(b.Name, tempbuffs[0]);

                    var newestBuff = tempbuffs[tempbuffs.Count - 1];

                    if (!b.IsHidden)
                    {
                        if (b.BuffType == BuffType.COUNTER)
                        {
                            _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Buffs[b.Name]);
                        }
                        else
                        {
                            if (b.StackCount == 1)
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCount(newestBuff, b.Duration - newestBuff.TimeElapsed, newestBuff.TimeElapsed);
                            }
                            else
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCountGroup(this, tempbuffs, b.Duration - newestBuff.TimeElapsed, newestBuff.TimeElapsed);
                            }
                        }
                    }
                }
                else
                {
                    BuffList.RemoveAll(buff => buff.Elapsed());
                    RemoveBuff(b.Name);
                    RemoveBuffSlot(b);
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffRemove2(b);
                    }
                }
            }
        }

        public void RemoveBuff(string b)
        {
            lock (_buffsLock)
            {
                Buffs.Remove(b);
            }
        }

        public void RemoveBuffsWithName(string buffName)
        {
            lock (_buffsLock)
            {
                BuffList.FindAll(b => 
                b.IsBuffSame(buffName)).ForEach(b => 
                b.DeactivateBuff());
            }
        }

        public void RemoveBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot(b);
            BuffSlots[slot] = null;
        }

        public void RemoveCrowdControl(ICrowdControl cc)
        {
            _crowdControlList.Remove(cc);
        }

        public void RemoveStatModifier(IStatsModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }

        public void ResetAutoAttackSpellData()
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(Model + "BasicAttack");
        }

        public void SetDashingState(bool state)
        {
            IsDashing = state;
        }

        public void SetTargetUnit(IAttackableUnit target)
        {
            TargetUnit = target;
            RefreshWaypoints();
        }

        public void StopMovement()
        {
            SetWaypoints(new List<Vector2> { GetPosition(), GetPosition() });
        }

        /// <summary> TODO: Probably not the best place to have this, but still better than packet notifier </summary>
        public void TeleportTo(float x, float y)
        {
            if (!_game.Map.NavigationGrid.IsWalkable(x, y))
            {
                var walkableSpot = _game.Map.NavigationGrid.GetClosestTerrainExit(new Vector2(x, y));
                SetPosition(walkableSpot);

                x = MovementVector.TargetXToNormalFormat(_game.Map.NavigationGrid, walkableSpot.X);
                y = MovementVector.TargetYToNormalFormat(_game.Map.NavigationGrid, walkableSpot.Y);
            }
            else
            {
                SetPosition(x, y);

                x = MovementVector.TargetXToNormalFormat(_game.Map.NavigationGrid, x);
                y = MovementVector.TargetYToNormalFormat(_game.Map.NavigationGrid, y);
            }
            
            _game.PacketNotifier.NotifyTeleport(this, new Vector2(x, y));
        }

        public override void Update(float diff)
        {
            foreach (var cc in _crowdControlList)
            {
                cc.Update(diff);
            }

            if (IsDashing)
            {
                _dashElapsedTime += diff;
                if (_dashElapsedTime >= _dashTime)
                {
                    IsDashing = false;
                    var animList = new List<string> { "RUN" };
                    _game.PacketNotifier.NotifySetAnimation(this, animList);
                }
            }

            _crowdControlList.RemoveAll(cc => cc.IsRemoved);

            var onUpdate = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, double>>(Model, "Passive", "OnUpdate");
            onUpdate?.Invoke(this, diff);
            base.Update(diff);
            UpdateAutoAttackTarget(diff);
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
                _game.PacketNotifier.NotifyNPC_InstantStopAttack(this, false);
            }

            if (_autoAttackCurrentCooldown > 0)
            {
                _autoAttackCurrentCooldown -= diff / 1000.0f;
            }
        }

        public void UpdateTargetUnit(IAttackableUnit unit)
        {
            TargetUnit = unit;
        }
    }
}
