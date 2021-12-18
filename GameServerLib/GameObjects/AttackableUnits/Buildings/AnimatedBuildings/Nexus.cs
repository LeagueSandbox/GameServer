using GameServerCore.Domain;
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

        public override void Die(IDeathData data)
        {
            var players = _game.PlayerManager.GetPlayers();
            _game.Stop();
            _game.PacketNotifier.NotifyBuilding_Die(data);
            _game.PacketNotifier.NotifyS2C_EndGame(this, 5000.0f);
            foreach(var player in players)
            {
                _game.PacketNotifier.NotifyS2C_DisableHUDForEndOfGame(player);
                _game.PacketNotifier.NotifyS2C_MoveCameraToPoint(player, Vector3.Zero, new Vector3(this.Position.X, this.GetHeight(), this.Position.Y), 3.0f);
            }
            _game.SetGameToExit();
        }

        public override void SetToRemove()
        {

        }
    }
}
