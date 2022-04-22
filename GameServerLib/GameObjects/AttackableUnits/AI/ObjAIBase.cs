using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Activities.Presentation.View;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    /// <summary>
    /// Base class for all moving, attackable, and attacking units.
    /// ObjAIBases normally follow these guidelines of functionality: Self movement, Inventory, Targeting, Attacking, and Spells.
    /// </summary>
    public class ObjAiBase : AttackableUnit, IObjAiBase
    {
        // Crucial Vars
        private float _autoAttackCurrentCooldown;
        private bool _skipNextAutoAttack;
        private ISpell _castingSpell;
        private Random _random = new Random();
        protected ItemManager _itemManager;
        protected AIState _aiState = AIState.AI_IDLE;
        protected bool _aiPaused;
        protected IPet _lastPetSpawned;

        /// <summary>
        /// Variable storing all the data related to this AI's current auto attack. *NOTE*: Will be deprecated as the spells system gets finished.
        /// </summary>
        public ISpell AutoAttackSpell { get; protected set; }
        /// <summary>
        /// Spell this AI is currently channeling.
        /// </summary>
        public ISpell ChannelSpell { get; protected set; }
        /// <summary>
        /// Variable containing all data about the AI's current character such as base health, base mana, whether or not they are melee, base movespeed, per level stats, etc.
        /// </summary>
        /// TODO: Move to AttackableUnit as it relates to stats.
        public ICharData CharData { get; }
        /// <summary>
        /// The ID of the skin this unit should use for its model.
        /// </summary>
        public int SkinID { get; set; }
        public bool HasAutoAttacked { get; set; }
        /// <summary>
        /// Whether or not this AI has made their first auto attack against their current target. Refreshes after untargeting or targeting another unit.
        /// </summary>
        public bool HasMadeInitialAttack { get; set; }
        /// <summary>
        /// Variable housing all variables and functions related to this AI's Inventory, ex: Items.
        /// </summary>
        /// TODO: Verify if we want to move this to AttackableUnit since items are related to stats.
        public IInventoryManager Inventory { get; protected set; }
        /// <summary>
        /// Whether or not this AI is currently auto attacking.
        /// </summary>
        public bool IsAttacking { get; private set; }
        /// <summary>
        /// Spell this unit will cast when in range of its target.
        /// Overrides auto attack spell casting.
        /// </summary>
        public ISpell SpellToCast { get; protected set; }
        /// <summary>
        /// Whether or not this AI's auto attacks apply damage to their target immediately after their cast time ends.
        /// </summary>
        public bool IsMelee { get; set; }
        public bool IsNextAutoCrit { get; protected set; }
        /// <summary>
        /// Current order this AI is performing.
        /// </summary>
        /// TODO: Rework AI so this enum can be finished.
        public OrderType MoveOrder { get; set; }
        /// <summary>
        /// Unit this AI will auto attack or use a spell on when in range.
        /// </summary>
        public IAttackableUnit TargetUnit { get; set; }
        public Dictionary<short, ISpell> Spells { get; }
        public ICharScript CharScript { get; private set; }
        public bool IsBot { get; set; }
        public IAIScript AIScript { get; protected set; }
        public ObjAiBase(Game game, string model, Stats.Stats stats, int collisionRadius = 0,
            Vector2 position = new Vector2(), int visionRadius = 0, int skinId = 0, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL, string aiScript = "") :
            base(game, model, stats, collisionRadius, position, visionRadius, netId, team)
        {
            _itemManager = game.ItemManager;

            CharData = _game.Config.ContentManager.GetCharData(Model);

            SkinID = skinId;

            stats.LoadStats(CharData);

            // TODO: Centralize this instead of letting it lay in the initialization.
            if (collisionRadius > 0)
            {
                CollisionRadius = collisionRadius;
            }
            else if (CharData.GameplayCollisionRadius > 0)
            {
                CollisionRadius = CharData.GameplayCollisionRadius;
            }
            else
            {
                CollisionRadius = 40;
            }

            if (CharData.PathfindingCollisionRadius > 0)
            {
                PathfindingRadius = CharData.PathfindingCollisionRadius;
            }
            else
            {
                PathfindingRadius = 40;
            }

            // TODO: Centralize this instead of letting it lay in the initialization.
            if (visionRadius > 0)
            {
                VisionRadius = visionRadius;
            }
            else if (CharData.PerceptionBubbleRadius > 0)
            {
                VisionRadius = CharData.PerceptionBubbleRadius;
            }
            else
            {
                VisionRadius = 1100;
            }

            Stats.CurrentMana = stats.ManaPoints.Total;
            Stats.CurrentHealth = stats.HealthPoints.Total;

            SpellToCast = null;

            Spells = new Dictionary<short, ISpell>();

            if (!string.IsNullOrEmpty(model))
            {
                IsMelee = CharData.IsMelee;

                // SpellSlots
                // 0 - 3
                for (short i = 0; i < CharData.SpellNames.Length; i++)
                {
                    if (!string.IsNullOrEmpty(CharData.SpellNames[i]))
                    {
                        Spells[i] = new Spell.Spell(game, this, CharData.SpellNames[i], (byte)i);
                    }
                }

                //If character has a passive spell, it'll initialize the CharScript with it
                if (!string.IsNullOrEmpty(CharData.PassiveData.PassiveLuaName))
                {
                    Spells[(int)SpellSlotType.PassiveSpellSlot] = new Spell.Spell(game, this, CharData.PassiveData.PassiveLuaName, (int)SpellSlotType.PassiveSpellSlot);
                }
                //If there's no passive spell, it'll just initialize the CharScript with Spell = null
                else
                {
                    LoadCharScript();
                }

                Spells[(int)SpellSlotType.SummonerSpellSlots] = new Spell.Spell(game, this, "BaseSpell", (int)SpellSlotType.SummonerSpellSlots);
                Spells[(int)SpellSlotType.SummonerSpellSlots + 1] = new Spell.Spell(game, this, "BaseSpell", (int)SpellSlotType.SummonerSpellSlots + 1);

                // InventorySlots
                // 6 - 12 (12 = TrinketSlot)
                for (byte i = (int)SpellSlotType.InventorySlots; i < (int)SpellSlotType.BluePillSlot; i++)
                {
                    Spells[i] = new Spell.Spell(game, this, "BaseSpell", i);
                }

                Spells[(int)SpellSlotType.BluePillSlot] = new Spell.Spell(game, this, "BaseSpell", (int)SpellSlotType.BluePillSlot);
                Spells[(int)SpellSlotType.TempItemSlot] = new Spell.Spell(game, this, "BaseSpell", (int)SpellSlotType.TempItemSlot);

                // RuneSlots
                // 15 - 44
                for (short i = (int)SpellSlotType.RuneSlots; i < (int)SpellSlotType.ExtraSlots; i++)
                {
                    Spells[(byte)i] = new Spell.Spell(game, this, "BaseSpell", (byte)i);
                }

                // ExtraSpells
                // 45 - 60
                for (short i = 0; i < CharData.ExtraSpells.Length; i++)
                {
                    var extraSpellName = "BaseSpell";
                    if (!string.IsNullOrEmpty(CharData.ExtraSpells[i]))
                    {
                        extraSpellName = CharData.ExtraSpells[i];
                    }

                    var slot = i + (int)SpellSlotType.ExtraSlots;
                    Spells[(byte)slot] = new Spell.Spell(game, this, extraSpellName, (byte)slot);
                    Spells[(byte)slot].LevelUp();
                }

                Spells[(int)SpellSlotType.RespawnSpellSlot] = new Spell.Spell(game, this, "BaseSpell", (int)SpellSlotType.RespawnSpellSlot);
                Spells[(int)SpellSlotType.UseSpellSlot] = new Spell.Spell(game, this, "BaseSpell", (int)SpellSlotType.UseSpellSlot);

                // BasicAttackNormalSlots & BasicAttackCriticalSlots
                // 64 - 72 & 73 - 81
                for (short i = 0; i < CharData.AttackNames.Length; i++)
                {
                    if (!string.IsNullOrEmpty(CharData.AttackNames[i]))
                    {
                        int slot = i + (int)SpellSlotType.BasicAttackNormalSlots;
                        Spells[(byte)slot] = new Spell.Spell(game, this, CharData.AttackNames[i], (byte)slot);
                    }
                }

                AutoAttackSpell = GetNewAutoAttack();
            }
            else
            {
                IsMelee = true;
            }

            AIScript = game.ScriptEngine.CreateObject<IAIScript>($"AIScripts", aiScript) ?? new EmptyAIScript();
            AIScript.OnActivate(this);
        }

        public override void OnAdded()
        {
            base.OnAdded();

            if (Spells.ContainsKey((int)SpellSlotType.PassiveSpellSlot))
            {
                CharScript.OnActivate(this, Spells[(int)SpellSlotType.PassiveSpellSlot]);
            }
            else
            {
                CharScript.OnActivate(this);
            }
        }

        /// <summary>
        /// Loads the Passive Script
        /// </summary>
        public void LoadCharScript(ISpell spell = null)
        {
            CharScript = CSharpScriptEngine.CreateObjectStatic<ICharScript>("CharScripts", $"CharScript{Model}") ?? new CharScriptEmpty();
        }

        /// <summary>
        /// Function called by this AI's auto attack projectile when it hits its target.
        /// </summary>
        public virtual void AutoAttackHit(IAttackableUnit target)
        {
            var damage = Stats.AttackDamage.Total;
            if (IsNextAutoCrit)
            {
                damage *= Stats.CriticalDamage.Total;
            }

            IDamageData damageData = new DamageData
            {
                IsAutoAttack = true,
                Attacker = this,
                Target = target,
                Damage = damage,
                PostMitigationdDamage = target.Stats.GetPostMitigationDamage(damage, DamageType.DAMAGE_TYPE_PHYSICAL, this),
                DamageSource = DamageSource.DAMAGE_SOURCE_ATTACK,
                DamageType = DamageType.DAMAGE_TYPE_PHYSICAL,
            };

            ApiEventManager.OnHitUnit.Publish(this, damageData);

            // TODO: Verify if we should use MissChance instead.
            if (HasBuffType(BuffType.BLIND))
            {
                target.TakeDamage(this, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageResultType.RESULT_MISS);
                return;
            }

            target.TakeDamage(damageData, IsNextAutoCrit);
        }

        public override bool CanMove()
        {
            return (!IsDead
                && MovementParameters != null)
                || (Status.HasFlag(StatusFlags.CanMove) && Status.HasFlag(StatusFlags.CanMoveEver)
                && (MoveOrder != OrderType.CastSpell && _castingSpell == null)
                && (ChannelSpell == null || (ChannelSpell != null && ChannelSpell.SpellData.CanMoveWhileChanneling))
                && (!IsAttacking || !AutoAttackSpell.SpellData.CantCancelWhileWindingUp)
                && !(Status.HasFlag(StatusFlags.Netted)
                || Status.HasFlag(StatusFlags.Rooted)
                || Status.HasFlag(StatusFlags.Sleep)
                || Status.HasFlag(StatusFlags.Stunned)
                || Status.HasFlag(StatusFlags.Suppressed)));
        }

        public override bool CanChangeWaypoints()
        {
            return !IsDead
                && (MovementParameters == null || (MovementParameters != null && MovementParameters.FollowNetID != 0))
                && _castingSpell == null
                && (ChannelSpell == null || (ChannelSpell != null && !ChannelSpell.SpellData.CantCancelWhileChanneling));
        }

        /// <summary>
        /// Whether or not this AI is able to auto attack.
        /// </summary>
        /// <returns></returns>
        public bool CanAttack()
        {
            // TODO: Verify if all cases are accounted for.
            return Status.HasFlag(StatusFlags.CanAttack)
                && !Status.HasFlag(StatusFlags.Charmed)
                && !Status.HasFlag(StatusFlags.Disarmed)
                && !Status.HasFlag(StatusFlags.Feared)
                // TODO: Verify
                && !Status.HasFlag(StatusFlags.Pacified)
                && !Status.HasFlag(StatusFlags.Sleep)
                && !Status.HasFlag(StatusFlags.Stunned)
                && !Status.HasFlag(StatusFlags.Suppressed)
                && _castingSpell == null
                && ChannelSpell == null;
        }

        /// <summary>
        /// Whether or not this AI is able to cast spells.
        /// </summary>
        /// <param name="spell">Spell to check.</param>
        public bool CanCast(ISpell spell = null)
        {
            // TODO: Verify if all cases are accounted for.
            return ApiEventManager.OnCanCast.Publish(this, spell)
                && Status.HasFlag(StatusFlags.CanCast)
                && !Status.HasFlag(StatusFlags.Charmed)
                && !Status.HasFlag(StatusFlags.Feared)
                // TODO: Verify what pacified is
                && !Status.HasFlag(StatusFlags.Pacified)
                && !Status.HasFlag(StatusFlags.Silenced)
                && !Status.HasFlag(StatusFlags.Sleep)
                && !Status.HasFlag(StatusFlags.Stunned)
                && !Status.HasFlag(StatusFlags.Suppressed)
                && !Status.HasFlag(StatusFlags.Taunted)
                && _castingSpell == null
                && (ChannelSpell == null || (ChannelSpell != null && !ChannelSpell.SpellData.CantCancelWhileChanneling))
                && (!IsAttacking || (IsAttacking && !AutoAttackSpell.SpellData.CantCancelWhileWindingUp));
        }

        public bool CanLevelUpSpell(ISpell s)
        {
            return CharData.SpellsUpLevels[s.CastInfo.SpellSlot][s.CastInfo.SpellLevel] <= Stats.Level;
        }

        public virtual bool LevelUp(bool force = true)
        {
            Stats.LevelUp();
            _game.PacketNotifier.NotifyNPC_LevelUp(this);
            _game.PacketNotifier.NotifyOnReplication(this, partial: false);
            ApiEventManager.OnLevelUp.Publish(this);
            return true;
        }

        /// <summary>
        /// Classifies the given unit. Used for AI attack priority, such as turrets or minions. Known in League internally as "Call for help".
        /// </summary>
        /// <param name="target">Unit to classify.</param>
        /// <returns>Classification for the given unit.</returns>
        /// TODO: Verify if we want to rename this to something which relates more to the internal League name "Call for Help".
        /// TODO: Move to AttackableUnit.
        public ClassifyUnit ClassifyTarget(IAttackableUnit target, IAttackableUnit victium = null)
        {
            if (target is IObjAiBase ai && victium != null) // If an ally is in distress, target this unit. (Priority 1~5)
            {
                switch (target)
                {
                    // Champion attacking an allied champion
                    case IChampion _ when victium is IChampion:
                        return ClassifyUnit.CHAMPION_ATTACKING_CHAMPION;
                    // Champion attacking lane minion
                    case IChampion _ when victium is ILaneMinion:
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    // Champion attacking minion
                    case IChampion _ when victium is IMinion:
                        return ClassifyUnit.CHAMPION_ATTACKING_MINION;
                    // Minion attacking an allied champion.
                    case IMinion _ when victium is IChampion:
                        return ClassifyUnit.MINION_ATTACKING_CHAMPION;
                    // Minion attacking lane minion
                    case IMinion _ when victium is ILaneMinion:
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    // Minion attacking minion
                    case IMinion _ when victium is IMinion:
                        return ClassifyUnit.MINION_ATTACKING_MINION;
                    // Turret attacking lane minion
                    case IBaseTurret _ when victium is ILaneMinion:
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                    // Turret attacking minion
                    case IBaseTurret _ when victium is IMinion:
                        return ClassifyUnit.TURRET_ATTACKING_MINION;
                }
            }

            switch (target)
            {
                case IMinion m:
                    if (m.IsLaneMinion)
                    {
                        switch ((m as ILaneMinion).MinionSpawnType)
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
                    return ClassifyUnit.MINION;
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

        public override bool Move(float diff)
        {
            // If we have waypoints, but our move order is one of these, we shouldn't move.
            if (MoveOrder == OrderType.CastSpell
                || MoveOrder == OrderType.OrderNone
                || MoveOrder == OrderType.Stop
                || MoveOrder == OrderType.Taunt)
            {
                return false;
            }

            return base.Move(diff);
        }

        /// <summary>
        /// Cancels any auto attacks this AI is performing and resets the time between the next auto attack if specified.
        /// </summary>
        /// <param name="reset">Whether or not to reset the delay between the next auto attack.</param>
        public void CancelAutoAttack(bool reset, bool fullCancel = false)
        {
            AutoAttackSpell.SetSpellState(SpellState.STATE_READY);
            if (reset)
            {
                _autoAttackCurrentCooldown = 0;
                AutoAttackSpell.ResetSpellCast();
            }

            if (fullCancel)
            {
                IsAttacking = false;
                HasMadeInitialAttack = false;
            }
            _game.PacketNotifier.NotifyNPC_InstantStop_Attack(this, false);
        }

        /// <summary>
        /// Forces this AI unit to perform a dash which follows the specified AttackableUnit.
        /// </summary>
        /// <param name="target">Unit to follow.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="leapGravity">How much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time (in seconds) the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// <param name="consideredCC">Whether or not to prevent movement, casting, or attacking during the duration of the movement.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void DashToTarget
        (
            IAttackableUnit target,
            float dashSpeed,
            string animation = "",
            float leapGravity = 0,
            bool keepFacingLastDirection = true,
            float followTargetMaxDistance = 0,
            float backDistance = 0,
            float travelTime = 0,
            bool consideredCC = true
        )
        {
            SetWaypoints(new List<Vector2> { Position, target.Position }, false);

            SetTargetUnit(target, true);

            // TODO: Take into account the rest of the arguments
            MovementParameters = new ForceMovementParameters
            {
                SetStatus = StatusFlags.None,
                ElapsedTime = 0,
                PathSpeedOverride = dashSpeed,
                ParabolicGravity = leapGravity,
                ParabolicStartPoint = Position,
                KeepFacingDirection = keepFacingLastDirection,
                FollowNetID = target.NetId,
                FollowDistance = followTargetMaxDistance,
                FollowBackDistance = backDistance,
                FollowTravelTime = travelTime
            };

            if (consideredCC)
            {
                MovementParameters.SetStatus = StatusFlags.CanAttack | StatusFlags.CanCast | StatusFlags.CanMove;
            }

            _game.PacketNotifier.NotifyWaypointGroupWithSpeed(this);

            SetDashingState(true);

            if (animation != null && animation != "")
            {
                var animPairs = new Dictionary<string, string> { { "RUN", animation } };
                SetAnimStates(animPairs);
            }

            // TODO: Verify if we want to use NotifyWaypointListWithSpeed instead as it does not require conversions.
        }

        /// <summary>
        /// Automatically paths this AI to a favorable auto attacking position. 
        /// Used only for Minions currently.
        /// </summary>
        /// <returns></returns>
        /// TODO: Move this to Minion? It isn't used anywhere else.
        /// TODO: Re-implement this for LaneMinions and add a patience or distance threshold so they don't follow forever.
        public bool RecalculateAttackPosition()
        {
            // If we are already where we should be, which means we are in attack range, then keep our current position.
            if (TargetUnit == null || TargetUnit.IsDead || Vector2.DistanceSquared(Position, TargetUnit.Position) <= Stats.Range.Total * Stats.Range.Total)
            {
                return false;
            }

            var nearestObjects = _game.Map.CollisionHandler.GetNearestObjects(new Circle(Position, DETECT_RANGE));

            foreach (var gameObject in nearestObjects)
            {
                var unit = gameObject as IAttackableUnit;
                if (unit == null ||
                    unit.NetId == NetId ||
                    unit.IsDead ||
                    Vector2.DistanceSquared(Position, TargetUnit.Position) > DETECT_RANGE * DETECT_RANGE
                )
                {
                    continue;
                }

                var closestPoint = GameServerCore.Extensions.GetClosestCircleEdgePoint(Position, gameObject.Position, gameObject.PathfindingRadius);

                // If this unit is colliding with gameObject
                if (GameServerCore.Extensions.IsVectorWithinRange(closestPoint, Position, PathfindingRadius))
                {
                    var exitPoint = GameServerCore.Extensions.GetCircleEscapePoint(Position, PathfindingRadius + 1, gameObject.Position, gameObject.PathfindingRadius);
                    SetWaypoints(new List<Vector2> { Position, exitPoint });
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Function which refreshes this AI's waypoints if they have a target.
        /// </summary>
        public virtual void RefreshWaypoints(float idealRange)
        {
            // Stop dashing to target if we reached them.
            // TODO: Implement events so we can centralize things like this.
            if (MovementParameters != null)
            {
                if (TargetUnit != null)
                {
                    if (IsCollidingWith(TargetUnit))
                    {
                        SetDashingState(false);
                    }
                    else
                    {
                        SetWaypoints(new List<Vector2> { Position, TargetUnit.Position });
                    }
                }
                else
                {
                    if (IsPathEnded())
                    {
                        SetDashingState(false);
                    }
                }

                return;
            }

            if (TargetUnit != null && _castingSpell == null && ChannelSpell == null
                && MoveOrder != OrderType.AttackTo)
            {
                UpdateMoveOrder(OrderType.AttackTo, true);
            }

            if (SpellToCast != null)
            {
                // Spell casts usually do not take into account collision radius, thus range is center -> center VS edge -> edge for attacks.
                idealRange = SpellToCast.GetCurrentCastRange();
            }

            Vector2 targetPos = Vector2.Zero;

            if (MoveOrder == OrderType.AttackTo
                && TargetUnit != null
                && !TargetUnit.IsDead)
            {
                targetPos = TargetUnit.Position;
            }

            if (MoveOrder == OrderType.AttackMove
                || MoveOrder == OrderType.AttackTerrainOnce
                || MoveOrder == OrderType.AttackTerrainSustained
                && !IsPathEnded())
            {
                targetPos = Waypoints.LastOrDefault();

                if (targetPos == Vector2.Zero)
                {
                    // Neither AttackTo nor AttackMove (etc.) were successful.
                    return;
                }
            }

            // If the target is already in range, stay where we are.
            if (MoveOrder == OrderType.AttackMove
                && targetPos != Vector2.Zero
                && MovementParameters == null
                && Vector2.DistanceSquared(Position, targetPos) <= idealRange * idealRange
                && _autoAttackCurrentCooldown <= 0)
            {
                UpdateMoveOrder(OrderType.Stop, true);
            }
            // No TargetUnit
            else if (targetPos == Vector2.Zero)
            {
                return;
            }

            if (MoveOrder == OrderType.AttackTo && targetPos != Vector2.Zero)
            {
                if (MovementParameters == null)
                {
                    if (Vector2.DistanceSquared(Position, targetPos) <= idealRange * idealRange)
                    {
                        UpdateMoveOrder(OrderType.Hold, true);
                    }
                    else
                    {
                        if (!_game.Map.PathingHandler.IsWalkable(targetPos, PathfindingRadius))
                        {
                            targetPos = _game.Map.NavigationGrid.GetClosestTerrainExit(targetPos, PathfindingRadius);
                        }

                        var newWaypoints = _game.Map.PathingHandler.GetPath(Position, targetPos);
                        if (newWaypoints != null && newWaypoints.Count > 1)
                        {
                            SetWaypoints(newWaypoints);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a random auto attack spell from the list of auto attacks available for this AI.
        /// Will only select crit auto attacks if the next auto attack is going to be a crit, otherwise normal auto attacks will be selected.
        /// </summary>
        /// <returns>Random auto attack spell.</returns>
        public ISpell GetNewAutoAttack()
        {
            if (IsNextAutoCrit)
            {
                // TODO: Verify if we want these explicitly defined instead of taken via iteration of all spells.
                var critAttackSpells = Spells.Where(s =>
                {
                    if (s.Key - 64 >= 9 && s.Key - 64 < 18)
                    {
                        if (CharData.AttackProbabilities[s.Key - 64] > 0.0f)
                        {
                            return true;
                        }
                    }
                    return false;
                });
                return critAttackSpells.ElementAt(_random.Next(0, Math.Max(0, critAttackSpells.Count() - 1))).Value;
            }
            // TODO: Verify if we want these explicitly defined instead of taken via iteration of all spells.
            var basicAttackSpells = Spells.Where(s =>
            {
                if (s.Key - 64 >= 0 && s.Key - 64 <= 9)
                {
                    if (CharData.AttackProbabilities[s.Key - 64] > 0.0f)
                    {
                        return true;
                    }
                }
                return false;
            });
            return basicAttackSpells.ElementAt(_random.Next(0, Math.Max(0, basicAttackSpells.Count() - 1))).Value;
        }

        public ISpell GetSpell(byte slot)
        {
            return Spells[slot];
        }

        public ISpell GetSpell(string name)
        {
            foreach (var s in Spells.Values)
            {
                if (s == null)
                {
                    continue;
                }

                if (s.SpellName == name)
                {
                    return s;
                }
            }

            return null;
        }

        public virtual ISpell LevelUpSpell(byte slot)
        {
            var s = Spells[slot];

            if (s == null || !CanLevelUpSpell(s))
            {
                return null;
            }

            s.LevelUp();
            ApiEventManager.OnLevelUpSpell.Publish(s);
            return s;
        }

        /// <summary>
        /// Removes the spell instance from the given slot (replaces it with an empty BaseSpell).
        /// </summary>
        /// <param name="slot">Byte slot of the spell to remove.</param>
        public void RemoveSpell(byte slot)
        {
            if (Spells[slot].CastInfo.IsAutoAttack)
            {
                return;
            }
            // Verify if we want to support removal/re-addition of character scripts.
            //Removes normal Spells
            else
            {
                Spells[slot].Deactivate();
            }
            Spells[slot] = new Spell.Spell(_game, this, "BaseSpell", slot); // Replace previous spell with empty spell.
            Stats.SetSpellEnabled(slot, false);
        }

        /// <summary>
        /// Sets this AI's current auto attack to their base auto attack.
        /// </summary>
        public void ResetAutoAttackSpell()
        {
            AutoAttackSpell = GetNewAutoAttack();
        }

        /// <summary>
        /// Sets this unit's auto attack spell that they will use when in range of their target (unless they are going to cast a spell first).
        /// </summary>
        /// <param name="spell">ISpell instance to set.</param>
        /// <param name="isReset">Whether or not setting this spell causes auto attacks to be reset (cooldown).</param>
        public void SetAutoAttackSpell(ISpell spell, bool isReset)
        {
            AutoAttackSpell = spell;
            CancelAutoAttack(isReset);
        }

        /// <summary>
        /// Sets this unit's auto attack spell that they will use when in range of their target (unless they are going to cast a spell first).
        /// </summary>
        /// <param name="name">Internal name of the spell to set.</param>
        /// <param name="isReset">Whether or not setting this spell causes auto attacks to be reset (cooldown).</param>
        /// <returns>ISpell set.</returns>
        public ISpell SetAutoAttackSpell(string name, bool isReset)
        {
            AutoAttackSpell = GetSpell(name);
            CancelAutoAttack(isReset);

            return AutoAttackSpell;
        }

        /// <summary>
        /// Forces this AI to skip its next auto attack. Usually used when spells intend to override the next auto attack with another spell.
        /// </summary>
        public void SkipNextAutoAttack()
        {
            _skipNextAutoAttack = true;
        }

        /// <summary>
        /// Sets the spell for the given slot to a new spell of the given name.
        /// </summary>
        /// <param name="name">Internal name of the spell to set.</param>
        /// <param name="slot">Slot of the spell to replace.</param>
        /// <param name="enabled">Whether or not the new spell should be enabled.</param>
        /// <param name="networkOld">Whether or not to notify clients of this change using an older packet method.</param>
        /// <returns>Newly created spell set.</returns>
        public ISpell SetSpell(string name, byte slot, bool enabled, bool networkOld = false)
        {
            if (!Spells.ContainsKey(slot) || Spells[slot].CastInfo.IsAutoAttack)
            {
                return null;
            }

            ISpell toReturn = Spells[slot];

            if (name != Spells[slot].SpellName)
            {
                // Use any existing spells before making a new one.
                bool exists = false;
                foreach (var spell in Spells.Values)
                {
                    if (spell.SpellName == name)
                    {
                        toReturn = spell;
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    toReturn = new Spell.Spell(_game, this, name, slot);

                    if (Spells[slot] != null)
                    {
                        Spells[slot].Deactivate();
                    }

                    toReturn.SetLevel(Spells[slot].CastInfo.SpellLevel);
                }

                Spells[slot] = toReturn;
                Stats.SetSpellEnabled(slot, enabled);
            }

            if (this is IChampion champion)
            {
                int userId = (int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId;
                // TODO: Verify if this is all that is needed.
                _game.PacketNotifier.NotifyChangeSlotSpellData(userId, champion, slot, ChangeSlotSpellDataType.SpellName, slot == 4 || slot == 5, newName: name);
                if (networkOld)
                {
                    _game.PacketNotifier.NotifyS2C_SetSpellData(userId, NetId, name, slot);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Sets the spell that this unit will cast when it gets in range of the spell's target.
        /// Overrides auto attack spell casting.
        /// </summary>
        /// <param name="s">Spell that will be cast.</param>
        /// <param name="location">Location to cast the spell on. May set to Vector2.Zero if unit parameter is used.</param>
        /// <param name="unit">Unit to cast the spell on.</param>
        public void SetSpellToCast(ISpell s, Vector2 location, IAttackableUnit unit = null)
        {
            SpellToCast = s;

            if (s == null)
            {
                return;
            }

            if (location != Vector2.Zero)
            {
                var exit = _game.Map.NavigationGrid.GetClosestTerrainExit(location, PathfindingRadius);
                var path = _game.Map.PathingHandler.GetPath(Position, exit);

                if (path != null)
                {
                    SetWaypoints(path);
                }
                else
                {
                    SetWaypoints(new List<Vector2> { Position, exit });
                }

                UpdateMoveOrder(OrderType.MoveTo, true);
            }

            if (unit != null)
            {
                // Unit targeted.
                SetTargetUnit(unit, true);
                UpdateMoveOrder(OrderType.AttackTo, true);
            }
            else
            {
                SetTargetUnit(null, true);
            }
        }

        /// <summary>
        /// Sets the spell that this unit is currently casting.
        /// </summary>
        /// <param name="s">Spell that is being cast.</param>
        public void SetCastSpell(ISpell s)
        {
            _castingSpell = s;
        }

        /// <summary>
        /// Gets the spell this unit is currently casting.
        /// </summary>
        /// <returns>Spell that is being cast.</returns>
        public ISpell GetCastSpell()
        {
            return _castingSpell;
        }

        /// <summary>
        /// Forces this unit to stop targeting the given unit.
        /// Applies to attacks, spell casts, spell channels, and any queued spell casts.
        /// </summary>
        /// <param name="target"></param>
        public void Untarget(IAttackableUnit target)
        {
            if (TargetUnit == target)
            {
                SetTargetUnit(null, true);
            }

            if (_castingSpell != null)
            {
                _castingSpell.RemoveTarget(target);
            }
            if (ChannelSpell != null)
            {
                ChannelSpell.RemoveTarget(target);
            }
            if (SpellToCast != null)
            {
                SpellToCast.RemoveTarget(target);
            }
        }

        /// <summary>
        /// Sets this AI's current target unit. This relates to both auto attacks as well as general spell targeting.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        public void SetTargetUnit(IAttackableUnit target, bool networked = false)
        {
            if (target == null && TargetUnit != null)
            {
                ApiEventManager.OnTargetLost.Publish(this, TargetUnit);
            }

            TargetUnit = target;

            if (networked)
            {
                _game.PacketNotifier.NotifyAI_TargetS2C(this, target);

                if (target is IChampion c)
                {
                    _game.PacketNotifier.NotifyAI_TargetHeroS2C(this, c);
                }
            }
        }

        /// <summary>
        /// Swaps the spell in the given slot1 with the spell in the given slot2.
        /// </summary>
        /// <param name="slot1">Slot of the spell to put into slot2.</param>
        /// <param name="slot2">Slot of the spell to put into slot1.</param>
        public void SwapSpells(byte slot1, byte slot2)
        {
            if (Spells[slot1].CastInfo.IsAutoAttack || Spells[slot2].CastInfo.IsAutoAttack)
            {
                return;
            }

            var slot1Name = Spells[slot1].SpellName;
            var slot2Name = Spells[slot2].SpellName;

            var enabledBuffer = Stats.GetSpellEnabled(slot1);
            var buffer = Spells[slot1];
            Spells[slot1] = Spells[slot2];

            Spells[slot2] = buffer;
            Stats.SetSpellEnabled(slot1, Stats.GetSpellEnabled(slot2));
            Stats.SetSpellEnabled(slot2, enabledBuffer);

            if (this is IChampion champion)
            {
                _game.PacketNotifier.NotifyS2C_SetSpellData((int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId, NetId, slot2Name, slot1);
                _game.PacketNotifier.NotifyS2C_SetSpellData((int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId, NetId, slot1Name, slot2);
            }
        }

        /// <summary>
        /// Sets the spell that will be channeled by this unit. Used by Spell for manual stopping and networking.
        /// </summary>
        /// <param name="spell">Spell that is being channeled.</param>
        /// <param name="network">Whether or not to send the channeling of this spell to clients.</param>
        public void SetChannelSpell(ISpell spell, bool network = true)
        {
            ChannelSpell = spell;
        }

        /// <summary>
        /// Forces this AI to stop channeling based on the given condition with the given reason.
        /// </summary>
        /// <param name="condition">Canceled or successful?</param>
        /// <param name="reason">How it should be treated.</param>
        public void StopChanneling(ChannelingStopCondition condition, ChannelingStopSource reason)
        {
            if (ChannelSpell != null)
            {
                ChannelSpell.StopChanneling(condition, reason);
                ChannelSpell = null;
            }
        }

        /// <summary>
        /// Gets the most recently spawned Pet unit which is owned by this unit.
        /// </summary>
        public IPet GetPet()
        {
            return _lastPetSpawned;
        }

        /// <summary>
        /// Sets the most recently spawned Pet unit which is owned by this unit.
        /// </summary>
        public void SetPet(IPet pet)
        {
            _lastPetSpawned = pet;
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            CharScript.OnUpdate(diff);
            if (!_aiPaused)
            {
                AIScript.OnUpdate(diff);
            }

            // bit of a hack
            foreach (var s in new List<ISpell>(Spells.Values))
            {
                s.Update(diff);
            }

            if (Inventory != null)
            {
                Inventory.OnUpdate(diff);
            }

            UpdateTarget();

            if (_autoAttackCurrentCooldown > 0)
            {
                _autoAttackCurrentCooldown -= diff / 1000.0f;
            }

            if (_lastPetSpawned != null && _lastPetSpawned.IsDead)
            {
                SetPet(null);
            }
        }

        public override void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, DamageResultType damageText)
        {
            base.TakeDamage(attacker, damage, type, source, damageText);
            OnTakeDamage(attacker);
        }
        public override void TakeDamage(IDamageData damageData, DamageResultType damageText)
        {
            base.TakeDamage(damageData, damageText);
            OnTakeDamage(damageData.Attacker);
        }
        void OnTakeDamage(IAttackableUnit attacker)
        {
            var objects = _game.ObjectManager.GetObjects();
            foreach (var it in objects)
            {
                if (it.Value is IObjAiBase u)
                {
                    float acquisitionRange = Stats.AcquisitionRange.Total;
                    float acquisitionRangeSquared = acquisitionRange * acquisitionRange;
                    if (
                        u != this
                        && !u.IsDead
                        && u.Team == Team
                        && u.AIScript.AIScriptMetaData.HandlesCallsForHelp
                        && Vector2.DistanceSquared(u.Position, Position) <= acquisitionRangeSquared
                        && Vector2.DistanceSquared(u.Position, attacker.Position) <= acquisitionRangeSquared
                    )
                    {
                        u.AIScript.OnCallForHelp(attacker, this);
                    }
                }
            }
        }

        /// <summary>
        /// Updates this AI's current target and attack actions depending on conditions such as crowd control, death state, vision, distance to target, etc.
        /// Used for both auto and spell attacks.
        /// </summary>
        private void UpdateTarget()
        {
            if (IsDead)
            {
                if (TargetUnit != null)
                {
                    CancelAutoAttack(true, true);
                    SetTargetUnit(null, true);
                }
                return;
            }
            else if (TargetUnit == null)
            {
                if ((IsAttacking && !AutoAttackSpell.SpellData.CantCancelWhileWindingUp) || HasMadeInitialAttack)
                {
                    CancelAutoAttack(!HasAutoAttacked, true);
                }
            }
            else if (TargetUnit.IsDead || (!TargetUnit.Status.HasFlag(StatusFlags.Targetable) && TargetUnit is IObjAiBase obj && !obj.CharData.IsUseable) || !TargetUnit.IsVisibleByTeam(Team))
            {
                if (IsAttacking)
                {
                    CancelAutoAttack(!HasAutoAttacked, true);
                }

                SetTargetUnit(null, true);
                return;
            }
            else if (IsAttacking)
            {
                if (Vector2.Distance(TargetUnit.Position, Position) > (Stats.Range.Total + TargetUnit.CollisionRadius)
                        && AutoAttackSpell.State == SpellState.STATE_CASTING && !AutoAttackSpell.SpellData.CantCancelWhileWindingUp)
                {
                    CancelAutoAttack(!HasAutoAttacked, true);
                }

                if (AutoAttackSpell.State == SpellState.STATE_READY)
                {
                    IsAttacking = false;
                }
                return;
            }

            // Dashes override all other actions (perhaps move this to a general check, such as IsActionable?)
            if (MovementParameters != null)
            {
                // TODO: Account for dashes which move a certain distance away from their target before stopping.
                RefreshWaypoints(0);
                return;
            }

            var idealRange = Stats.Range.Total;
            if (TargetUnit != null && SpellToCast != null && !IsAttacking && SpellToCast.SpellData.IsValidTarget(this, TargetUnit))
            {
                // Spell casts usually do not take into account collision radius, thus range is center -> center VS edge -> edge for attacks.
                idealRange = SpellToCast.GetCurrentCastRange();

                if (MoveOrder == OrderType.AttackTo
                    && TargetUnit != null
                    && Vector2.DistanceSquared(TargetUnit.Position, SpellToCast.CastInfo.Owner.Position) <= idealRange * idealRange)
                {
                    SpellToCast.Cast(new Vector2(SpellToCast.CastInfo.TargetPosition.X, SpellToCast.CastInfo.TargetPosition.Z), new Vector2(SpellToCast.CastInfo.TargetPositionEnd.X, SpellToCast.CastInfo.TargetPositionEnd.Z), TargetUnit);
                }
                else if (MoveOrder == OrderType.MoveTo
                        && Vector2.DistanceSquared(new Vector2(SpellToCast.CastInfo.TargetPosition.X, SpellToCast.CastInfo.TargetPosition.Z), SpellToCast.CastInfo.Owner.Position) <= idealRange * idealRange)
                {
                    SpellToCast.Cast(new Vector2(SpellToCast.CastInfo.TargetPosition.X, SpellToCast.CastInfo.TargetPosition.Z), new Vector2(SpellToCast.CastInfo.TargetPositionEnd.X, SpellToCast.CastInfo.TargetPositionEnd.Z));
                }
                else
                {
                    RefreshWaypoints(idealRange);
                }
            }
            else
            {
                // TODO: Verify if there are any other cases we want to avoid.
                if (TargetUnit != null && TargetUnit.Team != Team && MoveOrder != OrderType.CastSpell)
                {
                    idealRange = Stats.Range.Total + TargetUnit.CollisionRadius;

                    if (Vector2.DistanceSquared(Position, TargetUnit.Position) <= idealRange * idealRange && MovementParameters == null)
                    {
                        if (AutoAttackSpell.State == SpellState.STATE_READY)
                        {
                            // Stops us from continuing to move towards the target.
                            RefreshWaypoints(idealRange);

                            if (CanAttack())
                            {
                                IsNextAutoCrit = _random.Next(0, 100) < Stats.CriticalChance.Total * 100;
                                if (_autoAttackCurrentCooldown <= 0)
                                {
                                    HasAutoAttacked = false;
                                    AutoAttackSpell.ResetSpellCast();
                                    // TODO: ApiEventManager.OnUnitPreAttack.Publish(this);
                                    IsAttacking = true;

                                    if (AutoAttackSpell.HasEmptyScript || (AutoAttackSpell.CastInfo.SpellSlot - 64 < 9 && IsNextAutoCrit) || (AutoAttackSpell.CastInfo.SpellSlot - 64 >= 9 && !IsNextAutoCrit))
                                    {
                                        AutoAttackSpell = GetNewAutoAttack();
                                    }

                                    if (!_skipNextAutoAttack)
                                    {
                                        AutoAttackSpell.Cast(TargetUnit.Position, TargetUnit.Position, TargetUnit);

                                        _autoAttackCurrentCooldown = 1.0f / Stats.GetTotalAttackSpeed();
                                    }
                                    else
                                    {
                                        _skipNextAutoAttack = false;
                                    }
                                }
                            }
                        }
                        // Update the auto attack spell target.
                        // Units outside of range are ignored.
                        else if (IsAttacking && AutoAttackSpell.CastInfo.Targets[0].Unit != TargetUnit && !(Vector2.Distance(TargetUnit.Position, Position) > (Stats.Range.Total + TargetUnit.CollisionRadius)))
                        {
                            AutoAttackSpell.SetCurrentTarget(TargetUnit);
                        }
                    }
                    else
                    {
                        RefreshWaypoints(idealRange);
                    }
                }
                else
                {
                    // Acquires the closest target.
                    // TODO: Make a function which uses this method and use it for every case of target acquisition (ex minions, turrets, attackmove).
                    if (MoveOrder == OrderType.AttackMove)
                    {
                        if (_autoAttackCurrentCooldown > 0)
                        {
                            return;
                        }

                        var objects = _game.ObjectManager.GetObjects();
                        var distanceSqrToTarget = 25000f * 25000f;
                        IAttackableUnit nextTarget = null;
                        // Previously `Math.Max(Stats.Range.Total, Stats.AcquisitionRange.Total)` which is incorrect
                        var range = Stats.AcquisitionRange.Total;

                        foreach (var it in objects)
                        {
                            if (!(it.Value is IAttackableUnit u) ||
                                u.IsDead ||
                                u.Team == Team ||
                                Vector2.DistanceSquared(Position, u.Position) > range * range ||
                                !u.Status.HasFlag(StatusFlags.Targetable))
                            {
                                continue;
                            }

                            if (!(Vector2.DistanceSquared(Position, u.Position) < distanceSqrToTarget))
                            {
                                continue;
                            }

                            distanceSqrToTarget = Vector2.DistanceSquared(Position, u.Position);
                            nextTarget = u;
                        }

                        if (nextTarget != null)
                        {
                            SetTargetUnit(nextTarget, true);
                        }
                    }

                    if (AutoAttackSpell != null && AutoAttackSpell.State == SpellState.STATE_READY && IsAttacking)
                    {
                        IsAttacking = false;
                        HasMadeInitialAttack = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets this unit's move order to the given order.
        /// </summary>
        /// <param name="order">MoveOrder to set.</param>
        public void UpdateMoveOrder(OrderType order, bool publish = true)
        {
            if (publish)
            {
                // Return if scripts do not allow this order.
                if (!ApiEventManager.OnUnitUpdateMoveOrder.Publish(this, order))
                {
                    return;
                }
            }

            MoveOrder = order;

            if ((MoveOrder == OrderType.OrderNone
                || MoveOrder == OrderType.Stop
                || MoveOrder == OrderType.PetHardStop)
                && !IsPathEnded())
            {
                StopMovement();
                SetTargetUnit(null, true);
            }

            if (MoveOrder == OrderType.Hold
                || MoveOrder == OrderType.Taunt)
            {
                StopMovement();
            }
        }

        /// <summary>
        /// Gets the state of this unit's AI.
        /// </summary>
        public AIState GetAIState()
        {
            return _aiState;
        }

        /// <summary>
        /// Sets the state of this unit's AI.
        /// </summary>
        /// <param name="newState">State to set.</param>
        public void SetAIState(AIState newState)
        {
            _aiState = newState;
        }

        /// <summary>
        /// Whether or not this unit's AI is innactive.
        /// </summary>
        public bool IsAiPaused()
        {
            return _aiPaused;
        }

        /// <summary>
        /// Forces this unit's AI to pause/unpause.
        /// </summary>
        /// <param name="isPaused">Whether or not to pause.</param>
        public void PauseAi(bool isPaused)
        {
            _aiPaused = isPaused;
        }
    }
}
