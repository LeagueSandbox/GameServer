using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class AzirTurret : BaseTurret
    {
        public AttackableUnit Owner { get; private set; }

        public AzirTurret(
            Game game,
            AttackableUnit owner,
            string name,
            string model,
            Vector2 position,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0,
            Lane lane = Lane.LANE_Unknown
        ) : base(game, name, model, position, team, netId, lane)
        {
            Owner = owner;

            SetTeam(team);
            Stats.Range.BaseValue = 905.0f;
        }
    }
}
