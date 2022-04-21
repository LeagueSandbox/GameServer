using System.Collections.Generic;
using System.Numerics;
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
        /// Normally, objects will spawn at the end of the frame, but calling this function will force the teams' and players' vision of that object to update and send out a spawn notification.
        /// </summary>
        /// <param name="obj">Object to spawn.</param>
        void SpawnObject(IGameObject o);

        /// <summary>
        /// Gets a new Dictionary of all NetID,GameObject pairs present in the dictionary of objects in ObjectManager.
        /// </summary>
        /// <returns>Dictionary of NetIDs and the GameObjects that they refer to.</returns>
        Dictionary<uint, IGameObject> GetObjects();

        /// <summary>
        /// Gets a GameObject from the list of objects in ObjectManager that is identified by the specified NetID.
        /// </summary>
        /// <param name="id">NetID to check.</param>
        /// <returns>GameObject instance that has the specified NetID. Null otherwise.</returns>
        IGameObject GetObjectById(uint id);

        /// <summary>
        /// Removes a GameObject from the dictionary of GameObjects in ObjectManager.
        /// </summary>
        /// <param name="o">GameObject to remove.</param>
        void RemoveObject(IGameObject o);

        /// <summary>
        /// Adds a GameObject to the list of Vision Providers in ObjectManager.
        /// </summary>
        /// <param name="obj">GameObject to add.</param>
        /// <param name="team">The team that GameObject can provide vision to.</param>
        void AddVisionProvider(IGameObject obj, TeamId team);

        /// <summary>
        /// Removes a GameObject from the list of Vision Providers in ObjectManager.
        /// </summary>
        /// <param name="obj">GameObject to remove.</param>
        /// <param name="team">The team that GameObject provided vision to.</param>
        void RemoveVisionProvider(IGameObject obj, TeamId team);

        bool TeamHasVisionOn(TeamId team, IGameObject o);

        /// <summary>
        /// Gets a list of all GameObjects of type AttackableUnit that are within a certain distance from a specified position.
        /// </summary>
        /// <param name="checkPos">Vector2 position to check.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead units should be excluded or not.</param>
        /// <returns>List of all AttackableUnits within the specified range and of the specified alive status.</returns>
        List<IAttackableUnit> GetUnitsInRange(Vector2 checkPos, float range, bool onlyAlive = false);

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
        /// Adds a GameObject of type BaseTurret to the list of BaseTurrets in ObjectManager.
        /// </summary>
        /// <param name="turret">BaseTurret to add.</param>
        void AddTurret(IBaseTurret turret);

        /// <summary>
        /// Gets a GameObject of type BaseTurret from the list of BaseTurrets in ObjectManager who is identified by the specified NetID.
        /// </summary>
        /// <param name="netId"></param>
        /// <returns>BaseTurret instance identified by the specified NetID.</returns>
        IBaseTurret GetTurretById(uint netId);

        /// <summary>
        /// Removes a GameObject of type BaseTurret from the list of BaseTurrets in ObjectManager.
        /// </summary>
        /// <param name="turret">BaseTurret to remove.</param>
        void RemoveTurret(IBaseTurret turret);

        /// <summary>
        /// How many turrets of a specified team are destroyed in the specified lane.
        /// </summary>
        /// <param name="team">Team of the BaseTurrets to check.</param>
        /// <returns>true/false; destroyed or not</returns>
        int GetTurretsDestroyedForTeam(TeamId team, LaneID lane);

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
        /// Gets a list of all GameObjects of type Champion that are within a certain distance from a specified position.
        /// </summary>
        /// <param name="checkPos">Vector2 position to check.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead Champions should be excluded or not.</param>
        /// <returns>List of all Champions within the specified range of the position and of the specified alive status.</returns>
        List<IChampion> GetChampionsInRange(Vector2 checkPos, float range, bool onlyAlive = false);

        /// <summary>
        /// Gets a list of all GameObjects of type Champion from a specific team that are within a certain distance from a specified position.
        /// </summary>
        /// <param name="checkPos">Vector2 position to check.</param>
        /// <param name="range">Distance to check.</param>
        /// <param name="onlyAlive">Whether dead Champions should be excluded or not.</param>
        /// <returns>List of all Champions within the specified range of the position and of the specified alive status.</returns>
        List<IChampion> GetChampionsInRangeFromTeam(Vector2 checkPos, float range, TeamId team, bool onlyAlive = false);

        /// <summary>
        /// Removes a GameObject of type Champion from the list of Champions in ObjectManager.
        /// </summary>
        /// <param name="champion">Champion to remove.</param>
        void RemoveChampion(IChampion champion);
    }
}
