using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.GameObjects.Spells;
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
            if (unit.IsDashing)
            {
                CancelDash(unit);
            }
            unit.TeleportTo(x, y);
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
        public static IBuff AddBuff(string buffName, float duration, byte stacks, ISpell originspell, IObjAiBase onto, IObjAiBase from, bool infiniteduration = false)
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
        public static bool HasBuff(IObjAiBase unit, IBuff b)
        {
            return unit.HasBuff(b);
        }

        /// <summary>
        /// Whether or not the specified unit has a buff with the given name.
        /// </summary>
        /// <param name="unit">Unit to check.</param>
        /// <param name="b">Buff name to check for.</param>
        /// <returns>True/False</returns>
        public static bool HasBuff(IObjAiBase unit, string b)
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
        public static void RemoveBuff(IObjAiBase target, string buff)
        {
            target.RemoveBuffsWithName(buff);
        }

        /// <summary>
        /// Creates a new particle with the specified parameters.
        /// </summary>
        /// <param name="unit">AI unit that should own this particle.</param>
        /// <param name="particle">Internal name of the particle.</param>
        /// <param name="position">Position to spawn at.</param>
        /// <param name="size">Scale.</param>
        /// <param name="bone">Bone the particle should be attached to.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="reqVision">Whether or not the particle can be obstructed by terrain.</param>
        /// <returns>New particle instance.</returns>
        public static IParticle AddParticle(IObjAiBase unit, string particle, Vector2 position, float size = 1.0f, string bone = "", Vector3 direction = new Vector3(), float lifetime = 0, bool reqVision = true)
        {
            var p = new Particle(_game, unit, position, particle, size, bone, 0, direction, lifetime, reqVision);
            return p;
        }

        /// <summary>
        /// Creates a new particle with the specified parameters.
        /// This particle will be attached to a target.
        /// </summary>
        /// <param name="unit">Unit that should own this particle.</param>
        /// <param name="particle">Internal name of the particle.</param>
        /// <param name="target">GameObject to attach this particle to.</param>
        /// <param name="size">Scale.</param>
        /// <param name="bone">Bone the particle should be attached to.</param>
        /// <param name="direction">3D direction the particle should face.</param>
        /// <param name="lifetime">Time in seconds the particle should last.</param>
        /// <param name="reqVision">Whether or not the particle can be obstructed by terrain.</param>
        /// <returns>New particle instance.</returns>
        public static IParticle AddParticleTarget(IObjAiBase unit, string particle, IGameObject target, float size = 1.0f, string bone = "", Vector3 direction = new Vector3(), float lifetime = 0, bool reqVision = true)
        {
            var p = new Particle(_game, unit, target, particle, size, bone, 0, direction, lifetime, reqVision);
            return p;
        }

        /// <summary>
        /// Removes the specified particle from ObjectManager and networks the removal to players.
        /// </summary>
        /// <param name="p">Particle to remove.</param>
        public static void RemoveParticle(IParticle p)
        {
            p.SetToRemove();
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
        public static IMinion AddMinion(IObjAiBase owner, string model, string name, Vector2 position, bool isVisible = true, bool aiPaused = true)
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
            return AddMinion(owner, model, name, target.Position, isVisible, aiPaused);
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
        /// Forces the specified unit to face the specified 2D direction.
        /// </summary>
        /// <param name="unit">Unit to set.</param>
        /// <param name="direction">2D direction to face.</param>
        /// <param name="instant">Whether or not the unit should face the direction instantly.</param>
        /// <param name="turnTime">Time in seconds until the unit finishes turning towards the new direction.</param>
        public static void FaceDirection(IAttackableUnit unit, Vector2 direction, bool instant = true, float turnTime = 0.0833f)
        {
            _game.PacketNotifier.NotifyFaceDirection(unit, direction, instant, turnTime); // TODO: Move PacketNotifier usage to less abstract classes (in this case GameObject)
            // TODO: Change direction of actual GameObject
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

            // Reset the default run animation
            var animList = new List<string> { "RUN", "" };
            // TODO: Move PacketNotifier usage to less abstract classes (in this case ObjAiBase)
            _game.PacketNotifier.NotifySetAnimation(unit, animList);
        }

        /// <summary>
        /// Forces the specified AI unit to perform a dash which follows the specified AttackableUnit.
        /// </summary>
        /// <param name="unit">AI unit that is dashing.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="leapGravity">How much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="target">Unit to follow.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public static void DashToTarget
        (
            IObjAiBase unit,
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
            unit.DashToTarget(target, dashSpeed, animation, leapGravity, keepFacingLastDirection, followTargetMaxDistance, backDistance, travelTime);
        }

        /// <summary>
        /// Forces the specified unit to perform a dash which ends at the given position.
        /// </summary>
        /// <param name="unit">Unit that will perform the dash.</param>
        /// <param name="animation">Internal name of the dash animation.</param>
        /// <param name="endPos">Position to end the dash at.</param>
        /// <param name="dashSpeed">Amount of units the dash should travel in a second (movespeed).</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Whether or not the AI unit should face the direction they were facing before the dash.</param>
        /// TODO: Implement Dash class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public static void DashToLocation
        (
            IAttackableUnit unit,
            Vector2 endPos,
            float dashSpeed,
            string animation = "RUN",
            float leapGravity = 0.0f,
            bool keepFacingLastDirection = true
        )
        {
            unit.DashToLocation(endPos, dashSpeed, animation, leapGravity, keepFacingLastDirection);
        }
    }
}
