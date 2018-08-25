using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class ReplicationAnimatedBuilding : Replication
    {
        public ReplicationAnimatedBuilding(ObjAnimatedBuilding owner) : base(owner)
        {

        }
        public override void Update()
        {
            UpdateFloat(Stats.CurrentHealth, 1, 0); //mHP
            UpdateBool(Stats.IsInvulnerable, 1, 1); //IsInvulnerable
            UpdateBool(Stats.IsTargetable, 5, 0); //mIsTargetable
            UpdateUint((uint)Stats.IsTargetableToTeam, 5, 1); //mIsTargetableToTeamFlags
        }
    }
}
