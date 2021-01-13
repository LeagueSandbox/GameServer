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
    /// <summary>
    /// Base class for all moving, attackable, and attacking units.
    /// ObjAIBases normally follow these guidelines of functionality: Movement, Crowd Control, Inventory, Targeting, Attacking, and Spells.
    /// </summary>
    public class ObjAiBase : AttackableUnit, IObjAiBase
    {
        // Crucial Vars
        private float _autoAttackCurrentCooldown;
        private float _autoAttackCurrentDelay;
        private uint _autoAttackProjId;
        private object _buffsLock;
        private List<ICrowdControl> _crowdControlList = new List<ICrowdControl>();
        private float _dashElapsedTime;
        private float _dashTime;
        private bool _isNextAutoCrit;
        protected ItemManager _itemManager;
        private bool _nextAttackFlag;
        private Random _random = new Random();
        // Move to AttackableUnit and make it a public variable so this class can access it.
        protected CSharpScriptEngine _scriptEngine;

        /// <summary>
        /// Variable storing all the data related to this AI's current auto attack. *NOTE*: Will be deprecated as the spells system gets finished.
        /// </summary>
        public ISpellData AaSpellData { get; private set; }
        /// <summary>
        /// Variable for the cast time of this AI's current auto attack.
        /// </summary>
        public float AutoAttackCastTime { get; set; }
        /// <summary>
        /// Variable for the projectile speed of this AI's current auto attack projectile.
        /// </summary>
        public float AutoAttackProjectileSpeed { get; set; }
        /// <summary>
        /// This AI's current auto attack target. Null if no target.
        /// </summary>
        public IAttackableUnit AutoAttackTarget { get; set; }
        /// <summary>
        /// Array of buff slots which contains all parent buffs (oldest buff of a given name) applied to this AI.
        /// Maximum of 256 slots, hard limit due to packets.
        /// </summary>
        /// TODO: Move to AttackableUnit.
        private IBuff[] BuffSlots { get; }
        /// <summary>
        /// Dictionary containing all parent buffs (oldest buff of a given name). Used for packets and assigning stacks if a buff of the same name is added.
        /// </summary>
        /// TODO: Move to AttackableUnit.
        private Dictionary<string, IBuff> Buffs { get; }
        /// <summary>
        /// List of all buffs applied to this AI. Used for easier indexing of buffs.
        /// </summary>
        /// TODO: Verify if we can remove this in favor of BuffSlots while keeping the functions which allow for easy accessing of individual buff instances.
        /// TODO: Move to AttackableUnit.
        private List<IBuff> BuffList { get; }
        /// <summary>
        /// Variable containing all data about the AI's current character such as base health, base mana, whether or not they are melee, base movespeed, per level stats, etc.
        /// </summary>
        /// TODO: Move to AttackableUnit as it relates to stats.
        public ICharData CharData { get; }
        /// <summary>
        /// Speed of the AI's current dash.
        /// </summary>
        /// TODO: Implement a dash class so these things can be separate from AI (however, dashes should only be applicable to ObjAIBase).
        public float DashSpeed { get; set; }
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
        /// Whether or not this AI is currently casting a spell. *NOTE*: Not to be confused with channeling (which isn't implemented yet).
        /// </summary>
        public bool IsCastingSpell { get; set; }
        /// <summary>
        /// Whether or not this AI is currently dashing.
        /// </summary>
        public bool IsDashing { get; protected set; }
        /// <summary>
        /// Whether or not this AI's auto attacks apply damage to their target immediately after their cast time ends.
        /// </summary>
        public bool IsMelee { get; set; }
        /// <summary>
        /// Current order this AI is performing. *NOTE*: Does not contain all possible values.
        /// </summary>
        /// TODO: Rework AI so this enum can be finished.
        public MoveOrder MoveOrder { get; set; }
        /// <summary>
        /// Unit this AI will auto attack when it is in auto attack range.
        /// </summary>
        public IAttackableUnit TargetUnit { get; set; }

        public ObjAiBase(Game game, string model, Stats.Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL) :
            base(game, model, stats, collisionRadius, x, y, visionRadius, netId, team)
        {
            _itemManager = game.ItemManager;
            _scriptEngine = game.ScriptEngine;

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
            if (CharData.PerceptionBubbleRadius > 0)
            {
                VisionRadius = CharData.PerceptionBubbleRadius;
            }
            else if (collisionRadius > 0)
            {
                VisionRadius = visionRadius;
            }
            else
            {
                VisionRadius = 1100;
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

        /// <summary>
        /// Adds the given buff instance to this AI.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// TODO: Probably needs a refactor to lessen thread usage. Make sure to stick very closely to the current method; just optimize it.
        /// TODO: Move to AttackableUnit.
        /// <summary>
        /// Adds the given buff instance to this AI.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// TODO: Probably needs a refactor to lessen thread usage. Make sure to stick very closely to the current method; just optimize it.
        /// TODO: Move to AttackableUnit.
        public void AddBuff(IBuff b)
        {
            lock (_buffsLock)
            {
                // If this is the first buff of this name to be added, then add it to the parent buffs list (regardless of its add type).
                if (!Buffs.ContainsKey(b.Name))
                {
                    // If the parent buff has ended, make the next oldest buff the parent buff.
                    if (HasBuff(b.Name))
                    {
                        var buff = GetBuffsWithName(b.Name)[0];
                        Buffs.Add(b.Name, buff);
                        return;
                    }
                    // If there is no other buffs of this name, make it the parent and add it normally.
                    Buffs.Add(b.Name, b);
                    BuffList.Add(b);
                    // Add the buff to the visual hud.
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffAdd2(b);
                    }
                    // Activate the buff for BuffScripts
                    b.ActivateBuff();
                }
                // If the buff is supposed to replace any existing buff instances of the same name
                else if (b.BuffAddType == BuffAddType.REPLACE_EXISTING)
                {
                    // Removing the previous buff of the same name.
                    var prevbuff = Buffs[b.Name];

                    prevbuff.DeactivateBuff();
                    RemoveBuff(b.Name);
                    BuffList.Remove(prevbuff);

                    // Clear the newly given buff's slot since we will move it into the previous buff's slot.
                    RemoveBuffSlot(b);

                    // Adding the newly given buff instance into the slot of the previous buff.
                    BuffSlots[prevbuff.Slot] = b;
                    b.SetSlot(prevbuff.Slot);

                    // Add the buff as a parent and normally.
                    Buffs.Add(b.Name, b);
                    BuffList.Add(b);

                    // Update the visual buff in-game (usually just resets the buff time of the visual icon).
                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffReplace(b);
                    }
                    b.ActivateBuff();
                }
                // If the buff is supposed to reset the timer on any existing buff instances of the same name.
                else if (b.BuffAddType == BuffAddType.RENEW_EXISTING)
                {
                    Buffs[b.Name].ResetTimeElapsed();

                    if (!b.IsHidden)
                    {
                        _game.PacketNotifier.NotifyNPC_BuffReplace(Buffs[b.Name]);
                    }
                    // Attempt to remove any stats or modifiers applied by the pre-existing buff instance's BuffScript.
                    // TODO: Replace with a better method that unloads and reloads all data of a script
                    RemoveStatModifier(Buffs[b.Name].GetStatsModifier());
                    // Re-activate the buff's BuffScript.
                    Buffs[b.Name].ActivateBuff();
                }
                // If the buff is supposed to be applied alongside any existing buff instances of the same name.
                else if (b.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS)
                {
                    // If we've hit the max stacks count for this buff add type (usually 254 for this BuffAddType).
                    if (Buffs[b.Name].StackCount >= Buffs[b.Name].MaxStacks)
                    {
                        // Get and remove the oldest buff of the same name so we can free up space for the newly given buff instance.
                        var tempbuffs = GetBuffsWithName(b.Name);
                        var oldestbuff = tempbuffs[0];

                        oldestbuff.DeactivateBuff();
                        RemoveBuff(b.Name);
                        BuffList.Remove(oldestbuff);
                        RemoveBuffSlot(oldestbuff);

                        // Move the next oldest buff of the same name into the position of the removed oldest buff.
                        tempbuffs = GetBuffsWithName(b.Name);

                        BuffSlots[oldestbuff.Slot] = tempbuffs[0];
                        Buffs.Add(oldestbuff.Name, tempbuffs[0]);
                        BuffList.Add(b);

                        if (!b.IsHidden)
                        {
                            // If the buff is a counter buff (ex: Nasus Q stacks), then use a packet specialized for big buff stack counts (int.MaxValue).
                            if (Buffs[b.Name].BuffType == BuffType.COUNTER)
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateNumCounter(Buffs[b.Name]);
                            }
                            // Otherwise, use the normal buff stack (254) update (usually just adds one to the number on the icon and refreshes the time of the icon).
                            else
                            {
                                _game.PacketNotifier.NotifyNPC_BuffUpdateCount(b, b.Duration, b.TimeElapsed);
                            }
                        }
                        b.ActivateBuff();

                        return;
                    }
                    // If we haven't hit the max stack count (usually 254).
                    BuffList.Add(b);

                    // Increment the number of stacks on the parent buff, which is the buff instance which is used for packets.
                    Buffs[b.Name].IncrementStackCount();

                    // Increment the number of stacks on every buff of the same name (so if any of them become the parent, there is no problem).
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
                // If the buff is supposed to add a stack to any existing buffs of the same name and refresh their timer.
                // Essentially the method is: have one parent buff which has the stacks, and just refresh its time, this means no overlapping buff instances, but functionally it is the same.
                else if (Buffs[b.Name].BuffAddType == BuffAddType.STACKS_AND_RENEWS)
                {
                    // Don't need the newly added buff instance as we already have a parent who we can add stacks to.
                    RemoveBuffSlot(b);

                    // Refresh the time of the parent buff and add a stack.
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
                    // Attempt to remove any stats or modifiers applied by the pre-existing buff instance's BuffScript.
                    // TODO: Replace with a better method that unloads and reloads all data of a script
                    RemoveStatModifier(Buffs[b.Name].GetStatsModifier());
                    Buffs[b.Name].ActivateBuff();
                }
            }
        }

        /// <summary>
        /// Adds a modifier to this AI's stats, ex: Armor, Attack Damage, Movespeed, etc.
        /// </summary>
        /// <param name="statModifier">Modifier to add.</param>
        /// TODO: Move to AttackableUnit.
        public void AddStatModifier(IStatsModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        /// <summary>
        /// Applies the specified crowd control to this AI, refer to CrowdControlType for examples.
        /// </summary>
        /// <param name="cc">Crowd control to apply.</param>
        /// TODO: Make a CrowdControl class which contains info. about the time the CC is applied, and functions to stop it on command (for scripts).
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
        /// Function called by this AI's auto attack projectile when it hits its target.
        /// </summary>
        public virtual void AutoAttackHit(IAttackableUnit target)
        {
            if (HasCrowdControl(CrowdControlType.BLIND))
            {
                target.TakeDamage(this, 0, DamageType.DAMAGE_TYPE_PHYSICAL,
                                             DamageSource.DAMAGE_SOURCE_ATTACK,
                                             DamageResultType.RESULT_MISS);
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

        /// <summary>
        /// Sets this AI's current auto attack to the given auto attack. *NOTE*: Will be deprecated when spells are fully implemented.
        /// </summary>
        /// <param name="newAutoAttackSpellData">Auto attack spell data to use.</param>
        public void ChangeAutoAttackSpellData(ISpellData newAutoAttackSpellData)
        {
            AaSpellData = newAutoAttackSpellData;
        }

        /// <summary>
        /// Sets this AI's current auto attack to the given auto attack. *NOTE*: Will be deprecated when spells are fully implemented.
        /// </summary>
        /// <param name="newAutoAttackSpellDataName">Name of the auto attack to use.</param>
        public void ChangeAutoAttackSpellData(string newAutoAttackSpellDataName)
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(newAutoAttackSpellDataName);
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

        /// <summary>
        /// Clears all crowd control from this AI.
        /// </summary>
        public void ClearAllCrowdControl()
        {
            _crowdControlList.Clear();
        }

        /// <summary>
        /// Forces this AI to move towards the given target (GameObject/Position (remove Target btw)).
        /// </summary>
        /// <param name="t">GameObject/Position to move towards.</param>
        /// <param name="dashSpeed">How fast (units/sec) to move towards the target.</param>
        /// <param name="followTargetMaxDistance">Max distance to follow the target before the dash ends. TODO: Verify.</param>
        /// <param name="backDistance">Distance to move past the target? Unused.</param>
        /// <param name="travelTime">Time until this dash ends.</param>
        /// TODO: Remove Target class.
        /// TODO: Find a good way to grab these variables from spell data.
        /// TODO: Verify if we should count Dashing as a form of Crowd Control.
        public void DashToTarget(ITarget t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            _dashTime = this.GetDistanceTo(t) / (dashSpeed * 0.001f);
            _dashElapsedTime = 0;
            DashSpeed = dashSpeed;
            SetWaypoints(new List<Vector2> { GetPosition(), new Vector2(t.X, t.Y) });
        }

        /// <summary>
        /// Function called when this unit's health drops to 0 or less.
        /// </summary>
        /// <param name="killer">Unit which killed this unit.</param>
        // TODO: Move functionality to AttackableUnit.Die() when scriptengine variable is moved to AttackableUnit.
        public override void Die(IAttackableUnit killer)
        {
            var onDie = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "OnDie");
            onDie?.Invoke(this, killer);
            base.Die(killer);
        }

        /// <summary>
        /// Gets the parent buff instance of the buffs of the given name.
        /// </summary>
        /// <param name="name">Internal buff name to check.</param>
        /// <returns>Parent buff instance.</returns>
        /// TODO: Move to AttackableUnit.
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

        /// <summary>
        /// Gets the slot of the given buff instance, or an open slot if no buff is given.
        /// </summary>
        /// <param name="buffToLookFor">Buff to check. Leave empty to get an empty slot.</param>
        /// <returns>Slot of the given buff or an empty slot.</returns>
        /// TODO: Move to AttackableUnit.
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

        /// <summary>
        /// Gets a list of all buffs applied to this AI (parents and children).
        /// </summary>
        /// <returns>List of buff instances.</returns>
        /// TODO: Move to AttackableUnit.
        public List<IBuff> GetBuffs()
        {
            return BuffList;
        }

        /// <summary>
        /// Gets the number of parent buffs applied to this AI.
        /// </summary>
        /// <returns>Number of parent buffs.</returns>
        /// TODO: Move to AttackableUnit.
        public int GetBuffsCount()
        {
            return Buffs.Count;
        }

        /// <summary>
        /// Gets a list of all buff instances of the given name (parent and children).
        /// </summary>
        /// <param name="buffName">Internal buff name to check.</param>
        /// <returns>List of buff instances.</returns>
        /// TODO: Move to AttackableUnit.
        public List<IBuff> GetBuffsWithName(string buffName)
        {
            lock (_buffsLock)
            {
                return BuffList.FindAll(b => b.IsBuffSame(buffName));
            }
        }

        /// <summary>
        /// Gets this AI's current movement speed (units/sec).
        /// </summary>
        /// <returns>Float units/sec.</returns>
        public override float GetMoveSpeed()
        {
            return IsDashing ? DashSpeed : base.GetMoveSpeed();
        }

        /// <summary>
        /// Gets a new buff slot for the given buff instance.
        /// </summary>
        /// <param name="b">Buff instance to add.</param>
        /// <returns>Byte buff slot of the given buff.</returns>
        /// TODO: Move to AttackableUnit.
        public byte GetNewBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot();
            BuffSlots[slot] = b;
            return slot;
        }

        /// <summary>
        /// Gets the HashString for this AI's model. Used for packets so clients know what data to load.
        /// </summary>
        /// <returns>Hashed string of this AI's model.</returns>
        /// TODO: Move to AttackableUnit.
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

        /// <summary>
        /// Whether or not this AI has the given buff instance.
        /// </summary>
        /// <param name="buff">Buff instance to check.</param>
        /// <returns>True/False.</returns>
        /// TODO: Move to AttackableUnit.
        public bool HasBuff(IBuff buff)
        {
            return !(BuffList.Find(b => b == buff) == null);
        }

        /// <summary>
        /// Whether or not this AI has a buff of the given name.
        /// </summary>
        /// <param name="buffName">Internal buff name to check for.</param>
        /// <returns>True/False.</returns>
        /// TODO: Move to AttackableUnit.
        public bool HasBuff(string buffName)
        {
            return !(BuffList.Find(b => b.IsBuffSame(buffName)) == null);
        }

        /// <summary>
        /// Whether or not this AI is affected by the given crowd control.
        /// </summary>
        /// <param name="ccType">Crowd control to check for.</param>
        /// <returns>True/False.</returns>
        public bool HasCrowdControl(CrowdControlType ccType)
        {
            return _crowdControlList.FirstOrDefault(cc => cc.IsTypeOf(ccType)) != null;
        }

        /// <summary>
        /// Called when this AI collides with the terrain or with another GameObject. Refer to CollisionHandler for exact cases.
        /// </summary>
        /// <param name="collider">GameObject that collided with this AI. Null if terrain.</param>
        /// <param name="isTerrain">Whether or not this AI collided with terrain.</param>
        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            if (IsDashing)
            {
                return;
            }

            base.OnCollision(collider, isTerrain);

            if (collider is IObjMissile || collider is IObjBuilding)
            {
                // TODO: Implement OnProjectileCollide/Hit here.
                return;
            }

            if (isTerrain)
            {
                // TODO: Move this to AttackableUnit alongside the scriptengine variable.
                var onCollideWithTerrain = _scriptEngine.GetStaticMethod<Action<IGameObject>>(Model, "Passive", "onCollideWithTerrain");
                onCollideWithTerrain?.Invoke(this);
            }
            else
            {
                // TODO: Move this to AttackableUnit alongside the scriptengine variable.
                var onCollide = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, IAttackableUnit>>(Model, "Passive", "onCollide");
                onCollide?.Invoke(this, collider as IAttackableUnit);

                // Champions are only teleported if they collide with other Champions.
                // TODO: Implement Collision Priority
                // TODO: Implement dynamic navigation grid for buildings and turrets.
                if (!(this is IChampion) || collider is IChampion || collider is IBaseTurret)
                {
                    // Teleport out of other objects (+1 for insurance).
                    Vector2 exit = Extensions.GetCircleEscapePoint(GetPosition(), CollisionRadius * 2, collider.GetPosition(), collider.CollisionRadius);
                    TeleportTo(exit.X, exit.Y);
                }
            }

            // If we were trying to path somewhere before colliding, then repath from our new position.
            if (!IsPathEnded())
            {
                // TODO: When using this safePath, sometimes we collide with the terrain again, so we use an unsafe path the next collision, however,
                // sometimes we collide again before we can finish the unsafe path, so we end up looping collisions between safe and unsafe paths, never actually escaping (ex: sharp corners).
                // Edit the current method to fix the above problem.
                List<Vector2> safePath = _game.Map.NavigationGrid.GetPath(GetPosition(), _game.Map.NavigationGrid.GetClosestTerrainExit(Waypoints.Last()));
                if (safePath != null)
                {
                    SetWaypoints(safePath);
                }
            }
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
                // TODO: Optimize this, preferably without things like CirclePoly.
                var targetCircle = new CirclePoly(TargetUnit.Target?.GetPosition() ?? TargetUnit.GetPosition(), Stats.Range.Total, 72);
                //Find optimal position...
                foreach (var point in targetCircle.Points.OrderBy(x => GetDistanceTo(X, Y)))
                {
                    if (!_game.Map.NavigationGrid.IsVisible(point))
                    {
                        continue;
                    }
                    var positionUsed = false;
                    foreach (var circlePoly in usedPositions)
                    {
                        if (circlePoly.CheckForOverLaps(new CirclePoly(point, CollisionRadius + 10, 20)))
                        {
                            positionUsed = true;
                        }
                    }

                    if (positionUsed)
                    {
                        continue;
                    }
                    SetWaypoints(new List<Vector2> { GetPosition(), point });
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Function which refreshes this AI's waypoints if they have a target.
        /// </summary>
        public virtual void RefreshWaypoints()
        {
            if (TargetUnit == null || TargetUnit.IsDead || GetDistanceTo(TargetUnit) <= Stats.Range.Total && Waypoints.Count == 1)
            {
                return;
            }

            // If the target is already in range, stay where we are.
            // TODO: Fix Waypoints so we don't have to keep adding our current position to the start.
            if (GetDistanceTo(TargetUnit) <= Stats.Range.Total - 2f)
            {
                StopMovement();
            }
            // Otherwise, move to the target.
            else
            {
                // TODO: Fix Waypoints so we don't have to keep adding our current position to the start.
                if (WaypointIndex >= Waypoints.Count)
                {
                    Vector2 targetPos = TargetUnit.GetPosition();
                    if (!_game.Map.NavigationGrid.IsWalkable(targetPos, TargetUnit.CollisionRadius))
                    {
                        targetPos = _game.Map.NavigationGrid.GetClosestTerrainExit(targetPos, CollisionRadius);
                    }

                    var newWaypoints = _game.Map.NavigationGrid.GetPath(GetPosition(), targetPos);
                    if (newWaypoints.Count > 1)
                    {
                        SetWaypoints(newWaypoints);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the given buff from this AI.
        /// </summary>
        /// <param name="b">Buff to remove.</param>
        /// TODO: Move to AttackableUnit.
        public void RemoveBuff(IBuff b)
        {
            lock (_buffsLock)
            {
                // If the buff is supposed to be applied alongside other buffs of the same name, and their are more than one already present.
                if (b.BuffAddType == BuffAddType.STACKS_AND_OVERLAPS && b.StackCount > 1)
                {
                    // Remove one stack and update the other buff instances of the same name
                    b.DecrementStackCount();

                    RemoveBuff(b.Name);
                    BuffList.Remove(b);
                    RemoveBuffSlot(b);

                    var tempbuffs = GetBuffsWithName(b.Name);

                    tempbuffs.ForEach(tempbuff => tempbuff.SetStacks(b.StackCount));

                    // Next oldest buff takes the place of the removed oldest buff; becomes parent buff.
                    BuffSlots[b.Slot] = tempbuffs[0];
                    Buffs.Add(b.Name, tempbuffs[0]);

                    // Used in packets to maintain the visual buff icon's timer, as removing a stack from the icon can reset the timer.
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

        /// <summary>
        /// Removes the parent buff of the given internal name from this AI object.
        /// </summary>
        /// <param name="b">Internal buff name to remove.</param>
        /// TODO: Move to AttackableUnit.
        public void RemoveBuff(string b)
        {
            lock (_buffsLock)
            {
                Buffs.Remove(b);
            }
        }

        /// <summary>
        /// Removes all buffs of the given internal name from this AI object.
        /// Also removes parent buffs.
        /// </summary>
        /// <param name="buffName">Internal buff name to remove.</param>
        /// TODO: Move to AttackableUnit.
        public void RemoveBuffsWithName(string buffName)
        {
            lock (_buffsLock)
            {
                BuffList.FindAll(b => 
                b.IsBuffSame(buffName)).ForEach(b => 
                b.DeactivateBuff());
            }
        }

        /// <summary>
        /// Removes the given buff instance from the buff slots of this AI.
        /// Called automatically by RemoveBuff().
        /// </summary>
        /// <param name="b">Buff instance to check for.</param>
        /// TODO: Move to AttackableUnit.
        private void RemoveBuffSlot(IBuff b)
        {
            var slot = GetBuffSlot(b);
            BuffSlots[slot] = null;
        }

        /// <summary>
        /// Removes the given crowd control instance from this AI.
        /// </summary>
        /// <param name="cc">Crowd control instance to remove.</param>
        public void RemoveCrowdControl(ICrowdControl cc)
        {
            _crowdControlList.Remove(cc);
        }

        /// <summary>
        /// Removes the given stat modifier instance from this AI.
        /// </summary>
        /// <param name="statModifier">Stat modifier instance to remove.</param>
        /// TODO: Move to AttackableUnit.
        public void RemoveStatModifier(IStatsModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }

        /// <summary>
        /// Sets this AI's current auto attack to their base auto attack.
        /// *NOTE*: Will be depricated when spell systems are fully implemented.
        /// </summary>
        public void ResetAutoAttackSpellData()
        {
            AaSpellData = _game.Config.ContentManager.GetSpellData(Model + "BasicAttack");
        }

        /// <summary>
        /// Sets this AI's current dash state to the given state.
        /// </summary>
        /// <param name="state">State to set. True = dashing, false = not dashing.</param>
        /// TODO: Verify if we want to classify Dashing as a form of Crowd Control.
        public void SetDashingState(bool state)
        {
            IsDashing = state;
        }

        /// <summary>
        /// Sets this AI's current target unit. This relates to both auto attacks as well as general spell targeting.
        /// </summary>
        /// <param name="target">Unit to target.</param>
        /// TODO: Remove Target class.
        public void SetTargetUnit(IAttackableUnit target)
        {
            TargetUnit = target;
            RefreshWaypoints();
        }

        /// <summary>
        /// Forces this AI to stop moving.
        /// </summary>
        /// TODO: Remove the need for the Waypoints to have at least 1 waypoint so this can be a simple clearing of waypoints (i.e. ClearWaypoints()).
        public void StopMovement()
        {
            SetWaypoints(new List<Vector2> { GetPosition() });
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

            // TODO: Move this to AttackableUnit alongside the scriptengine variable.
            var onUpdate = _scriptEngine.GetStaticMethod<Action<IAttackableUnit, double>>(Model, "Passive", "OnUpdate");
            onUpdate?.Invoke(this, diff);
            base.Update(diff);
            UpdateAutoAttackTarget(diff);
        }

        /// <summary>
        /// Updates this AI's current target and auto attack actions depending on conditions such as crowd control, death state, vision, distance to target, etc.
        /// </summary>
        /// <param name="diff">Number of milliseconds that passed before this tick occurred.</param>
        private void UpdateAutoAttackTarget(float diff)
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
                if (TargetUnit.IsDead || !_game.ObjectManager.TeamHasVisionOn(Team, TargetUnit) && !(TargetUnit is IBaseTurret) && !(Target is IObjBuilding))
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

        /// <summary>
        /// Sets this AI's current target unit.
        /// </summary>
        /// <param name="unit">Unit to target.</param>
        public void UpdateTargetUnit(IAttackableUnit unit)
        {
            TargetUnit = unit;
        }
    }
}
