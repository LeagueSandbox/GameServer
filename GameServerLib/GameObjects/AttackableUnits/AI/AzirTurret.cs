using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class AzirTurret : BaseTurret, IAzirTurret
    {
        public AttackableUnit Owner { get; private set; }

        IAttackableUnit IAzirTurret.Owner => Owner;

        public AzirTurret(
            Game game,
            AttackableUnit owner,
            Vector2 position,
            string name,
            string model,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0
        ) : base(game, position, name, model, team, netId)
        {
            Owner = owner;

            SetTeam(team);
            Stats.Range.BaseValue = 905.0f;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifySpawn(this);
        }

        public override void RefreshWaypoints()
        {
        }

        public override float GetMoveSpeed()
        {
            return 0;
        }
    }
}
