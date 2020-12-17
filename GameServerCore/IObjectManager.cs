﻿using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore
{
    /// <summary>
    /// Interface of all variables and functions related to ObjectManager. ObjectManager manages addition, removal, and updating of all GameObjects, their visibility, and buffs.
    /// </summary>
    public interface IObjectManager: IUpdate
    {
        /// <summary>
        /// List of all possible teams in League of Legends. Normally there are only three.
        /// </summary>
        List<TeamId> Teams { get; }

        /// <summary>
        /// Adds a GameObject to the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to add.</param>
        void AddObject(IGameObject o);

        /// <summary>
        /// Gets a new Dictionary of all NetID,GameObject pairs present in the dictionary of objects in ObjectManager.
        /// </summary>
        /// <returns>Dictionary of NetIDs and the GameObjects that they refer to.</returns>
        Dictionary<uint, IGameObject> GetObjects();

        /// <summary>
        /// Gets a GameObject from the list of objects in ObjectManager that is identified by the specified NetID.
        /// </summary>
        /// <param name="netId">NetID to check.</param>
        /// <returns>GameObject instance that has the specified NetID.</returns>
        IGameObject GetObjectById(uint netId);

        /// <summary>
        /// Removes a GameObject from the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to remove.</param>
        void RemoveObject(IGameObject o);

        /// <summary>
        /// Adds a GameObject of type AttackableUnit to the list of Vision Units in ObjectManager. *NOTE*: Naming conventions of VisionUnits will change to AttackableUnits.
        /// </summary>
        /// <param name="unit">AttackableUnit to add.</param>
        void AddVisionUnit(IAttackableUnit unit);

        /// <summary>
        /// Gets a new Dictionary containing all GameObjects of type AttackableUnit of the specified team contained in the list of Vision Units in ObjectManager.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        /// <returns>Dictionary of NetID,AttackableUnit pairs that belong to the specified team.</returns>
        Dictionary<uint, IAttackableUnit> GetVisionUnits(TeamId team);

        /// <summary>
        /// Whether or not a specified GameObject is being networked to the specified team.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        /// <param name="o">GameObject to check.</param>
        /// <returns>true/false; networked or not.</returns>
        bool TeamHasVisionOn(TeamId team, IGameObject o);

        /// <summary>
        /// Removes a GameObject of type AttackableUnit from the list of Vision Units in ObjectManager. *NOTE*: Naming conventions of VisionUnits will change to AttackableUnits.
        /// </summary>
        /// <param name="unit">AttackableUnit to remove.</param>
        void RemoveVisionUnit(IAttackableUnit unit);

        /// <summary>
        /// Removes a GameObject of type AttackableUnit from the list of Vision Units in ObjectManager via the AttackableUnit's NetID and team.
        /// </summary>
        /// <param name="team">Team of the AttackableUnit.</param>
        /// <param name="netId">NetID of the AttackableUnit.</param>
        void RemoveVisionUnit(TeamId team, uint netId);

        /// <summary>
        /// Gets a list of all GameObjects of type AttackableUnit that are within a certain distance from a specified point.
        /// </summary>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point (Z-axis in 3D space).</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead units should be excluded or not.</param>
        /// <returns>List of all AttackableUnits within the specified range and of the specified alive status.</returns>
        List<IAttackableUnit> GetUnitsInRange(float x, float y, float range, bool onlyAlive = false);

        /// <summary>
        /// Gets a list of all GameObjects of type AttackableUnit that are within a certain distance from a specified Target. *NOTE*: Function will be depricated when Target class is removed.
        /// </summary>
        /// <param name="t">Target to check; could be a single point or an instance of a GameObject.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead units should be excluded or not.</param>
        /// <returns>List of all AttackableUnits within the specified range and of the specified alive status.</returns>
        List<IAttackableUnit> GetUnitsInRange(ITarget t, float range, bool onlyAlive = false);

        /// <summary>
        /// Counts the number of units attacking a specified GameObject of type AttackableUnit.
        /// </summary>
        /// <param name="target">AttackableUnit potentially being attacked.</param>
        /// <returns>Number of units attacking target.</returns>
        int CountUnitsAttackingUnit(IAttackableUnit target);

        /// <summary>
        /// Forces all GameObjects of type ObjAIBase to stop targeting the specified AttackableUnit.
        /// </summary>
        /// <param name="target">AttackableUnit that should be untargeted.</param>
        void StopTargeting(IAttackableUnit target);

        /// <summary>
        /// Adds a GameObject of type Inhibitor to the list of Inhibitors in ObjectManager.
        /// </summary>
        /// <param name="inhib">Inhibitor to add.</param>
        void AddInhibitor(IInhibitor inhib);

        /// <summary>
        /// Gets a GameObject of type Inhibitor from the list of Inhibitors in ObjectManager who is identified by the specified NetID.
        /// </summary>
        /// <param name="netId"></param>
        /// <returns>Inhibitor instance identified by the specified NetID.</returns>
        IInhibitor GetInhibitorById(uint netId);

        /// <summary>
        /// Removes a GameObject of type Inhibitor from the list of Inhibitors in ObjectManager.
        /// </summary>
        /// <param name="inhib">Inhibitor to remove.</param>
        void RemoveInhibitor(IInhibitor inhib);

        /// <summary>
        /// Whether or not all of the Inhibitors of a specified team are destroyed.
        /// </summary>
        /// <param name="team">Team of the Inhibitors to check.</param>
        /// <returns>true/false; destroyed or not</returns>
        bool AllInhibitorsDestroyedFromTeam(TeamId team);

        /// <summary>
        /// Adds a GameObject of type Champion to the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to add.</param>
        void AddChampion(IChampion champion);

        /// <summary>
        /// Gets a new list of all Champions found in the list of Champions in ObjectManager.
        /// </summary>
        /// <returns>List of all valid Champions.</returns>
        List<IChampion> GetAllChampions();

        /// <summary>
        /// Gets a new list of all Champions of the specified team found in the list of Champios in ObjectManager.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        /// <returns>List of valid Champions of the specified team.</returns>
        List<IChampion> GetAllChampionsFromTeam(TeamId team);

        /// <summary>
        /// Gets a new list of all Champions that are within the specified distance range of the specified point.
        /// </summary>
        /// <param name="x">X coordinate of the point.</param>
        /// <param name="y">Y coordinate of the point.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether to exclude dead Champions from the list.</param>
        /// <returns>List of all Champions within the specified range and of the specified alive status.</returns>
        List<IChampion> GetChampionsInRange(float x, float y, float range, bool onlyAlive = false);

        /// <summary>
        /// Gets a list of all GameObjects of type Champion that are within a certain distance from a specified Target. *NOTE*: Function will be depricated when Target class is removed.
        /// </summary>
        /// <param name="t">Target to check; could be a single point or an instance of a GameObject.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead Champions should be excluded or not.</param>
        /// <returns>List of all Champions within the specified range and of the specified alive status.</returns>
        List<IChampion> GetChampionsInRange(ITarget t, float range, bool onlyAlive = false);

        /// <summary>
        /// Removes a GameObject of type Champion from the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to remove.</param>
        void RemoveChampion(IChampion champion);
    }
}