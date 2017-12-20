using LeagueSandbox.GameServer.Logic.Enet;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class AzirTurret : BaseTurret
    {
        public Unit Owner { get; private set; }

        public AzirTurret(
            Unit owner,
            string name,
            string model,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0
        ) : base(name, model, x, y, team, netId)
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
        public override void refreshWaypoints()
        {
        }

        public override float getMoveSpeed()
        {
            return 0;
        }
    }
}
