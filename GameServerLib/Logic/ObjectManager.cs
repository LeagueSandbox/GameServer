using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic
{
    public class ObjectManager
    {
        private Game _game;

        private Dictionary<uint, GameObject> _objects;
        private Dictionary<uint, Champion> _champions;
        private Dictionary<uint, Inhibitor> _inhibitors;
        private Dictionary<TeamId, Dictionary<uint, AttackableUnit>> _visionUnits;

        private object _objectsLock = new object();
        private object _inhibitorsLock = new object();
        private object _championsLock = new object();
        private object _visionLock = new object();

        public List<TeamId> Teams { get; private set; }

        public ObjectManager(Game game)
        {
            _game = game;
            _objects = new Dictionary<uint, GameObject>();
            _inhibitors = new Dictionary<uint, Inhibitor>();
            _champions = new Dictionary<uint, Champion>();
            _visionUnits = new Dictionary<TeamId, Dictionary<uint, AttackableUnit>>();

            Teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            foreach (var team in Teams)
                _visionUnits.Add(team, new Dictionary<uint, AttackableUnit>());
        }

        public void Update(float diff)
        {
            var temp = GetObjects();
            foreach (var obj in temp.Values)
            {
                if (obj.isToRemove())
                {
                    if (obj.AttackerCount == 0)
                        RemoveObject(obj);
                    continue;
                }

                obj.update(diff);

                if (!(obj is AttackableUnit))
                    continue;

                var u = obj as AttackableUnit;
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
                
                var ai = u as ObjAIBase;
                if (ai != null)
                {
                    var tempBuffs = ai.GetBuffs();
                    foreach (var buff in tempBuffs.Values)
                    {
                        if (buff.NeedsToRemove())
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

                if (obj.isMovementUpdated())
                {
                    _game.PacketNotifier.NotifyMovement(obj);
                    obj.clearMovementUpdated();
                }
            }
        }

        public GameObject GetObjectById(uint id)
        {
            if (!_objects.ContainsKey(id))
                return null;

            return _objects[id];
        }

        public Inhibitor GetInhibitorById(uint id)
        {
            if (!_inhibitors.ContainsKey(id))
                return null;

            return _inhibitors[id];
        }

        public bool AllInhibitorsDestroyedFromTeam(TeamId team)
        {
            foreach (var inhibitor in _inhibitors.Values)
            {
                if (inhibitor.Team == team && inhibitor.getState() == InhibitorState.Alive)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddObject(GameObject o)
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

        public void AddInhibitor(Inhibitor inhib)
        {
            lock (_inhibitorsLock)
            {
                _inhibitors.Add(inhib.NetId, inhib);
            }
        }

        public void RemoveInhibitor(Inhibitor inhib)
        {
            lock (_inhibitorsLock)
            {
                _inhibitors.Remove(inhib.NetId);
            }
        }

        public void RemoveObject(GameObject o)
        {
            lock (_objectsLock)
            {
                _objects.Remove(o.NetId);
            }
            o.OnRemoved();
        }

        public void AddChampion(Champion champion)
        {
            lock (_championsLock)
                _champions.Add(champion.NetId, champion);
        }

        public void RemoveChampion(Champion champion)
        {
            lock (_championsLock)
                _champions.Remove(champion.NetId);
        }

        public Dictionary<uint, AttackableUnit> GetVisionUnits(TeamId team)
        {
            var ret = new Dictionary<uint, AttackableUnit>();
            lock (_visionLock)
            {
                var visionUnitsTeam = _visionUnits[team];
                foreach (var unit in visionUnitsTeam)
                    ret.Add(unit.Key, unit.Value);

                return ret;
            }
        }

        public void AddVisionUnit(AttackableUnit unit)
        {
            lock (_visionLock)
            {
                _visionUnits[unit.Team].Add(unit.NetId, unit);
            }
        }

        public void RemoveVisionUnit(AttackableUnit unit)
        {
            RemoveVisionUnit(unit.Team, unit.NetId);
        }

        public void RemoveVisionUnit(TeamId team, uint netId)
        {
            lock (_visionLock)
                _visionUnits[team].Remove(netId);
        }

        public Dictionary<uint, GameObject> GetObjects()
        {
            var ret = new Dictionary<uint, GameObject>();
            lock (_objectsLock)
                foreach (var obj in _objects)
                    ret.Add(obj.Key, obj.Value);

            return ret;
        }

        public void StopTargeting(AttackableUnit target)
        {
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as AttackableUnit;
                    if (u == null)
                        continue;
                    var ai = u as ObjAIBase;
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

        public List<Champion> GetAllChampionsFromTeam(TeamId team)
        {
            var champs = new List<Champion>();
            foreach (var kv in _champions)
            {
                Champion c = kv.Value;
                if (c.Team == team)
                    champs.Add(c);
            }
            return champs;
        }

        public List<Champion> GetChampionsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetChampionsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<Champion> GetChampionsInRange(Target t, float range, bool onlyAlive = false)
        {
            var champs = new List<Champion>();
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

        public List<AttackableUnit> GetUnitsInRange(float x, float y, float range, bool onlyAlive = false)
        {
            return GetUnitsInRange(new Target(x, y), range, onlyAlive);
        }

        public List<AttackableUnit> GetUnitsInRange(Target t, float range, bool onlyAlive = false)
        {
            var units = new List<AttackableUnit>();
            lock (_objectsLock)
            {
                foreach (var kv in _objects)
                {
                    var u = kv.Value as AttackableUnit;
                    if (u != null && t.GetDistanceTo(u) <= range)
                        if ((onlyAlive && !u.IsDead) || !onlyAlive)
                            units.Add(u);
                }
            }
            return units;
        }

        public bool TeamHasVisionOn(TeamId team, GameObject o)
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
                        var unit = kv.Value as AttackableUnit;
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
    }
}