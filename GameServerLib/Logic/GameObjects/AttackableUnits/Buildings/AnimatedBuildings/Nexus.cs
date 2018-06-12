using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Nexus : ObjAnimatedBuilding
    {
        public Nexus(
            string model,
            TeamId team,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(model, collisionRadius, x, y, visionRadius, netId)
        {
            Stats.Level1Health = 5500;
            Stats.IsTargetable = false;
            Stats.IsInvulnerable = true;

            SetTeam(team);
        }

        public override void die(ObjAIBase killer)
        {
            _game.Stop();
            _game.PacketNotifier.NotifyGameEnd(this);
        }

        public override void setToRemove()
        {

        }

        public override void UpdateReplication()
        {
            ReplicationManager.UpdateFloat(Stats.CurrentHealth, 1, 0);
            ReplicationManager.UpdateBool(Stats.IsInvulnerable, 1, 1);
            ReplicationManager.UpdateBool(Stats.IsTargetable, 5, 0);
            ReplicationManager.UpdateUint((uint)Stats.IsTargetableToTeam, 5, 1);
        }
    }
}
