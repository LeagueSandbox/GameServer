using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Other
{
    public class Placeable : ObjAiBase
    {
        public string Name { get; private set; }
        public ObjAiBase Owner { get; private set; } // We'll probably want to change this in the future

        public Placeable(
            Game game,
            ObjAiBase owner,
            float x,
            float y,
            string model,
            string name,
            uint netId = 0
        ) : base(game, model, new Stats.Stats(), 40, x, y, 0, netId)
        {
            SetTeam(owner.Team);

            Owner = owner;

            SetVisibleByTeam(Team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;

            Name = name;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifySpawn(this);
        }
    }
}
