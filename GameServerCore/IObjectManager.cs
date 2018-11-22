using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore
{
    public interface IObjectManager: IUpdate
    {
        List<TeamId> Teams { get; }

        void AddChampion(IChampion champion);
        void AddInhibitor(IInhibitor inhib);
        void AddObject(IGameObject o);
        void AddVisionUnit(IAttackableUnit unit);
        bool AllInhibitorsDestroyedFromTeam(TeamId team);
        int CountUnitsAttackingUnit(IAttackableUnit target);
        List<IChampion> GetAllChampionsFromTeam(TeamId team);
        List<IChampion> GetChampionsInRange(float x, float y, float range, bool onlyAlive = false);
        List<IChampion> GetChampionsInRange(ITarget t, float range, bool onlyAlive = false);
        IInhibitor GetInhibitorById(uint id);
        IGameObject GetObjectById(uint id);
        Dictionary<uint, IGameObject> GetObjects();
        List<IAttackableUnit> GetUnitsInRange(float x, float y, float range, bool onlyAlive = false);
        List<IAttackableUnit> GetUnitsInRange(ITarget t, float range, bool onlyAlive = false);
        Dictionary<uint, IAttackableUnit> GetVisionUnits(TeamId team);
        void RemoveChampion(IChampion champion);
        void RemoveInhibitor(IInhibitor inhib);
        void RemoveObject(IGameObject o);
        void RemoveVisionUnit(IAttackableUnit unit);
        void RemoveVisionUnit(TeamId team, uint netId);
        void StopTargeting(IAttackableUnit target);
        bool TeamHasVisionOn(TeamId team, IGameObject o);
    }
}