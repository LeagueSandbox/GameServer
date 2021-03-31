using System;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Monster : Minion, IMonster
    {
        public Vector2 Facing { get; private set; }
        public string SpawnAnimation { get; private set; }
        public byte CampId { get; private set; }
        public byte CampUnk { get; private set; }
        public float SpawnAnimationTime { get; private set; }

        public Monster(
            Game game,
            Vector2 position,
            Vector2 facing,
            string model,
            string name,
            string spawnAnimation = "",
            byte campId = 0x01,
            byte campUnk = 0x2A,
            float spawnAnimationTime = 0.0f,
            uint netId = 0
        ) : base(game, null, position, model, name, netId)
        {
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
            {
                SetVisibleByTeam(team, true);
            }

            MoveOrder = OrderType.MoveTo;
            Facing = facing;
            Name = name;
            SpawnAnimation = spawnAnimation;
            CampId = campId;
            CampUnk = campUnk;
            SpawnAnimationTime = spawnAnimationTime;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifySpawn(this);
        }
    }
}
