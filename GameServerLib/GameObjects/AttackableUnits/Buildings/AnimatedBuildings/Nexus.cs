using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class Nexus : ObjAnimatedBuilding, INexus
    {
        public Nexus(
            Game game,
            string model,
            TeamId team,
            int collisionRadius = 40,
            Vector2 position = new Vector2(),
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, model, new Stats.Stats(), collisionRadius, position, visionRadius, netId, team)
        {
            Stats.CurrentHealth = 5500;
            Stats.HealthPoints.BaseValue = 5500;
        }

        public override void Die(IAttackableUnit killer)
        {
            //On SR the Z value was hardcoded to 188 for blue, 110 Purple, but it seemed fine with for both sides with only 110
            //Double check from where those values came from and if they're accurate.
            //I'll use 110 as default here just to keep it simple for now.
            var cameraPosition = new Vector3 (this.Position.X, this.Position.Y, 110);
            _game.Stop();
            _game.PacketNotifier.NotifyGameEnd(cameraPosition, this, _game.PlayerManager.GetPlayers());
            _game.SetGameToExit();
        }

        public override void SetToRemove()
        {

        }
    }
}
