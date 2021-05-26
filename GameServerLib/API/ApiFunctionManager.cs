using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Spell;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;

namespace LeagueSandbox.GameServer.API
{
    /// <summary>
    /// Class housing functions most commonly used by scripts.
    /// </summary>
    public static class ApiFunctionManager
    {
        // Required variables.
        private static Game _game;
        private static ILog _logger;

        /// <summary>
        /// Converts the given string of hex values into an array of bytes.
        /// </summary>
        /// <param name="hex">String of hex values.</param>
        /// <returns>Array of bytes.</returns>
        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        /// <summary>
        /// Sets the Game instance of ApiFunctionManager to the given instance.
        /// Also assigns the debug logger.
        /// </summary>
        /// <param name="game">Game instance to set.</param>
        internal static void SetGame(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
        }

        /// <summary>
        /// Logs the given string to the server console as info.
        /// </summary>
        /// <param name="format">String to print.</param>
        public static void LogInfo(string format)
        {
            _logger.Info(format);
        }

        /// <summary>
        /// Logs the given string and its arguments to the server console as info.
        /// Instanced classes in the arguments will be a string representation of the object's namespace.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void LogInfo(string format, params object[] args)
        {
            _logger.Info(string.Format(format, args));
        }

        /// <summary>
        /// Logs the given string to the server console as debug info.
        /// Only works in Debug mode.
        /// </summary>
        /// <param name="format">String to debug print.</param>
        public static void LogDebug(string format)
        {
            _logger.Debug(format);
        }

        /// <summary>
        /// Logs the given string to the server console as debug info.
        /// Only works in Debug mode.
        /// Instanced classes in the arguments will be a string representation of the object's namespace.
        /// </summary>
        /// <param name="format">String to debug print.</param>
        public static void LogDebug(string format, params object[] args)
        {
            _logger.Debug(string.Format(format, args));
        }

        /// <summary>
        /// Creates a new instance of a GameScriptTimer with the specified arguments.
        /// </summary>
        /// <param name="duration">Time till the timer ends.</param>
        /// <param name="callback">Action to perform when the timer ends.</param>
        /// <returns>New GameScriptTimer instance.</returns>
        public static GameScriptTimer CreateTimer(float duration, Action callback)
        {
            var newTimer = new GameScriptTimer(duration, callback);
            _game.AddGameScriptTimer(newTimer);

            return newTimer;
        }

        /// <summary>
        /// Sets the visibility of the specified GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to set.</param>
        /// <param name="visibility">Whether or not the GameObject should be visible.</param>
        public static void SetGameObjectVisibility(IGameObject gameObject, bool visibility)
        {
            var teams = GetTeams();
            foreach (var id in teams)
            {
                gameObject.SetVisibleByTeam(id, visibility);
            }
        }

        /// <summary>
        /// Gets the possible teams.
        /// </summary>
        /// <returns>Usually BLUE/PURPLE/NEUTRAL.</returns>
        public static List<TeamId> GetTeams()
        {
            return _game.ObjectManager.Teams;
        }

        /// <summary>
        /// Teleports an AI unit to the specified coordinates.
        /// Instant.
        /// TODO: Change to GameObjects.
        /// </summary>
        /// <param name="unit">AI unit to teleport.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public static void TeleportTo(IObjAiBase unit, float x, float y)
        {
            if (unit.MovementParameters != null)
            {
                CancelDash(unit);
            }

            unit.TeleportTo(x, y);
        }

        public static void FaceDirection(Vector2 location, IGameObject target, bool isInstant = false, float turnTime = 0.08333f)
        {
            if (location == target.Position)
            {
                return;
            }

            var goingTo = location - target.Position;
            var direction = Vector2.Normalize(goingTo);

            target.FaceDirection(new Vector3(direction.X, 0, direction.Y), isInstant, turnTime);
        }

        /// <summary>
        /// Gets a point that is in the direction the specified unit is facing, given it is a specified distance away from the unit.
        /// </summary>
        /// <param name="obj">Unit to base the point off of.</param>
        /// <param name="offsetAngle">Offset angle from the unit's facing angle (in degrees, clockwise). Must be > 0 to have an effect.</param>
        /// <returns>Vector2 point.</returns>
        public static Vector2 GetPointFromUnit(IGameObject obj, float distance, float offsetAngle = 0)
        {
            Vector2 pos = new Vector2(obj.Position.X, obj.Position.Y);
            Vector2 dir = new Vector2(obj.Direction.X, obj.Direction.Z);
            if (offsetAngle != 0)
            {
                dir = GameServerCore.Extensions.Rotate(dir, offsetAngle);
            }
            return pos + (dir * distance);
        }

        /// <summary>
        /// Reports whether or not the specified coordinates are walkable.
        /// </summary>
        /// <param name="x">X coordinaate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="checkRadius">Radius around the given point to check for walkability.</param>
        /// <returns>True/False</returns>
        public static bool IsWalkable(float x, float y, float checkRadius = 0)
        {
            return _game.Map.NavigationGrid.IsWalkable(x, y, checkRadius);
        }

        /// <summary>
        /// Adds a named buff with the given duration, stacks, and origin spell to a unit.
        /// From = owner of the spell (usually caster).
        /// </summary>
        /// <param name="buffName">Internally named buff to add.</param>
        /// <param name="duration">Time in seconds the buff should last.</param>
        /// <param name="stacks">Stacks of the buff to add.</param>
        /// <param name="originspell">Spell which called this function.</param>
        /// <param name="onto">Target of the buff.</param>
        /// <param name="from">Owner of the buff.</param>
        /// <param name="infiniteduration">Whether or not the buff should last forever.</param>
        /// <returns>New buff instance.</returns>
        public static IBuff AddBuff(string buffName, float duration, byte stacks, ISpell originspell, IAttackableUnit onto, IObjAiBase from, bool infiniteduration = false)
        {
            IBuff buff;

            try
            {
                buff = new Buff(_game, buffName, duration, stacks, originspell, onto, from, infiniteduration);
            }
            catch (ArgumentException exception)
            {
                _logger.Error(exception);
                return null;
            }

            onto.AddBuff(buff);
            return buff;
        }

        /// <summary>
        /// Whether or not the specified unit has the specified buff instance.
        /// </summary>
        /// <param name="unit">Unit to check.</param>
        /// <param name="b">Buff to check for.</param>
        /// <returns>True/False</returns>
        public static bool HasBuff(IAttackableUnit unit, IBuff b)
        {
            return unit.HasBuff(b);
        }

        /// <summary>
        /// Whether or not the specified unit has a buff with the given name.
        /// </summary>
        /// <param name="unit">Unit to check.</param>
        /// <param name="b">Buff name to check for.</param>
        /// <returns>True/False</returns>
        public static bool HasBuff(IAttackableUnit unit, string b)
        {
            return unit.HasBuff(b);
        }

        /// <summary>
        /// Sets the stacks of the specified buff instance to the given number of stacks.
        /// </summary>
        /// <param name="b">Buff instance.</param>
        /// <param name="newStacks">Stacks to set.</param>
        public static void EditBuff(IBuff b, byte newStacks)
        {
            b.SetStacks(newStacks);
        }

        /// <summary>
        /// Removes the specified buff from any AI units it is applied to and runs OnDeactivate callback for the buff's script.
        /// If the buff's BuffAddType is STACKS_AND_OVERLAPS, each stack is individually instanced, so only one stack is removed.
        /// </summary>
        /// <param name="buff">Buff instance to remove.</param>
        public static void RemoveBuff(IBuff buff)
        {
            buff.DeactivateBuff();
        }

        /// <summary>
        /// Removes all buffs of the given name from the specified AI unit and runs OnDeactivate callback for the buff's script.
        /// Even if the buff's BuffAddType is STACKS_AND_OVERLAPS, it will still remove all buff instances.
        /// </summary>
        /// <param name="target">AI unit to check.</param>
        /// <param name="buff">Buff name to remove.</param>
        public static void RemoveBuff(IAttackableUnit target, string buff)
        {
            target.RemoveBuffsWithName(buff);
        }

        /// <summary>
        /// Creates a new particle with the specified parameters.
        /// </summary>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="bindObj">GameObject that the particle should bind to.</param>
        /// <param name="particle">Internal name of the particle.</param>
        /// <param name="position">Position to spawn at.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="size">Scale.</param>
        /// <param name="bone">Bone on the owner the particle should be attached to.</param>
        /// <param name="targetBone">Bone on the target the particle should be attached to.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="reqVision">Whether or not the particle can be obstructed by terrain.</param>
        /// <returns>New particle instance.</returns>
        public static IParticle AddParticle(IGameObject caster, IGameObject bindObj, string particle, Vector2 position, float lifetime = 1.0f, float size = 1.0f, string bone = "", string targetBone = "", Vector3 direction = new Vector3(), bool followGroundTilt = false, bool reqVision = true, TeamId teamOnly = TeamId.TEAM_NEUTRAL)
        {
            var p = new Particle(_game, caster, bindObj, position, particle, size, bone, targetBone, 0, direction, followGroundTilt, lifetime, reqVision, true, teamOnly);
            return p;
        }

        /// <summary>
        /// Creates a new particle with the specified parameters.
        /// This particle will be attached to a target.
        /// </summary>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="bindObj">GameObject that the particle should bind to (prioritized over target).</param>
        /// <param name="particle">Internal name of the particle.</param>
        /// <param name="target">GameObject that the particle should bind to after the bindObj.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="size">Scale.</param>
        /// <param name="bone">Bone on the owner the particle should be attached to.</param>
        /// <param name="targetBone">Bone on the target the particle should be attached to.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="reqVision">Whether or not the particle can be obstructed by terrain.</param>
        /// <returns>New particle instance.</returns>
        public static IParticle AddParticleTarget(IGameObject caster, IGameObject bindObj, string particle, IGameObject target, float lifetime = 1.0f, float size = 1.0f, string bone = "", string targetBone = "", Vector3 direction = new Vector3(), bool followGroundTilt = false, bool reqVision = true)
        {
            var p = new Particle(_game, caster, bindObj, target, particle, size, bone, targetBone, 0, direction, followGroundTilt, lifetime, reqVision);
            return p;
        }

        /// <summary>
        /// Removes the specified particle from ObjectManager and networks the removal to players.
        /// </summary>
        /// <param name="p">Particle to remove.</param>
        public static void RemoveParticle(IParticle p)
        {
            if (p != null)
            {
                p.SetToRemove();
            }
        }

        /// <summary>
        /// Creates a new Minion with the specified parameters.
        /// </summary>
        /// <param name="owner">AI unit that owns this minion.</param>
        /// <param name="model">Internal name of the model of this minion.</param>
        /// <param name="name">Internal name of the minion.</param>
        /// <param name="position">Position to spawn at.</param>
        /// <param name="isVisible">Whether or not this minion should be visible.</param>
        /// <param name="aiPaused">Whether or not this minion's AI is inactive.</param>
        /// <returns>New Minion instance.</returns>
        public static IMinion AddMinion(IObjAiBase owner, string model, string name, Vector2 position, Vector2 facingDirection = new Vector2(), bool isVisible = true, bool aiPaused = true)
        {
            var m = new Minion(_game, owner, position, model, name, 0, owner.Team);
            _game.ObjectManager.AddObject(m);
            m.SetVisibleByTeam(owner.Team, isVisible);
            m.PauseAi(aiPaused);
            return m;
        }

        /// <summary>
        /// Creates a new Minion with the specified parameters.
        /// Minion will spawn at the given target's position.
        /// </summary>
        /// <param name="owner">AI unit that owns this minion.</param>
        /// <param name="model">Internal name of the minion's model.</param>
        /// <param name="name">Internal name of the minion.</param>
        /// <param name="target">Target to spawn the minion on.</param>
        /// <param name="isVisible">Whether or not this minion should be visible.</param>
        /// <param name="aiPaused">Whether or not this minion's AI is inactive.</param>
        /// <returns>New Minion instance.</returns>
        public static IMinion AddMinionTarget(IObjAiBase owner, string model, string name, IGameObject target, bool isVisible = true, bool aiPaused = true)
        {
            // TODO: Implement attachable Minions/GameObjects.
            return AddMinion(owner, model, name, target.Position, isVisible: isVisible, aiPaused: aiPaused);
        }

        /// <summary>
        /// Prints the specified string to the in-game chat.
        /// </summary>
        /// <param name="msg">String to print.</param>
        public static void PrintChat(string msg)
        {
            _game.PacketNotifier.NotifyDebugMessage(msg); // TODO: Move PacketNotifier usage to less abstract classes
        }

        /// <summary>
        /// Checks if the AttackableUnit is within the specified range of a target position.
        /// </summary>
        /// <param name="unit">Unit to check.</param>
        /// <param name="targetPos">Position to check from.</param>
        /// <param name="range">Range around the position to check.</param>
        /// <param name="isAlive">Whether or not the unit should be alive.</param>
        /// <returns></returns>
        public static bool IsUnitInRange(IAttackableUnit unit, Vector2 targetPos, float range, bool isAlive)
        {
            if (unit.IsDead == isAlive)
            {
                return false;
            }

            return GameServerCore.Extensions.IsVectorWithinRange(unit.Position, targetPos, range);
        }

        /// <summary>
        /// Acquires all alive or dead AttackableUnits within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <returns>List of AttackableUnits.</returns>
        public static List<IAttackableUnit> GetUnitsInRange(Vector2 targetPos, float range, bool isAlive)
        {
            return _game.ObjectManager.GetUnitsInRange(targetPos, range, isAlive);
        }

        /// <summary>
        /// Acquires the closest alive or dead AttackableUnit within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <returns>Closest AttackableUnit.</returns>
        public static IAttackableUnit GetClosestUnitInRange(Vector2 targetPos, float range, bool isAlive)
        {
            var units = _game.ObjectManager.GetUnitsInRange(targetPos, range, isAlive);
            var orderedUnits = units.OrderBy(unit => Vector2.DistanceSquared(targetPos, unit.Position));

            return orderedUnits.First();
        }

        /// <summary>
        /// Acquires the closest alive or dead AttackableUnit within the specified range of another unit.
        /// </summary>
        /// <param name="target">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <returns>Closest AttackableUnit.</returns>
        public static IAttackableUnit GetClosestUnitInRange(IAttackableUnit target, float range, bool isAlive)
        {
            var units = _game.ObjectManager.GetUnitsInRange(target.Position, range, isAlive);
            var orderedUnits = units.OrderBy(unit => Vector2.DistanceSquared(target.Position, unit.Position));

            if (orderedUnits.First() == target && orderedUnits.Count() > 1)
            {
                return orderedUnits.ElementAt(1);
            }

            return orderedUnits.First();
        }

        /// <summary>
        /// Acquires all alive or dead Champions within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not the return alive Champions.</param>
        /// <returns>List of Champions.</returns>
        public static List<IChampion> GetChampionsInRange(Vector2 targetPos, float range, bool isAlive)
        {
            return _game.ObjectManager.GetChampionsInRange(targetPos, range, isAlive);
        }

        /// <summary>
        /// Instantly cancels any dashes the specified unit is performing.
        /// </summary>
        /// <param name="unit">Unit to stop dashing.</param>
        public static void CancelDash(IAttackableUnit unit)
        {
            // Allow the user to move the champion
            unit.SetDashingState(false);
        }

        /// <summary>
        /// Forces the specified unit to perform a forced movement which ends at a specified position.
        /// </summary>
        /// <param name="unit">Unit that will perform the forced movement.</param>
        /// <param name="animation">Internally named animation to play during the forced movement.</param>
        /// <param name="target">End position of the forced movement.</param>
        /// <param name="speed">How fast the forced movement should travel.</param>
        /// <param name="idealDistance">How far the forced movement should travel from the unit's position.</param>
        /// <param name="gravity">How high the force movement should reach at the mid point of the force movement.</param>
        /// <param name="moveBackBy">How far behind the end point the force movement should go before finishing.</param>
        /// <param name="movementType">Type of force movement to perform. Refer to ForceMovementType enum.</param>
        /// <param name="movementOrdersType">How should the force movement affect the orders of the unit?</param>
        /// <param name="movementOrdersFacing">How should the force movement affect the facing direction of the unit?</param>
        /// TODO: Fully implement new ForceMovement functionality in AttackableUnit.
        public static void ForceMovement
        (
            IAttackableUnit unit,
            string animation,
            Vector2 target,
            float speed,
            float idealDistance,
            float gravity,
            float moveBackBy,
            ForceMovementType movementType = ForceMovementType.FURTHEST_WITHIN_RANGE,
            ForceMovementOrdersType movementOrdersType = ForceMovementOrdersType.POSTPONE_CURRENT_ORDER,
            ForceMovementOrdersFacing movementOrdersFacing = ForceMovementOrdersFacing.FACE_MOVEMENT_DIRECTION)
        {
            var keepFacingLastDirection = false;
            if (movementOrdersFacing == ForceMovementOrdersFacing.KEEP_CURRENT_FACING)
            {
                keepFacingLastDirection = true;
            }
            unit.DashToLocation(target, speed, animation, gravity, keepFacingLastDirection);
        }

        /// <summary>
        /// Forces the specified unit to perform a forced movement which follows a specified target unit.
        /// </summary>
        /// <param name="unit">Unit that will perform the forced movement.</param>
        /// <param name="animation">Internally named animation to play during the forced movement.</param>
        /// <param name="target">Target unit the forced movement will follow.</param>
        /// <param name="speed">How fast the forced movement should travel.</param>
        /// <param name="idealDistance">How far the forced movement should travel from the unit's position.</param>
        /// <param name="gravity">How high the force movement should reach at the mid point of the force movement.</param>
        /// <param name="moveBackBy">How far behind the end point the force movement should go before finishing.</param>
        /// <param name="maxTravelTime">Maximum amount of time the forced movement is allowed to last.</param>
        /// <param name="movementType">Type of force movement to perform. Refer to ForceMovementType enum.</param>
        /// <param name="movementOrdersType">How should the force movement affect the orders of the unit?</param>
        /// <param name="movementOrdersFacing">How should the force movement affect the facing direction of the unit?</param>
        /// TODO: Fully implement new ForceMovement functionality in AttackableUnit.
        public static void ForceMovement
        (
            IObjAiBase unit,
            IAttackableUnit target,
            string animation,
            float speed,
            float idealDistance,
            float gravity,
            float moveBackBy,
            float maxTravelTime,
            ForceMovementType movementType = ForceMovementType.FURTHEST_WITHIN_RANGE,
            ForceMovementOrdersType movementOrdersType = ForceMovementOrdersType.POSTPONE_CURRENT_ORDER,
            ForceMovementOrdersFacing movementOrdersFacing = ForceMovementOrdersFacing.FACE_MOVEMENT_DIRECTION)
        {
            var keepFacingLastDirection = false;
            if (movementOrdersFacing == ForceMovementOrdersFacing.KEEP_CURRENT_FACING)
            {
                keepFacingLastDirection = true;
            }
            unit.DashToTarget(target, speed, animation, gravity, keepFacingLastDirection, idealDistance, moveBackBy, maxTravelTime);
        }

        /// <summary>
        /// Forces the given object to perform the given animation.
        /// </summary>
        /// <param name="obj">Object that will play the animation.</param>
        /// <param name="animName">Internal name of an animation to play.</param>
        /// <param name="timeScale">How fast the animation should play. Default 1x speed.</param>
        /// <param name="startTime">Time in the animation to start at.</param>
        /// TODO: Verify if this description is correct, if not, correct it.
        /// <param name="speedScale">How much the speed of the GameObject should affect the animation.</param>
        /// TODO: Implement AnimationFlags enum for this and fill it in.
        /// <param name="flags">Animation flags. Refer to AnimationFlags enum.</param>
        public static void PlayAnimation(IGameObject obj, string animName, float timeScale = 1.0f, float startTime = 0, float speedScale = 0, AnimationFlags flags = 0)
        {
            obj.PlayAnimation(animName, timeScale, startTime, speedScale, flags);
        }

        /// <summary>
        /// Forces the given object's current animations to pause/unpause.
        /// </summary>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        public static void PauseAnimation(IGameObject obj, bool pause)
        {
            obj.PauseAnimation(pause);
        }

        /// <summary>
        /// Forces the given object to stop performing the given animation (or optionally all animations).
        /// </summary>
        /// <param name="obj">Object who's animations will be stopped.</param>
        /// <param name="animation">Internal name of the animation to stop playing. Set blank/null if stopAll is true.</param>
        /// <param name="stopAll">Whether or not to stop all animations. Only works if animation is empty/null.</param>
        /// <param name="fade">Whether or not the animation should fade before stopping.</param>
        /// <param name="ignoreLock">Whether or not locked animations should still be stopped.</param>
        public static void StopAnimation(IGameObject obj, string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true)
        {
            obj.StopAnimation(animation, stopAll, fade, ignoreLock);
        }

        public static void SealSpellSlot(IObjAiBase target, SpellSlotType slotType, int slot, SpellbookType spellbookType, bool seal)
        {
            if (spellbookType == SpellbookType.SPELLBOOK_UNKNOWN
                || spellbookType == SpellbookType.SPELLBOOK_SUMMONER && (slotType != SpellSlotType.SummonerSpellSlots)
                || (spellbookType == SpellbookType.SPELLBOOK_CHAMPION
                    && ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                        || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                        || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))))
            {
                return;
            }

            if (spellbookType == SpellbookType.SPELLBOOK_CHAMPION)
            {
                if (slotType == SpellSlotType.InventorySlots)
                {
                    slot += (int)SpellSlotType.InventorySlots;
                }
                if (slotType == SpellSlotType.ExtraSlots)
                {
                    slot += (int)SpellSlotType.ExtraSlots;
                }
            }

            if (spellbookType == SpellbookType.SPELLBOOK_SUMMONER)
            {
                target.Stats.SetSummonerSpellEnabled((byte)slot, !seal);
                return;
            }

            target.Stats.SetSpellEnabled((byte)slot, !seal);
        }

        /// <summary>
        /// Sets the specified unit's animation states to the given set of states.
        /// Given state pairs are expected to follow a specific structure:
        /// First string is the animation to override, second string is the animation to play in place of the first.
        /// </summary>
        /// <param name="unit">Unit to set animation states on.</param>
        /// <param name="animPairs">Dictionary of animations to set.</param>
        public static void SetAnimStates(IAttackableUnit unit, Dictionary<string, string> animPairs)
        {
            unit.SetAnimStates(animPairs);
        }

        public static void SetStatus(IAttackableUnit unit, StatusFlags status, bool enabled)
        {
            unit.SetStatus(status, enabled);
        }

        public static void SetTargetingType(IObjAiBase target, SpellSlotType slotType, int slot, TargetingType newType)
        {
            if ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))
            {
                return;
            }

            if (slotType == SpellSlotType.InventorySlots)
            {
                slot += (int)SpellSlotType.InventorySlots;
            }

            if (slotType == SpellSlotType.TempItemSlot)
            {
                slot = (int)SpellSlotType.TempItemSlot;
            }

            if (slotType == SpellSlotType.ExtraSlots)
            {
                slot += (int)SpellSlotType.ExtraSlots;
            }

            ISpell spell = target.GetSpell((byte)slot);

            spell.SpellData.SetTargetingType(newType);

            if (target is IChampion champion)
            {
                _game.PacketNotifier.NotifyChangeSlotSpellData
                (
                    (int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId,
                    target,
                    (byte)slot,
                    ChangeSlotSpellDataType.TargetingType,
                    targetingType: newType
                );
            }
        }

        public static void SpellCast(IObjAiBase caster, int slot, SpellSlotType slotType, Vector2 pos, Vector2 endPos, bool fireWithoutCasting, Vector2 overrideCastPos, List<ICastTarget> targets = null, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            if ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))
            {
                return;
            }

            if (slotType == SpellSlotType.InventorySlots)
            {
                slot += (int)SpellSlotType.InventorySlots;
            }

            if (slotType == SpellSlotType.TempItemSlot)
            {
                slot = (int)SpellSlotType.TempItemSlot;
            }

            if (slotType == SpellSlotType.ExtraSlots)
            {
                slot += (int)SpellSlotType.ExtraSlots;
            }

            ISpell spell = caster.GetSpell((byte)slot);

            if (targets == null)
            {
                targets = new List<ICastTarget> { new CastTarget(null, HitResult.HIT_Normal) };
            }

            ICastInfo castInfo = new CastInfo()
            {
                SpellHash = (uint)spell.GetId(),
                SpellNetID = _game.NetworkIdManager.GetNewNetId(),
                SpellLevel = spell.CastInfo.SpellLevel,
                AttackSpeedModifier = caster.Stats.AttackSpeedMultiplier.Total,
                Owner = caster,
                // TODO: Verify
                SpellChainOwnerNetID = caster.NetId,
                PackageHash = caster.GetObjHash(),
                MissileNetID = _game.NetworkIdManager.GetNewNetId(),
                TargetPosition = new Vector3(pos.X, caster.GetHeight(), pos.Y),
                TargetPositionEnd = new Vector3(endPos.X, caster.GetHeight(), endPos.Y),

                Targets = targets,

                IsAutoAttack = updateAutoAttackTimer,
                // TODO: Verify the differences between these two and make separate options for them.
                UseAttackCastTime = useAutoAttackSpell,
                UseAttackCastDelay = false,
                IsForceCastingOrChannel = isForceCastingOrChanneling,
                
                SpellSlot = (byte)slot,
                SpellCastLaunchPosition = caster.GetPosition3D()
            };

            if (overrideCastPos != Vector2.Zero)
            {
                castInfo.IsOverrideCastPosition = true;
                castInfo.SpellCastLaunchPosition = new Vector3(overrideCastPos.X, caster.GetHeight(), overrideCastPos.Y);

                if (endPos == Vector2.Zero)
                {
                    castInfo.TargetPositionEnd = new Vector3(pos.X, caster.GetHeight(), pos.Y);
                }
            }

            if (overrideForceLevel >= 0)
            {
                castInfo.SpellLevel = (byte)overrideForceLevel;
            }

            spell.Cast(castInfo, !fireWithoutCasting);
        }

        public static void SpellCast(IObjAiBase caster, int slot, SpellSlotType slotType, bool fireWithoutCasting, IAttackableUnit target, Vector2 overrideCastPos, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            ICastTarget castTarget = new CastTarget(target, CastTarget.GetHitResult(target, useAutoAttackSpell, caster.IsNextAutoCrit));

            SpellCast(caster, slot, slotType, target.Position, target.Position, fireWithoutCasting, overrideCastPos, new List<ICastTarget> { castTarget }, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);
        }

        public static void SpellCastItem(IObjAiBase caster, string itemSpellName, bool fireWithoutCasting, IAttackableUnit target, Vector2 overrideCastPos, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            // Apply the spell to the TempItemSlot.
            caster.SetSpell(itemSpellName, (byte)SpellSlotType.TempItemSlot, true);
            SpellCast(caster, 0, SpellSlotType.TempItemSlot, fireWithoutCasting, target, overrideCastPos, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);
        }

        public static void SpellCastItem(IObjAiBase caster, string itemSpellName, Vector2 pos, Vector2 endPos, bool fireWithoutCasting, Vector2 overrideCastPos, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            // Apply the spell to the TempItemSlot.
            caster.SetSpell(itemSpellName, (byte)SpellSlotType.TempItemSlot, true);
            SpellCast(caster, 0, SpellSlotType.TempItemSlot, pos, endPos, fireWithoutCasting, overrideCastPos, null, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);
        }

        public static void StopChanneling(IObjAiBase target, ChannelingStopCondition stopCondition, ChannelingStopSource stopSource)
        {
            target.StopChanneling(stopCondition, stopSource);
        }
    }
}
