using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

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
        ) : base(model, new Stats(), collisionRadius, x, y, visionRadius, netId)
        {
            Stats.CurrentHealth = 5500;
            Stats.HealthPoints.BaseValue = 5500;

            SetTeam(team);
        }

        public override void Die(AttackableUnit killer)
        {
            _game.Stop();
            _game.PacketNotifier.NotifyGameEnd(this);
        }

        public override void SetToRemove()
        {

        }

        public override float GetMoveSpeed()
        {
            return 0;
        }
    }
}
