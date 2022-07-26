using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.API
{
    /// <summary>
    /// Class housing functions most commonly used by scripts.
    /// </summary>
    public static class ApiFunctionManager
    {
        // Required variables.
        private static Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();

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
        /// This should only be used for debug purposes.
        /// </summary>
        /// <param name="duration">Time till the timer ends.</param>
        /// <param name="callback">Action to perform when the timer ends.</param>
        /// <returns>New GameScriptTimer instance.</returns>
        [Obsolete("CreateTimer should only be used for debug purposes.")]
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
        public static void SetGameObjectVisibility(GameObject gameObject, bool visibility)
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

        public static int ConvertAPISlot(SpellSlotType slotType, int slot)
        {
            if ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))
            {
                return -1;
            }

            if (slotType == SpellSlotType.SummonerSpellSlots)
            {
                slot += (int)SpellSlotType.SummonerSpellSlots;
            }
            else if (slotType == SpellSlotType.InventorySlots)
            {
                slot += (int)SpellSlotType.InventorySlots;
            }
            else if (slotType == SpellSlotType.TempItemSlot)
            {
                slot = (int)SpellSlotType.TempItemSlot;
            }
            else if (slotType == SpellSlotType.ExtraSlots)
            {
                slot += (int)SpellSlotType.ExtraSlots;
            }

            return slot;
        }

        public static int ConvertAPISlot(SpellbookType spellbookType, SpellSlotType slotType, int slot)
        {
            if (spellbookType == SpellbookType.SPELLBOOK_UNKNOWN
                || spellbookType == SpellbookType.SPELLBOOK_SUMMONER && (slotType != SpellSlotType.SummonerSpellSlots)
                || (spellbookType == SpellbookType.SPELLBOOK_CHAMPION
                    && ((slotType == SpellSlotType.SpellSlots && (slot < 0 || slot > 3))
                        || (slotType == SpellSlotType.InventorySlots && (slot < 0 || slot > 6))
                        || (slotType == SpellSlotType.ExtraSlots && (slot < 0 || slot > 15)))))
            {
                return -1;
            }

            if (spellbookType == SpellbookType.SPELLBOOK_CHAMPION)
            {
                if (slotType == SpellSlotType.InventorySlots)
                {
                    slot += (int)SpellSlotType.InventorySlots;
                }
                else if (slotType == SpellSlotType.TempItemSlot)
                {
                    slot = (int)SpellSlotType.TempItemSlot;
                }
                else if (slotType == SpellSlotType.ExtraSlots)
                {
                    slot += (int)SpellSlotType.ExtraSlots;
                }
            }
            else if (spellbookType == SpellbookType.SPELLBOOK_SUMMONER)
            {
                if (slotType == SpellSlotType.SummonerSpellSlots)
                {
                    slot += (int)SpellSlotType.SummonerSpellSlots;
                }
            }

            return slot;
        }

        /// <summary>
        /// Teleports an AI unit to the specified coordinates.
        /// Instant.
        /// TODO: Change to GameObjects.
        /// </summary>
        /// <param name="unit">AI unit to teleport.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public static void TeleportTo(ObjAIBase unit, float x, float y)
        {
            if (unit.MovementParameters != null)
            {
                CancelDash(unit);
            }

            unit.TeleportTo(x, y);
        }

        public static void FaceDirection(Vector2 location, GameObject target, bool isInstant = false, float turnTime = 0.08333f)
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
        public static Vector2 GetPointFromUnit(GameObject obj, float distance, float offsetAngle = 0)
        {
            Vector2 pos = obj.Position;
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
            return _game.Map.PathingHandler.IsWalkable(new Vector2(x, y), checkRadius);
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
        public static Buff AddBuff(string buffName, float duration, byte stacks, Spell originspell, AttackableUnit onto, ObjAIBase from, bool infiniteduration = false, IEventSource parent = null)
        {
            Buff buff;

            try
            {
                buff = new Buff(_game, buffName, duration, stacks, originspell, onto, from, infiniteduration, parent);
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
        public static bool HasBuff(AttackableUnit unit, Buff b)
        {
            return unit.HasBuff(b);
        }

        /// <summary>
        /// Whether or not the specified unit has a buff with the given name.
        /// </summary>
        /// <param name="unit">Unit to check.</param>
        /// <param name="b">Buff name to check for.</param>
        /// <returns>True/False</returns>
        public static bool HasBuff(AttackableUnit unit, string b)
        {
            return unit.HasBuff(b);
        }

        /// <summary>
        /// Sets the stacks of the specified buff instance to the given number of stacks.
        /// </summary>
        /// <param name="b">Buff instance.</param>
        /// <param name="newStacks">Stacks to set.</param>
        public static void EditBuff(Buff b, byte newStacks)
        {
            b.SetStacks(newStacks);
        }

        /// <summary>
        /// Removes the specified buff from any AI units it is applied to and runs OnDeactivate callback for the buff's script.
        /// If the buff's BuffAddType is STACKS_AND_OVERLAPS, each stack is individually instanced, so only one stack is removed.
        /// </summary>
        /// <param name="buff">Buff instance to remove.</param>
        public static void RemoveBuff(Buff buff)
        {
            buff.DeactivateBuff();
        }

        /// <summary>
        /// Removes all buffs of the given name from the specified AI unit and runs OnDeactivate callback for the buff's script.
        /// Even if the buff's BuffAddType is STACKS_AND_OVERLAPS, it will still remove all buff instances.
        /// </summary>
        /// <param name="target">AI unit to check.</param>
        /// <param name="buff">Buff name to remove.</param>
        public static void RemoveBuff(AttackableUnit target, string buff)
        {
            target.RemoveBuffsWithName(buff);
        }

        /// <summary>
        /// Creates a new particle with the specified parameters.
        /// </summary>
        /// <param name="caster">GameObject that caused this particle to spawn.</param>
        /// <param name="particle">Internal name of the particle.</param>
        /// <param name="start">Position to spawn at.</param>
        /// <param name="end">Position to end at.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="size">Scale.</param>
        /// <param name="bone">Bone on the owner the particle should be attached to.</param>
        /// <param name="targetBone">Bone on the target the particle should be attached to.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="followGroundTilt">Whether or not the particle should be titled along the ground towards its end position.</param>
        /// <param name="reqVision">Whether or not the particle can be obstructed by terrain.</param>
        /// <param name="teamOnly">The only team which should be able to see the particle.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        /// <returns>New particle instance.</returns>
        public static Particle AddParticlePos(GameObject caster, string particle, Vector2 start, Vector2 end, float lifetime = 1.0f, float size = 1.0f, string bone = "", string targetBone = "", Vector3 direction = new Vector3(), bool followGroundTilt = false, TeamId teamOnly = TeamId.TEAM_NEUTRAL, GameObject unitOnly = null, FXFlags flags = FXFlags.BindDirection)
        {
            var p = new Particle(_game, caster, start, end, particle, size, bone, targetBone, 0, direction, followGroundTilt, lifetime, teamOnly, unitOnly, flags);
            return p;
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
        /// <param name="teamOnly">The only team which should be able to see the particle.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        /// <returns>New particle instance.</returns>
        public static Particle AddParticle(GameObject caster, GameObject bindObj, string particle, Vector2 position, float lifetime = 1.0f, float size = 1.0f, string bone = "", string targetBone = "", Vector3 direction = new Vector3(), bool followGroundTilt = false, TeamId teamOnly = TeamId.TEAM_NEUTRAL, GameObject unitOnly = null, FXFlags flags = FXFlags.BindDirection)
        {
            return new Particle(_game, caster, bindObj, position, particle, size, bone, targetBone, 0, direction, followGroundTilt, lifetime, teamOnly, unitOnly, flags);
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
        /// <param name="teamOnly">The only team which should be able to see the particle.</param>
        /// <param name="flags">Flags which determine how the particle behaves. Refer to FXFlags enum.</param>
        /// <returns>New particle instance.</returns>
        public static Particle AddParticleTarget(GameObject caster, GameObject bindObj, string particle, GameObject target, float lifetime = 1.0f, float size = 1.0f, string bone = "", string targetBone = "", Vector3 direction = new Vector3(), bool followGroundTilt = false, TeamId teamOnly = TeamId.TEAM_NEUTRAL, GameObject unitOnly = null, FXFlags flags = FXFlags.BindDirection)
        {
            var p = new Particle(_game, caster, bindObj, target, particle, size, bone, targetBone, 0, direction, followGroundTilt, lifetime, teamOnly, unitOnly, flags);
            return p;
        }

        /// <summary>
        /// Removes the specified particle from ObjectManager and networks the removal to players.
        /// </summary>
        /// <param name="p">Particle to remove.</param>
        public static void RemoveParticle(Particle p)
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
        /// <param name="skinId">ID of the skin the minion should use for its model.</param>
        /// <param name="ignoreCollision">Whether or not the minion should ignore collisions.</param>
        /// <param name="targetable">Whether or not the minion should be targetable.</param>
        /// <param name="targetingFlags">Flags determining targetability.</param>
        /// <param name="visibilityOwner">Specifies the only unit which should be able to see the minion.</param>
        /// <param name="isVisible">Whether or not this minion should be visible.</param>
        /// <param name="aiPaused">Whether or not this minion's AI is inactive.</param>
        /// <returns>New Minion instance.</returns>
        public static Minion AddMinion
        (
            ObjAIBase owner,
            string model,
            string name,
            Vector2 position,
            TeamId team = TeamId.TEAM_NEUTRAL,
            int skinId = 0,
            bool ignoreCollision = false,
            bool targetable = true,
            bool isWard = false,
            SpellDataFlags targetingFlags = 0,
            ObjAIBase visibilityOwner = null,
            bool isVisible = true,
            bool aiPaused = true
        )
        {
            var m = new Minion(_game, owner, position, model, name, 0, team, skinId, ignoreCollision, targetable, isWard, visibilityOwner);
            m.Stats.IsTargetableToTeam = targetingFlags;
            _game.ObjectManager.AddObject(m);
            if (owner != null)
            {
                m.SetVisibleByTeam(owner.Team, isVisible);
            }
            m.PauseAI(aiPaused);
            return m;
        }

        /// <summary>
        /// Creates a stationary perception bubble at the given location.
        /// </summary>
        /// <param name="position">Position to spawn the perception bubble at.</param>
        /// <param name="radius">Size of the perception bubble.</param>
        /// <param name="duration">Number of seconds the perception bubble should exist.</param>
        /// <param name="team">Team the perception bubble should be owned by.</param>
        /// <param name="revealStealthed">Whether or not the perception bubble should reveal stealthed units while they are in range.</param>
        /// <param name="revealSpecificUnitOnly">Specific unit to reveal. Perception bubble will not reveal any other units when used. *NOTE* Currently does nothing.</param>
        /// <param name="collisionArea">Area around the perception bubble where units are not allowed to move into.</param>
        ///
        /// <returns>New Region instance.</returns>
        public static Region AddPosPerceptionBubble
        (
            Vector2 position,
            float radius,
            float duration,
            TeamId team = TeamId.TEAM_NEUTRAL,
            bool revealStealthed = false,
            AttackableUnit revealSpecificUnitOnly = null,
            float collisionArea = 0f,
            RegionType regionType = RegionType.Default
        )
        {
            return new Region
            (
                _game, team, position, regionType,
                visionTarget: revealSpecificUnitOnly,
                visionRadius: radius,
                revealStealth: revealStealthed,
                collisionRadius: collisionArea,
                lifetime: duration
            );
        }

        /// <summary>
        /// Creates a perception bubble which is attached to a specific unit.
        /// </summary>
        /// <param name="target">Unit to attach the perception bubble to.</param>
        /// <param name="radius">Size of the perception bubble.</param>
        /// <param name="duration">Number of seconds the perception bubble should exist.</param>
        /// <param name="team">Team the perception bubble should be owned by.</param>
        /// <param name="revealStealthed">Whether or not the perception bubble should reveal stealthed units while they are in range.</param>
        /// <param name="revealSpecificUnitOnly">Specific unit to reveal. Perception bubble will not reveal any other units when used. *NOTE* Currently does nothing.</param>
        /// <param name="collisionArea">Area around the perception bubble where units are not allowed to move into.</param>
        ///
        /// <returns>New Region instance.</returns>
        public static Region AddUnitPerceptionBubble
        (
            AttackableUnit target,
            float radius,
            float duration,
            TeamId team = TeamId.TEAM_NEUTRAL,
            bool revealStealthed = false,
            AttackableUnit revealSpecificUnitOnly = null,
            float collisionArea = 0f,
            RegionType regionType = RegionType.Default
        )
        {
            return new Region
            (
                _game, team, target.Position, regionType,
                collisionUnit: target,
                visionTarget: revealSpecificUnitOnly,
                visionRadius: radius,
                revealStealth: revealStealthed,
                collisionRadius: collisionArea,
                lifetime: duration
            );
        }

        /// <summary>
        /// Prints the specified string to the in-game chat.
        /// </summary>
        /// <param name="msg">String to print.</param>
        public static void PrintChat(string msg)
        {
            _game.PacketNotifier.NotifyS2C_SystemMessage(msg); // TODO: Move PacketNotifier usage to less abstract classes
        }

        /// <summary>
        /// Checks if the AttackableUnit is within the specified range of a target position.
        /// </summary>
        /// <param name="unit">Unit to check.</param>
        /// <param name="targetPos">Position to check from.</param>
        /// <param name="range">Range around the position to check.</param>
        /// <param name="isAlive">Whether or not the unit should be alive.</param>
        /// <returns></returns>
        public static bool IsUnitInRange(AttackableUnit unit, Vector2 targetPos, float range, bool isAlive)
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
        public static List<AttackableUnit> GetUnitsInRangeOld(Vector2 targetPos, float range, bool isAlive)
        {
            return _game.ObjectManager.GetUnitsInRange(targetPos, range, isAlive);
        }

        /// <summary>
        /// Acquires all dead or alive AttackableUnits within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <returns>List of AttackableUnits.</returns>
        public static IEnumerable<AttackableUnit> EnumerateUnitsInRange(Vector2 targetPos, float range, bool isAlive)
        {
            foreach (var obj in _game.Map.CollisionHandler.GetNearestObjects(new System.Activities.Presentation.View.Circle(targetPos, range)))
            {
                if (obj is AttackableUnit u && (!isAlive || !u.IsDead))
                {
                    yield return u;
                }
            }
        }

        /// <summary>
        /// Acquires all dead or alive AttackableUnits within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <returns>List of AttackableUnits.</returns>
        public static List<AttackableUnit> GetUnitsInRange(Vector2 targetPos, float range, bool isAlive)
        {
            var returnList = new List<AttackableUnit>();
            foreach (var obj in _game.Map.CollisionHandler.GetNearestObjects(new System.Activities.Presentation.View.Circle(targetPos, range)))
            {
                if (obj is AttackableUnit u && (!isAlive || !u.IsDead))
                {
                    returnList.Add(u);
                }
            }
            returnList.OrderBy(unit => Vector2.DistanceSquared(unit.Position, targetPos));
            return returnList;
        }

        /// <summary>
        /// Acquires the closest alive or dead AttackableUnit within the specified range of a target position.
        /// </summary>
        /// <param name="targetPos">Origin of the range to check.</param>
        /// <param name="range">Range to check from the target position.</param>
        /// <param name="isAlive">Whether or not to return alive AttackableUnits.</param>
        /// <returns>Closest AttackableUnit.</returns>
        public static AttackableUnit GetClosestUnitInRange(Vector2 targetPos, float range, bool isAlive)
        {
            var units = GetUnitsInRange(targetPos, range, isAlive);
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
        public static AttackableUnit GetClosestUnitInRange(AttackableUnit target, float range, bool isAlive)
        {
            var units = GetUnitsInRange(target.Position, range, isAlive);
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
        public static List<Champion> GetChampionsInRange(Vector2 targetPos, float range, bool isAlive)
        {
            return _game.ObjectManager.GetChampionsInRange(targetPos, range, isAlive);
        }

        /// <summary>
        /// Counts the number of units attacking a specified GameObject of type AttackableUnit.
        /// </summary>
        /// <param name="target">AttackableUnit potentially being attacked.</param>
        /// <returns>Number of units attacking target.</returns>
        public static int CountUnitsAttackingUnit(AttackableUnit target)
        {
            return _game.ObjectManager.CountUnitsAttackingUnit(target);
        }

        /// <summary>
        /// Returns a new list of all players in the game.
        /// Players are designated as clients, this includes bot champions.
        /// Currently only a single champion is designated to each player.
        /// </summary>
        /// <returns></returns>
        public static List<Champion> GetAllPlayers()
        {
            var toreturn = new List<Champion>();
            foreach (var player in _game.PlayerManager.GetPlayers(true))
            {
                toreturn.Add(player.Champion);
            }
            return toreturn;
        }

        //Consider changing this to take bots into account too
        public static List<Champion> GetAllPlayersFromTeam(TeamId team)
        {
            return _game.ObjectManager.GetAllChampionsFromTeam(team);
        }

        /// <summary>
        /// Returns a new list of all champions in the game.
        /// </summary>
        /// <returns></returns>
        public static List<Champion> GetAllChampions()
        {
            return _game.ObjectManager.GetAllChampions();
        }

        /// <summary>
        /// Instantly cancels any dashes the specified unit is performing.
        /// </summary>
        /// <param name="unit">Unit to stop dashing.</param>
        public static void CancelDash(AttackableUnit unit)
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
        /// <param name="consideredAsCC">Whether or not to prevent movement, casting, or attacking during the duration of the movement.</param>
        /// <param name="movementType">Type of force movement to perform. Refer to ForceMovementType enum.</param>
        /// <param name="movementOrdersType">How should the force movement affect the orders of the unit?</param>
        /// <param name="movementOrdersFacing">How should the force movement affect the facing direction of the unit?</param>
        /// TODO: Fully implement new ForceMovement functionality in AttackableUnit.
        public static void ForceMovement
        (
            AttackableUnit unit,
            string animation,
            Vector2 target,
            float speed,
            float idealDistance,
            float gravity,
            float moveBackBy,
            bool consideredAsCC = true,
            ForceMovementType movementType = ForceMovementType.FURTHEST_WITHIN_RANGE,
            ForceMovementOrdersType movementOrdersType = ForceMovementOrdersType.POSTPONE_CURRENT_ORDER,
            ForceMovementOrdersFacing movementOrdersFacing = ForceMovementOrdersFacing.FACE_MOVEMENT_DIRECTION)
        {
            var keepFacingLastDirection = false;
            if (movementOrdersFacing == ForceMovementOrdersFacing.KEEP_CURRENT_FACING)
            {
                keepFacingLastDirection = true;
            }
            unit.DashToLocation(target, speed, animation, gravity, keepFacingLastDirection, consideredAsCC);
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
        /// <param name="consideredAsCC">Whether or not to prevent movement, casting, or attacking during the duration of the movement.</param>
        /// <param name="movementType">Type of force movement to perform. Refer to ForceMovementType enum.</param>
        /// <param name="movementOrdersType">How should the force movement affect the orders of the unit?</param>
        /// <param name="movementOrdersFacing">How should the force movement affect the facing direction of the unit?</param>
        /// TODO: Fully implement new ForceMovement functionality in AttackableUnit.
        public static void ForceMovement
        (
            ObjAIBase unit,
            AttackableUnit target,
            string animation,
            float speed,
            float idealDistance,
            float gravity,
            float moveBackBy,
            float maxTravelTime,
            bool consideredAsCC = true,
            ForceMovementType movementType = ForceMovementType.FURTHEST_WITHIN_RANGE,
            ForceMovementOrdersType movementOrdersType = ForceMovementOrdersType.POSTPONE_CURRENT_ORDER,
            ForceMovementOrdersFacing movementOrdersFacing = ForceMovementOrdersFacing.FACE_MOVEMENT_DIRECTION)
        {
            var keepFacingLastDirection = false;
            if (movementOrdersFacing == ForceMovementOrdersFacing.KEEP_CURRENT_FACING)
            {
                keepFacingLastDirection = true;
            }
            unit.DashToTarget(target, speed, animation, gravity, keepFacingLastDirection, idealDistance, moveBackBy, maxTravelTime, consideredAsCC);
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
        public static void PlayAnimation(GameObject obj, string animName, float timeScale = 1.0f, float startTime = 0, float speedScale = 0, AnimationFlags flags = 0)
        {
            obj.PlayAnimation(animName, timeScale, startTime, speedScale, flags);
        }

        /// <summary>
        /// Forces the given object's current animations to pause/unpause.
        /// </summary>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        public static void PauseAnimation(GameObject obj, bool pause)
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
        public static void StopAnimation(GameObject obj, string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true)
        {
            obj.StopAnimation(animation, stopAll, fade, ignoreLock);
        }

        public static void SealSpellSlot(ObjAIBase target, SpellSlotType slotType, int slot, SpellbookType spellbookType, bool seal)
        {
            slot = ConvertAPISlot(spellbookType, slotType, slot);

            if (slot == -1)
            {
                return;
            }

            if (spellbookType == SpellbookType.SPELLBOOK_SUMMONER)
            {
                target.Stats.SetSummonerSpellEnabled((byte)slot, !seal);
                return;
            }

            target.Stats.SetSpellEnabled((byte)slot, !seal);
        }

        /// <summary>
        /// Overrides the given animation with the other given animation.
        /// First string is the animation to override, second string is the animation to play in place of the first.
        /// </summary>
        /// <param name="unit">Unit to set animation states on.</param>
        /// <param name="overrideAnim">Animation to use instead.</param>
        /// <param name="toOverrideAnim">Animation to override.</param>
        public static void OverrideAnimation(AttackableUnit unit, string overrideAnim, string toOverrideAnim)
        {
            unit.SetAnimStates(new Dictionary<string, string> { { toOverrideAnim, overrideAnim } });
        }

        /// <summary>
        /// Clears the given overridden animation, making it play its original animation.
        /// </summary>
        /// <param name="unit">Unit to set animation states on.</param>
        /// <param name="overriddenAnim">Animation which has been overridden.</param>
        public static void ClearOverrideAnimation(AttackableUnit unit, string overriddenAnim)
        {
            unit.SetAnimStates(new Dictionary<string, string> { { overriddenAnim, "" } });
        }

        /// <summary>
        /// Toggles the visual auto cast indicator for the given spell.
        /// </summary>
        /// <param name="spell">Spell to auto cast.</param>
        /// TODO: Verify if this works.
        public static void SetAutocast(Spell spell)
        {
            spell.SetAutocast();
        }

        /// <summary>
        /// Sets the state of the given status for the given unit.
        /// </summary>
        /// <param name="unit">Unit to set status.</param>
        /// <param name="status">Status to set.</param>
        /// <param name="enabled">Whether or not the status should be enabled.</param>
        public static void SetStatus(AttackableUnit unit, StatusFlags status, bool enabled)
        {
            unit.SetStatus(status, enabled);
        }

        /// <summary>
        /// Sets the given spell slot of the given unit to the spell of the given name.
        /// </summary>
        /// <param name="target">Unit to set the spell for.</param>
        /// <param name="newName">Name of the spell to place in the slot.</param>
        /// <param name="slotType">Type of slot being used.</param>
        /// <param name="slot">Index of the spell slot to change.</param>
        /// <param name="fullReplace">Whether or not the spell should be entirely replaced, or just the name. Typically used for transformations.</param>
        /// <returns>Newly created spell or existing spell with the given name. Null for failure.</returns>
        public static Spell SetSpell(ObjAIBase target, string newName, SpellSlotType slotType, int slot, bool fullReplace = false)
        {
            slot = ConvertAPISlot(slotType, slot);

            if (slot == -1)
            {
                return null;
            }

            return target.SetSpell(newName, (byte)slot, true, fullReplace);
        }

        /// <summary>
        /// Sets the targeting type of the spell in a given slot to a given targeting type.
        /// </summary>
        /// <param name="target">Unit to set the targeting type for.</param>
        /// <param name="slotType">Type of slot being used.</param>
        /// <param name="slot">Index of the spell slot to change.</param>
        /// <param name="newType">Targeting type to set.</param>
        public static void SetTargetingType(ObjAIBase target, SpellSlotType slotType, int slot, TargetingType newType)
        {
            slot = ConvertAPISlot(slotType, slot);

            if (slot == -1)
            {
                return;
            }

            var spell = target.Spells[(short)slot];

            spell.SpellData.SetTargetingType(newType);

            if (target is Champion champion)
            {
                _game.PacketNotifier.NotifyChangeSlotSpellData
                (
                    _game.PlayerManager.GetClientInfoByChampion(champion).ClientId,
                    target,
                    (byte)slot,
                    ChangeSlotSpellDataType.TargetingType,
                    targetingType: newType
                );
            }
        }

        public static void SetSpellToolTipVar<T>(AttackableUnit unit, int tipIndex, T value, SpellbookType book, byte slot, SpellSlotType slotType)
            where T : struct
        {
            if (unit is Champion champ)
            {
                slot = (byte)ConvertAPISlot(book, slotType, slot);

                champ.Spells[(short)slot].SetToolTipVar(tipIndex, value);
            }
        }

        public static void SetBuffToolTipVar<T>(Buff buff, int tipIndex, T value)
            where T : struct
        {
            buff.SetToolTipVar(tipIndex, value);
        }

        public static void SpellCast(ObjAIBase caster, int slot, SpellSlotType slotType, Vector2 pos, Vector2 endPos, bool fireWithoutCasting, Vector2 overrideCastPos, List<CastTarget> targets = null, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            slot = ConvertAPISlot(slotType, slot);

            Spell spell = caster.Spells[(short)slot];

            if (targets == null)
            {
                targets = new List<CastTarget> { new CastTarget(null, HitResult.HIT_Normal) };
            }

            CastInfo castInfo = new CastInfo()
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

        public static void SpellCast(ObjAIBase caster, int slot, SpellSlotType slotType, bool fireWithoutCasting, AttackableUnit target, Vector2 overrideCastPos, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            CastTarget castTarget = new CastTarget(target, CastTarget.GetHitResult(target, useAutoAttackSpell, caster.IsNextAutoCrit));

            SpellCast(caster, slot, slotType, target.Position, target.Position, fireWithoutCasting, overrideCastPos, new List<CastTarget> { castTarget }, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);
        }

        public static void SpellCastItem(ObjAIBase caster, string itemSpellName, bool fireWithoutCasting, AttackableUnit target, Vector2 overrideCastPos, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            // Apply the spell to the TempItemSlot.
            caster.SetSpell(itemSpellName, (byte)SpellSlotType.TempItemSlot, true);
            SpellCast(caster, 0, SpellSlotType.TempItemSlot, fireWithoutCasting, target, overrideCastPos, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);
        }

        public static void SpellCastItem(ObjAIBase caster, string itemSpellName, Vector2 pos, Vector2 endPos, bool fireWithoutCasting, Vector2 overrideCastPos, bool isForceCastingOrChanneling = false, int overrideForceLevel = -1, bool updateAutoAttackTimer = false, bool useAutoAttackSpell = false)
        {
            // Apply the spell to the TempItemSlot.
            caster.SetSpell(itemSpellName, (byte)SpellSlotType.TempItemSlot, true);
            SpellCast(caster, 0, SpellSlotType.TempItemSlot, pos, endPos, fireWithoutCasting, overrideCastPos, null, isForceCastingOrChanneling, overrideForceLevel, updateAutoAttackTimer, useAutoAttackSpell);
        }

        public static void StopChanneling(ObjAIBase target, ChannelingStopCondition stopCondition, ChannelingStopSource stopSource)
        {
            target.StopChanneling(stopCondition, stopSource);
        }

        /// <summary>
        /// Creates a DeathData variable for use with the AttackableUnit.Die() function.
        /// </summary>
        /// <param name="zombify">Whether or not the unit should become a zombie after death.</param>
        /// <param name="deathType">Type of death. Values unknown.</param>
        /// <param name="unit">Unit that died.</param>
        /// <param name="killer">Killer of the unit.</param>
        /// <param name="dmgType">Type of damage that caused the death.</param>
        /// <param name="dmgSource">Source of the damage that caused the death.</param>
        /// <param name="duration">Time until the death completes (fade-out?).</param>
        /// <returns></returns>
        public static DeathData CreateDeathData(bool zombify, byte deathType, AttackableUnit unit, AttackableUnit killer, DamageType dmgType, DamageSource dmgSource, float duration)
        {
            return new DeathData
            {
                BecomeZombie = zombify,
                DieType = deathType,
                Unit = unit,
                Killer = killer,
                DamageType = dmgType,
                DamageSource = dmgSource,
                DeathDuration = duration
            };
        }

        /// <summary>
        /// Returns whether or not the designed team has vision over an unit or not
        /// </summary>
        /// <param name="team"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool TeamHasVision(TeamId team, GameObject unit)
        {
            return _game.ObjectManager.TeamHasVisionOn(team, unit);
        }

        /// <summary>
        /// Returns wether or not an unit is protected from attacks (Monstly used to check tower protection)
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool UnitIsProtectionActive(AttackableUnit unit)
        {
            return _game.ProtectionManager.IsProtectionActive(unit);
        }

        /// <summary>
        /// Gets a list of waypoints, which forms a path to the desired destination
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="distanceThreshold"></param>
        /// <returns></returns>
        public static List<Vector2> GetPath(Vector2 from, Vector2 to, float distanceThreshold = 0)
        {
            return _game.Map.PathingHandler.GetPath(from, to, distanceThreshold);
        }

        public static void OverrideUnitAttackSpeedCap(AttackableUnit unit, bool doOverrideMax, float maxAttackSpeedOverride, bool doOverrideMin, float minAttackSpeedOverride)
        {
            //Investigate if we wanna update the stats here too
            _game.PacketNotifier.NotifyS2C_UpdateAttackSpeedCapOverrides(doOverrideMax, maxAttackSpeedOverride, doOverrideMin, minAttackSpeedOverride, unit);
        }

        public static ItemData GetItemData(int Id)
        {
            return _game.ItemManager.SafeGetItemType(Id);
        }

        public static void PlaySound(string soundName, AttackableUnit soundOwner)
        {
            _game.PacketNotifier.NotifyS2C_PlaySound(soundName, soundOwner);
        }

        public static void StopTargetingUnit(AttackableUnit unit)
        {
            _game.ObjectManager.StopTargeting(unit);
        }

        public static Pet CreatePet
        (
            Champion owner,
            Spell spell,
            Vector2 position,
            string name,
            string model,
            string buffName,
            float lifeTime,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            bool isClone = true,
            Stats stats = null,
            string aiScript = "Pet"

        )
        {
            return new Pet(_game, owner, spell, position, name, model, buffName, lifeTime, stats, cloneInventory, showMinimapIfClone, disallowPlayerControl, doFade, isClone, aiScript);
        }

        public static Pet CreateClonePet
        (
            Champion owner,
            Spell spell,
            ObjAIBase cloned,
            Vector2 position,
            string buffName,
            float lifeTime,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            Stats stats = null,
            string AIScript = "Pet"
        )
        {
            return new Pet(_game, owner, spell, cloned, position, buffName, lifeTime, stats, cloneInventory, showMinimapIfClone, disallowPlayerControl, doFade, AIScript);
        }

        public static float GetPetReturnRadius(Minion minion = null)
        {
            if (minion != null && minion is Pet pet)
            {
                return pet.GetReturnRadius();
            }

            return GlobalData.ObjAIBaseVariables.DefaultPetReturnRadius;
        }
    }
}
