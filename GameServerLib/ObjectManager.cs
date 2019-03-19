using System;
using System.Collections.Generic;
using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;

namespace LeagueSandbox.GameServer
{
    // TODO: refactor this class
    public class ObjectManager : IObjectManager
    {
        private Game _game;

        private Dictionary<uint, IGameObject> _objects;
        private Dictionary<uint, IChampion> _champions;
        private Dictionary<uint, IInhibitor> _inhibitors;
        private Dictionary<TeamId, Dictionary<uint, IAttackableUnit>> _visionUnits;

        private object _objectsLock = new object();
        private object _inhibitorsLock = new object();
        private object _championsLock = new object();
        private object _visionLock = new object();

        public List<TeamId> Teams { get; private set; }

        public ObjectManager(Game game)
        {
            _game = game;
            _objects = new Dictionary<uint, IGameObject>();
            _inhibitors = new Dictionary<uint, IInhibitor>();
            _champions = new Dictionary<uint, IChampion>();
            _visionUnits = new Dictionary<TeamId, Dictionary<uint, IAttackableUnit>>();

            Teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            foreach (var team in Teams)
            {
                _visionUnits.Add(team, new Dictionary<uint, IAttackableUnit>());
            }
        }

        public void Update(float diff)
        {
            var temp = GetObjects();
            foreach (var obj in temp.Values)
            {
                if (obj.IsToRemove())
                {
                    RemoveObject(obj);
                    continue;
                }

                obj.Update(diff);

                if (!(obj is IAttackableUnit))
                    continue;

                var u = obj as IAttackableUnit;
                foreach (var team in Teams)
                {
                    if (u.Team == team || team == TeamId.TEAM_NEUTRAL)
                        continue;

                    var visionUnitsTeam = GetVisionUnits(u.Team);
                    if (visionUnitsTeam.ContainsKey(u.NetId))
                    {
                        if (TeamHasVisionOn(team, u))
                        {
                            u.SetVisibleByTeam(team, true);
                            _game.PacketNotifier.NotifySpawn(u);
                            RemoveVisionUnit(u);
                            // TODO: send this in one place only
                            _game.PacketNotifier.NotifyUpdatedStats(u, false);
                            continue;
                        }
                    }

                    if (!u.IsVisibleByTeam(team) && TeamHasVisionOn(team, u))
                    {
                        u.SetVisibleByTeam(team, true);
                        _game.PacketNotifier.NotifyEnterVision(u, team);
                        // TODO: send this in one place only
                        _game.PacketNotifier.NotifyUpdatedStats(u, false);
                    }
                    else if (u.IsVisibleByTeam(team) && !TeamHasVisionOn(team, u))
                    {
                        u.SetVisibleByTeam(team, false);
                        _game.PacketNotifier.NotifyLeaveVision(u, team);
                    }
                }

                var ai = u as IObjAiBase;
                if (ai != null)
                {
                    var tempBuffs = ai.GetBuffs();
                    foreach (var buff in tempBuffs.Values)
                    {
                        if (buff.Elapsed())
                        {
                            if (buff.Name != "")
                            {
                                _game.PacketNotifier.NotifyRemoveBuff(buff.TargetUnit, buff.Name, buff.Slot);
                            }

                            ai.RemoveBuff(buff);
                            continue;
                        }

                        buff.Update(diff);
                    }
                }

                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(u, false);

                if (u.IsModelUpdated)
                {
                    _game.PacketNotifier.NotifyModelUpdate(u);
                    u.IsModelUpdated = false;
                }

                if (obj.IsMovementUpdated())
                {
                    _game.PacketNotifier.NotifyMovement(obj);
                    obj.ClearMovementUpdated();
                }
            }
        }

        public IGameObject GetObjectById(uint id)
        {
            if (!_objects.ContainsKey(id))
            {
                return null;
            }

            return _objects[id];
        }

        public IInhibitor GetInhibitorById(uint id)
        {
            if (!_inhibitors.ContainsKey(id))
            {
                return null;
            }

            return _inhibitors[id];
        }

        public bool AllInhibitorsDestroyedFromTeam(TeamId team)
        {
            foreach (var inhibitor in _inhibitors.Values)
            {
                if (inhibitor.Team == team && inhibitor.InhibitorState == InhibitorState.ALIVE)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddObject(IGameObject o)
        {
            if (o == null)
                return;

            // If it crashes here the problem is most likely somewhere else
            lock (_objectsLock)
            {
                _objects.Add(o.NetId, o);
            }

            o.OnAdded();
        }

        public void AddInhibitor(IInhibitor inhib)
        {
            lock (_inhibitorsLock)
            {
                _inhibitors.Add(inhib.NetId, inhib);
            }
        }

        public void RemoveInhibitor(IInhibitor inhib)
        {
            lock (_inhibitorsLock)
            {
                _inhibitors.Remove(inhib.NetId);
            }
        }

        public void RemoveObject(IGameObject o)
        {
            lock (_objectsLock)
            {
                _objects.Remove(o.NetId);
            }

            o.OnRemoved();
        }

        public void AddChampion(IChampion champion)
        {
            lock (_championsLock)
            {
                _champions.Add(champion.NetId, champion);
            }
        }

        public void RemoveChampion(IChampion champion)
        {
            lock (_championsLock)
            {
                _champions.Remove(champion.NetId);
            }
        }

        public Dictionary<uint, IAttackableUnit> GetVisionUnits(TeamId team)
        {
            var ret = new Dictionary<uint, IAttackableUnit>();
            lock (_visionLock)
            {
                var visionUnitsTeam = _visionUnits[team];
                foreach (var unit in visionUnitsTeam)
                {
                    ret.Add(unit.Key, unit.Value);
                }

                return ret;
            }
        }

        public void AddVisionUnit(IAttackableUnit unit)
        {
            lock (_visionLock)
            {
                _visionUnits[unit.Team].Add(unit.NetId, unit);
            }
        }

        public void RemoveVisionUnit(IAttackableUnit unit)
        {
            RemoveVisionUnit(unit.Team, unit.NetId);
        }

        public void RemoveVisionUnit(TeamId team, uint netId)
        {
            lock (_visionLock)
            {
                _visionUnits[team].Remove(netId);
            }
        }

        public Dictionary<uint, IGameObject> GetObjects()
        {
            var ret = new Dictionary<uint, IGameObject>();
            lock (_objectsLock)
            {
                foreach (var obj in _objects)
                {
                    ret.Add(obj.Key, obj.Value);
                }
            }

            return ret;
        }

        public void StopTargeting(IAttackableUnit target)
        {
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as IAttackableUnit;
                    if (u == null)
                    {
                        continue;
                    }

                    var ai = u as IObjAiBase;
                    if (ai != null)
                    {
                        if (ai.TargetUnit == target)
                        {
                            ai.TargetUnit = null;
                            ai.AutoAttackTarget = null;
                            _game.PacketNotifier.NotifySetTarget(u, null);
                        }
                    }
                }
            }
        }

        public List<IChampion> GetAllChampionsFromTeam(TeamId team)
        {
            var champs = new List<IChampion>();
            foreach (var kv in _champions)
            {
                var c = kv.Value;
                if (c.Team == team)
                {
                    champs.Add(c);
                }
            }

            return champs;
        }

        public List<IChampion> GetChampionsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetChampionsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<IChampion> GetChampionsInRange(ITarget t, float range, bool onlyAlive = false)
        {
            var champs = new List<IChampion>();
            lock (_championsLock)
            {
                foreach (var kv in _champions)
                {
                    var c = kv.Value;
                    if (t.GetDistanceTo(c) <= range)
                        if (onlyAlive && !c.IsDead || !onlyAlive)
                            champs.Add(c);
                }
            }

            return champs;
        }

        public List<IAttackableUnit> GetUnitsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetUnitsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<IAttackableUnit> GetUnitsInRange(ITarget t, float range, bool onlyAlive = false)
        {
            var units = new List<IAttackableUnit>();
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    if (kv.Value is IAttackableUnit u && t.GetDistanceTo(u) <= range && (onlyAlive && !u.IsDead || !onlyAlive))
                    {
                        units.Add(u);
                    }
                }
            }

            return units;
        }

        public bool TeamHasVisionOn(TeamId team, IGameObject o)
        {
            if (o == null)
            {
                return false;
            }

            if (o.Team == team)
            {
                return true;
            }

            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    if (kv.Value.Team == team && kv.Value.GetDistanceTo(o) < kv.Value.VisionRadius &&
                        !_game.Map.NavGrid.IsAnythingBetween(kv.Value, o))
                    {
                        var unit = kv.Value as IAttackableUnit;
                        if (unit != null && unit.IsDead)
                        {
                            continue;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public int CountUnitsAttackingUnit(IAttackableUnit target)
        {
            return GetObjects().Count(x =>
                x.Value is IObjAiBase aiBase &&
                aiBase.Team == target.Team.GetEnemyTeam() &&
                !aiBase.IsDead &&
                aiBase.TargetUnit != null &&
                aiBase.TargetUnit == target
            );
        }
    }
}