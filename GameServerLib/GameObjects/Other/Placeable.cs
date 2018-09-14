using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class Placeable : ObjAiBase, IPlaceable
    {
        public string Name { get; private set; }
        public ObjAiBase Owner { get; private set; } // We'll probably want to change this in the future

        IObjAiBase IPlaceable.Owner => Owner;

        public Placeable(
            Game game,
            ObjAiBase owner,
            Vector2 position,
            string model,
            string name,
            uint netId = 0
        ) : base(game, position, model, new Stats.Stats(), 40, 0, netId)
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
