using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using System.Linq;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class ObjAnimatedBuilding : ObjBuilding, IObjAnimatedBuilding
    {
        private readonly AttackableUnit[] _dependOnAll;
        private readonly AttackableUnit[] _dependOnSingle;

        private bool _hasProtection;

        public ObjAnimatedBuilding(Game game, string model, IStats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0, bool dependAll = false, params AttackableUnit[] dependOn) :
            base(game, model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            if (dependAll)
            {
                _dependOnAll = dependOn;
            }
            else
            {
                _dependOnSingle = dependOn;
            }

            Replication = new ReplicationAnimatedBuilding(this);
        }

        public ObjAnimatedBuilding(Game game, string model, IStats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0, AttackableUnit[] dependOnAll = null, AttackableUnit[] dependOnSingle = null) :
            base(game, model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            _dependOnAll = dependOnAll;
            _dependOnSingle = dependOnSingle;
            Replication = new ReplicationAnimatedBuilding(this);
        }

        public override void Update(float diff)
        {
            UpdateProtection();
            base.Update(diff);
            Replication.Update();
        }

        public void UpdateProtection()
        {
            if (_dependOnAll != null || _dependOnSingle != null)
            {
                int destroyedAllCount = 0;
                int destroyedSingleCount = 0;
                if (_dependOnAll != null)
                {
                    destroyedAllCount = _dependOnAll.Count(p => p.IsDead);
                }
                if (_dependOnSingle != null)
                {
                    destroyedSingleCount = _dependOnSingle.Count(p => p.IsDead);
                }

                if ((_dependOnAll == null || destroyedAllCount == _dependOnAll.Count()) && (destroyedSingleCount >= 1))
                {
                    if (_hasProtection)
                    {
                        SetIsTargetableToTeam(Team == TeamId.TEAM_BLUE ? TeamId.TEAM_PURPLE : TeamId.TEAM_BLUE, true);
                        _hasProtection = false;
                    }
                }
                else
                {
                    if (!_hasProtection)
                    {
                        SetIsTargetableToTeam(Team == TeamId.TEAM_BLUE ? TeamId.TEAM_PURPLE : TeamId.TEAM_BLUE, false);
                        _hasProtection = true;
                    }
                }
            }
        }
    }
}
