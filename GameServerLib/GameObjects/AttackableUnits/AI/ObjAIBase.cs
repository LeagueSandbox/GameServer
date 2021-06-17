using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameMaths.Geometry.Polygons;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.Items;

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
        protected ItemManager _itemManager;
        private Random _random = new Random();

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
        public bool IsAttacking { get; set; }
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

        public ObjAiBase(Game game, string model, Stats.Stats stats, int collisionRadius = 40,
            Vector2 position = new Vector2(), int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL) :
            base(game, model, stats, collisionRadius, position, visionRadius, netId, team)
        {
            _itemManager = game.ItemManager;

            CharData = _game.Config.ContentManager.GetCharData(Model);

            stats.LoadStats(CharData);

            // TODO: Centralize this instead of letting it lay in the initialization.
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

                var passiveSpellName = "BaseSpell";
                if (!string.IsNullOrEmpty(CharData.Passive.PassiveAbilityName))
                {
                    passiveSpellName = CharData.Passive.PassiveAbilityName;
                }

                Spells[(int)SpellSlotType.PassiveSpellSlot] = new Spell.Spell(game, this, passiveSpellName, (int)SpellSlotType.PassiveSpellSlot);

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
        }

        /// <summary>
        /// Function called by this AI's auto attack projectile when it hits its target.
        /// </summary>
        public virtual void AutoAttackHit(IAttackableUnit target)
        {
            ApiEventManager.OnHitUnit.Publish(this, target, IsNextAutoCrit);

            // TODO: Verify if we should instead use MissChance.
            if (HasBuffType(BuffType.BLIND))
            {
                target.TakeDamage(this, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageResultType.RESULT_MISS);
                return;
            }

            var damage = Stats.AttackDamage.Total;
            if (IsNextAutoCrit)
            {
                damage *= Stats.CriticalDamage.Total;
            }

            target.TakeDamage(this, damage, DamageType.DAMAGE_TYPE_PHYSICAL,
                DamageSource.DAMAGE_SOURCE_ATTACK,
                IsNextAutoCrit);
        }

        public override bool CanMove()
        {
            // TODO: Verify if Dashes should bypass this.
            return !(IsDead || MoveOrder == OrderType.CastSpell
                    || !(Status.HasFlag(StatusFlags.CanMove) || Status.HasFlag(StatusFlags.CanMoveEver))
                    || Status.HasFlag(StatusFlags.Immovable)
                    || Status.HasFlag(StatusFlags.Netted)
                    || Status.HasFlag(StatusFlags.Rooted)
                    || Status.HasFlag(StatusFlags.Sleep)
                    || Status.HasFlag(StatusFlags.Stunned)
                    || Status.HasFlag(StatusFlags.Suppressed));
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
                && !Status.HasFlag(StatusFlags.Suppressed);
        }

        /// <summary>
        /// Whether or not this AI is able to cast spells.
        /// </summary>
        public bool CanCast()
        {
            // TODO: Verify if all cases are accounted for.
            return Status.HasFlag(StatusFlags.CanCast)
                && !Status.HasFlag(StatusFlags.Charmed)
                && !Status.HasFlag(StatusFlags.Feared)
                // TODO: Verify
                && !Status.HasFlag(StatusFlags.Pacified)
                && !Status.HasFlag(StatusFlags.Silenced)
                && !Status.HasFlag(StatusFlags.Sleep)
                && !Status.HasFlag(StatusFlags.Stunned)
                && !Status.HasFlag(StatusFlags.Suppressed)
                && !Status.HasFlag(StatusFlags.Taunted);
        }

        public bool CanLevelUpSpell(ISpell s)
        {
            return CharData.SpellsUpLevels[s.CastInfo.SpellSlot][s.CastInfo.SpellLevel] <= Stats.Level;
        }

        /// <summary>
        /// Classifies the given unit. Used for AI attack priority, such as turrets or minions. Known in League internally as "Call for help".
        /// </summary>
        /// <param name="target">Unit to classify.</param>
        /// <returns>Classification for the given unit.</returns>
        /// TODO: Verify if we want to rename this to something which relates more to the internal League name "Call for Help".
        /// TODO: Move to AttackableUnit.
        public ClassifyUnit ClassifyTarget(IAttackableUnit target)
        {
            if (target is IObjAiBase ai && ai.TargetUnit != null && (ai.TargetUnit.Team == Team && ai.TargetUnit.IsInDistress())) // If an ally is in distress, target this unit. (Priority 1~5)
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
        public void CancelAutoAttack(bool reset)
        {
            AutoAttackSpell.SetSpellState(SpellState.STATE_READY);
            if (reset)
            {
                _autoAttackCurrentCooldown = 0;
                AutoAttackSpell.ResetSpellDelay();
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
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void DashToTarget
        (
            IAttackableUnit target,
            float dashSpeed,
            string animation,
            float leapGravity,
            bool keepFacingLastDirection,
            float followTargetMaxDistance,
            float backDistance,
            float travelTime
        )
        {
            SetWaypoints(new List<Vector2> { Position, target.Position }, false);

            SetTargetUnit(target, true);

            // TODO: Take into account the rest of the arguments
            MovementParameters = new ForceMovementParameters
            {
                PathSpeedOverride = dashSpeed,
                ParabolicGravity = leapGravity,
                ParabolicStartPoint = Position,
                KeepFacingDirection = keepFacingLastDirection,
                FollowNetID = target.NetId,
                FollowDistance = followTargetMaxDistance,
                FollowBackDistance = backDistance,
                FollowTravelTime = travelTime
            };
            DashElapsedTime = 0;

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

            var nearestObjects = _game.Map.CollisionHandler.QuadDynamic.GetNearestObjects(this);

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

                var closestPoint = GameServerCore.Extensions.GetClosestCircleEdgePoint(Position, gameObject.Position, gameObject.CollisionRadius);

                // If this unit is colliding with gameObject
                if (GameServerCore.Extensions.IsVectorWithinRange(closestPoint, Position, CollisionRadius))
                {
                    var exitPoint = GameServerCore.Extensions.GetCircleEscapePoint(Position, CollisionRadius + 1, gameObject.Position, gameObject.CollisionRadius);
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

                return;
            }

            if (MoveOrder == OrderType.AttackMove
                || MoveOrder == OrderType.AttackTo
                || MoveOrder == OrderType.AttackTerrainOnce
                || MoveOrder == OrderType.AttackTerrainSustained)
            {
                idealRange = Stats.Range.Total;
            }

            if (MoveOrder != OrderType.AttackTo && TargetUnit != null)
            {
                UpdateMoveOrder(OrderType.AttackTo, true);
                idealRange = Stats.Range.Total;
            }

            if (SpellToCast != null)
            {
                idealRange = SpellToCast.GetCurrentCastRange();
            }

            Vector2 targetPos = Vector2.Zero;

            if ((MoveOrder == OrderType.AttackTo)
                && TargetUnit != null)
            {
                if (!TargetUnit.IsDead)
                {
                    targetPos = TargetUnit.Position;
                }
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
            if (MoveOrder == OrderType.AttackMove && targetPos != Vector2.Zero)
            {
                if (MovementParameters == null && Vector2.DistanceSquared(Position, targetPos) <= idealRange * idealRange)
                {
                    UpdateMoveOrder(OrderType.Stop, true);
                }
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
                        UpdateMoveOrder(OrderType.Stop, true);
                    }
                    else
                    {
                        if (!_game.Map.NavigationGrid.IsWalkable(targetPos, CollisionRadius))
                        {
                            targetPos = _game.Map.NavigationGrid.GetClosestTerrainExit(targetPos, CollisionRadius);
                        }

                        var newWaypoints = _game.Map.NavigationGrid.GetPath(Position, targetPos);
                        if (newWaypoints.Count > 1)
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
                if (s.Key - 64 >= 0 && s.Key - 64 < 9)
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
            if (isReset)
            {
                _autoAttackCurrentCooldown = 0;
                AutoAttackSpell.SetSpellState(SpellState.STATE_READY);
            }
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
            if (isReset)
            {
                _autoAttackCurrentCooldown = 0;
                AutoAttackSpell.SetSpellState(SpellState.STATE_READY);
            }
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
        /// <returns>Newly created spell set.</returns>
        public ISpell SetSpell(string name, byte slot, bool enabled)
        {
            if (!Spells.ContainsKey(slot) || Spells[slot].CastInfo.IsAutoAttack)
            {
                return null;
            }

            ISpell newSpell = new Spell.Spell(_game, this, name, slot);

            newSpell.SetLevel(Spells[slot].CastInfo.SpellLevel);

            Spells[slot] = newSpell;
            Stats.SetSpellEnabled(slot, enabled);

            if (this is IChampion champion)
            {
                int userId = (int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId;
                _game.PacketNotifier.NotifyS2C_SetSpellData(userId, NetId, name, slot);
            }

            return newSpell;
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
                var exit = _game.Map.NavigationGrid.GetClosestTerrainExit(location, CollisionRadius);
                var path = _game.Map.NavigationGrid.GetPath(Position, exit);

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
        /// Sets this AI's current target unit. This relates to both auto attacks as well as general spell targeting.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        /// TODO: Remove Target class.
        public void SetTargetUnit(IAttackableUnit target, bool networked = false)
        {
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

        public override void Update(float diff)
        {
            base.Update(diff);

            foreach (var s in Spells.Values)
            {
                s.Update(diff);
            }

            if (CanMove())
            {
                UpdateAttackTarget(diff);
            }
        }

        /// <summary>
        /// Updates this AI's current target and attack actions depending on conditions such as crowd control, death state, vision, distance to target, etc.
        /// Used for both auto and spell attacks.
        /// </summary>
        /// <param name="diff">Number of milliseconds that passed before this tick occurred.</param>
        private void UpdateAttackTarget(float diff)
        {
            if (IsDead)
            {
                if (TargetUnit != null)
                {
                    SetTargetUnit(null, true);
                    IsAttacking = false;
                    HasMadeInitialAttack = false;
                }
                return;
            }

            if (MovementParameters != null)
            {
                RefreshWaypoints(0);
                return;
            }

            var idealRange = Stats.Range.Total;

            if (TargetUnit is IObjBuilding)
            {
                idealRange = Stats.Range.Total + TargetUnit.CollisionRadius;
            }

            if (SpellToCast != null && !IsAttacking)
            {
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
                if (TargetUnit != null && MoveOrder != OrderType.CastSpell)
                {
                    // TODO: Implement True Sight for turrets instead of using an exception for turret targeting in relation to vision here.
                    if (TargetUnit.IsDead || (!_game.ObjectManager.TeamHasVisionOn(Team, TargetUnit) && !(this is IBaseTurret) && !(TargetUnit is IBaseTurret) && !(TargetUnit is IObjBuilding) && MovementParameters == null))
                    {
                        SetTargetUnit(null);
                        IsAttacking = false;
                        HasMadeInitialAttack = false;
                    }
                    else if (IsAttacking && MovementParameters == null)
                    {
                        if (AutoAttackSpell.HasEmptyScript || (AutoAttackSpell.CastInfo.SpellSlot - 64 < 9 && IsNextAutoCrit) || (AutoAttackSpell.CastInfo.SpellSlot - 64 >= 9 && !IsNextAutoCrit))
                        {
                            AutoAttackSpell = GetNewAutoAttack();
                        }

                        if (AutoAttackSpell.State == SpellState.STATE_READY)
                        {
                            ApiEventManager.OnPreAttack.Publish(this, AutoAttackSpell);

                            if (!_skipNextAutoAttack)
                            {
                                AutoAttackSpell.Cast(TargetUnit.Position, TargetUnit.Position, TargetUnit);

                                _autoAttackCurrentCooldown = 1.0f / Stats.GetTotalAttackSpeed();
                            }
                            else
                            {
                                _skipNextAutoAttack = false;
                            }

                            IsAttacking = false;
                        }
                    }
                    else if (Vector2.DistanceSquared(Position, TargetUnit.Position) <= idealRange * idealRange && MovementParameters == null)
                    {
                        if (AutoAttackSpell.State == SpellState.STATE_READY)
                        {
                            // Stops us from continuing to move towards the target.
                            RefreshWaypoints(Stats.Range.Total);

                            if (CanAttack())
                            {
                                IsNextAutoCrit = _random.Next(0, 100) < Stats.CriticalChance.Total * 100;
                                if (_autoAttackCurrentCooldown <= 0)
                                {
                                    HasAutoAttacked = false;
                                    AutoAttackSpell.ResetSpellDelay();
                                    // TODO: ApiEventManager.OnUnitPreAttack.Publish(this);
                                    IsAttacking = true;
                                }
                            }
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
                    if (MoveOrder == OrderType.AttackMove)
                    {
                        var objects = _game.ObjectManager.GetObjects();
                        var distanceSqrToTarget = 25000f * 25000f;
                        IAttackableUnit nextTarget = null;
                        var range = Math.Max(Stats.Range.Total, DETECT_RANGE);

                        foreach (var it in objects)
                        {
                            if (!(it.Value is IAttackableUnit u) ||
                                u.IsDead ||
                                u.Team == Team ||
                                Vector2.DistanceSquared(Position, u.Position) > range * range)
                                continue;

                            if (!(Vector2.DistanceSquared(Position, u.Position) < distanceSqrToTarget))
                                continue;
                            distanceSqrToTarget = Vector2.DistanceSquared(Position, u.Position);
                            nextTarget = u;
                        }

                        if (nextTarget != null)
                        {
                            SetTargetUnit(nextTarget, true);
                        }
                    }

                    IsAttacking = false;

                    if (AutoAttackSpell != null && AutoAttackSpell.State != SpellState.STATE_READY)
                    {
                        if (!HasAutoAttacked)
                        {
                            _autoAttackCurrentCooldown = 0;
                        }

                        AutoAttackSpell.SetSpellState(SpellState.STATE_READY);
                        AutoAttackSpell.ResetSpellDelay();
                        _game.PacketNotifier.NotifyNPC_InstantStop_Attack(this, false);
                    }

                    HasMadeInitialAttack = false;
                }

                if (_autoAttackCurrentCooldown > 0)
                {
                    _autoAttackCurrentCooldown -= diff / 1000.0f;
                }
            }
        }

        /// <summary>
        /// Sets this unit's move order to the given order.
        /// </summary>
        /// <param name="order">MoveOrder to set.</param>
        public void UpdateMoveOrder(OrderType order, bool publish = true)
        {
            MoveOrder = order;

            if ((MoveOrder == OrderType.OrderNone
                || MoveOrder == OrderType.Taunt
                || MoveOrder == OrderType.Stop)
                && !IsPathEnded())
            {
                StopMovement();
            }

            if (publish)
            {
                ApiEventManager.OnUnitUpdateMoveOrder.Publish(this, order);
            }
        }
    }
}
