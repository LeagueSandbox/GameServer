using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class Nexus : ObjAnimatedBuilding
    {
        public Nexus(
            Game game,
            string model,
            TeamId team,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, model, new Stats.Stats(), collisionRadius, x, y, visionRadius, netId)
        {
            Stats.CurrentHealth = 5500;
            Stats.HealthPoints.BaseValue = 5500;

            SetTeam(team);
        }

        public override void Die(AttackableUnit killer)
        {
            var cameraPosition = _game.Map.MapGameScript.GetEndGameCameraPosition(Team);
            _game.Stop();
            _game.PacketNotifier.NotifyGameEnd(cameraPosition, this);
            _game.SetGameToExit();
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
